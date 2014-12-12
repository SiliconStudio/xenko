// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Post effect base class.
    /// </summary>
    public abstract class ImageEffect : ComponentBase
    {
        private readonly Texture[] inputTextures;
        private int maxInputTextureIndex;

        private Texture outputRenderTargetView;

        private Texture[] outputRenderTargetViews;

        private bool isInDrawCore;

        private readonly List<Texture> scopedRenderTargets;

        private ImageScaler scaler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffect" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="name">The name.</param>
        /// <exception cref="System.ArgumentNullException">context</exception>
        protected ImageEffect(ImageEffectContext context, string name = null) : base(name)
        {
            if (context == null) throw new ArgumentNullException("context");

            Context = context;
            GraphicsDevice = Context.GraphicsDevice;
            Assets = context.Services.GetSafeServiceAs<AssetManager>();
            Enabled = true;
            inputTextures = new Texture[128];
            scopedRenderTargets = new List<Texture>();
            maxInputTextureIndex = -1;
            EnableSetRenderTargets = true;
            Parameters = new ParameterCollection();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this post effect is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        public ImageEffectContext Context { get; private set; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public ParameterCollection Parameters { get; private set; }

        /// <summary>
        /// Gets the <see cref="AssetManager"/>.
        /// </summary>
        /// <value>The content.</value>
        protected AssetManager Assets { get; private set; }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        protected GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets a boolean to enable GraphicsDevice.SetDepthAndRenderTargets from output. Default is <c>true</c>.
        /// </summary>
        /// <value>A boolean to enable GraphicsDevice.SetDepthAndRenderTargets from output. Default is <c>true</c></value>
        protected bool EnableSetRenderTargets { get; set; }

        /// <summary>
        /// Gets a shared <see cref="ImageScaler"/>.
        /// </summary>
        protected ImageScaler Scaler
        {
            get
            {
                return scaler ?? (scaler = Context.GetSharedEffect<ImageScaler>());
            }
        }

        /// <summary>
        /// Sets an input texture
        /// </summary>
        /// <param name="slot">The slot.</param>
        /// <param name="texture">The texture.</param>
        public void SetInput(int slot, Texture texture)
        {
            if (slot < 0 || slot >= inputTextures.Length)
                throw new ArgumentOutOfRangeException("slot", "slot must be in the range [0, 128[");

            inputTextures[slot] = texture;
            if (slot > maxInputTextureIndex)
            {
                maxInputTextureIndex = slot;
            }
        }

        /// <summary>
        /// Resets the state of this effect.
        /// </summary>
        public virtual void Reset()
        {
            maxInputTextureIndex = -1;
            Array.Clear(inputTextures, 0, inputTextures.Length);
            outputRenderTargetView = null;
            outputRenderTargetViews = null;
            SetDefaultParameters();
        }

        /// <summary>
        /// Sets the default parameters (called at constructor time and if <see cref="Reset"/> is called)
        /// </summary>
        protected virtual void SetDefaultParameters()
        {
        }

        /// <summary>
        /// Sets the render target output.
        /// </summary>
        /// <param name="view">The render target output view.</param>
        /// <param name="depthStencilBuffer">The depth stencil buffer.</param>
        /// <exception cref="System.ArgumentNullException">view</exception>
        public void SetOutput(Texture view)
        {
            if (view == null) throw new ArgumentNullException("view");

            SetOutputInternal(view);
        }

        /// <summary>
        /// Sets the render target outputs.
        /// </summary>
        /// <param name="views">The render target output views.</param>
        public void SetOutput(params Texture[] views)
        {
            if (views == null) throw new ArgumentNullException("views");

            SetOutputInternal(views);
        }

        /// <summary>
        /// Draws a full screen quad using iterating on each pass of this effect.
        /// </summary>
        public void Draw(string name = null)
        {
            if (!Enabled)
            {
                return;
            }

            PreDrawCore(name);

            // Allow scoped allocation RenderTargets
            isInDrawCore = true;
            DrawCore();
            isInDrawCore = false;

            // Release scoped RenderTargets
            ReleaseAllScopedRenderTarget2D();

            PostDrawCore();
        }

        /// <summary>
        /// Draws a full screen quad using iterating on each pass of this effect.
        /// </summary>
        public void Draw(string nameFormat, params object[] args)
        {
            // TODO: this is alocating a string, we should try to not allocate here.
            Draw(string.Format(nameFormat, args));
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("Effect {0}", Name);
        }

        /// <summary>
        /// Prepare call before <see cref="DrawCore"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        protected virtual void PreDrawCore(string name)
        {
            GraphicsDevice.BeginProfile(Color.Green, name ?? Name);

            if (EnableSetRenderTargets)
            {
                if (outputRenderTargetView != null)
                {
                    GraphicsDevice.SetRenderTarget(outputRenderTargetView);
                }
                else if (outputRenderTargetViews != null)
                {
                    GraphicsDevice.SetRenderTargets(outputRenderTargetViews);
                }
            }
        }

        /// <summary>
        /// Posts call after <see cref="DrawCore"/>
        /// </summary>
        protected virtual void PostDrawCore()
        {
            GraphicsDevice.EndProfile();
        }

        /// <summary>
        /// Draws this post effect for a specific pass, implementation dependent.
        /// </summary>
        protected virtual void DrawCore()
        {
        }

        /// <summary>
        /// Gets the number of input textures.
        /// </summary>
        /// <value>The input count.</value>
        protected int InputCount
        {
            get
            {
                return maxInputTextureIndex + 1;
            }
        }

        /// <summary>
        /// Gets an input texture by the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Texture.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">index</exception>
        protected Texture GetInput(int index)
        {
            if (index < 0 || index > maxInputTextureIndex)
            {
                throw new ArgumentOutOfRangeException("index", string.Format("Invald texture input index [{0}]. Max value is [{1}]", index, maxInputTextureIndex));
            }
            return inputTextures[index];
        }

        /// <summary>
        /// Gets a non-null input texture by the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Texture.</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        protected Texture GetSafeInput(int index)
        {
            var input = GetInput(index);
            if (input == null)
            {
                throw new InvalidOperationException(string.Format("Expecting texture input on slot [{0}]", index));
            }

            return input;
        }

        /// <summary>
        /// Gets the number of output render target.
        /// </summary>
        /// <value>The output count.</value>
        protected int OutputCount
        {
            get
            {
                return outputRenderTargetView != null ? 1 : outputRenderTargetViews != null ? outputRenderTargetViews.Length : 0;
            }
        }

        /// <summary>
        /// Gets an output render target for the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>RenderTarget.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">index</exception>
        protected Texture GetOutput(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", string.Format("Invald texture outputindex [{0}] cannot be negative for effect [{1}]", index, Name));
            }

            return outputRenderTargetView ?? (outputRenderTargetViews != null ? outputRenderTargetViews[index] : null);
        }

        /// <summary>
        /// Gets an non-null output render target for the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>RenderTarget.</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        protected Texture GetSafeOutput(int index)
        {
            var output = GetOutput(index);
            if (output == null)
            {
                throw new InvalidOperationException(string.Format("Expecting texture output on slot [{0}]", index));
            }

            return output;
        }

        /// <summary>
        /// Gets a <see cref="RenderTarget" /> with the specified description, scoped for the duration of the <see cref="DrawCore"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="RenderTarget" /> class.</returns>
        protected Texture NewScopedRenderTarget2D(TextureDescription description)
        {
            // TODO: Check if we should introduce an enum for the kind of scope (per DrawCore, per Frame...etc.)
            CheckIsInDrawCore();
            return PushScopedRenderTarget2D(Context.Allocator.GetTemporaryTexture2D(description));
        }

        /// <summary>
        /// Gets a <see cref="RenderTarget" /> output for the specified description with a single mipmap, scoped for the duration of the <see cref="DrawCore"/>.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="flags">Sets the texture flags (for unordered access...etc.)</param>
        /// <param name="arraySize">Size of the texture 2D array, default to 1.</param>
        /// <returns>A new instance of <see cref="RenderTarget" /> class.</returns>
        /// <msdn-id>ff476521</msdn-id>
        ///   <unmanaged>HRESULT ID3D11Device::CreateTexture2D([In] const D3D11_TEXTURE2D_DESC* pDesc,[In, Buffer, Optional] const D3D11_SUBRESOURCE_DATA* pInitialData,[Out, Fast] ID3D11Texture2D** ppTexture2D)</unmanaged>
        ///   <unmanaged-short>ID3D11Device::CreateTexture2D</unmanaged-short>
        protected Texture NewScopedRenderTarget2D(int width, int height, PixelFormat format, TextureFlags flags = TextureFlags.RenderTarget | TextureFlags.ShaderResource, int arraySize = 1)
        {
            CheckIsInDrawCore();
            return PushScopedRenderTarget2D(Context.Allocator.GetTemporaryTexture2D(width, height, format, flags, arraySize));
        }

        /// <summary>
        /// Gets a <see cref="RenderTarget" /> output for the specified description, scoped for the duration of the <see cref="DrawCore"/>.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="mipCount">Number of mipmaps, set to true to have all mipmaps, set to an int &gt;=1 for a particular mipmap count.</param>
        /// <param name="flags">Sets the texture flags (for unordered access...etc.)</param>
        /// <param name="arraySize">Size of the texture 2D array, default to 1.</param>
        /// <returns>A new instance of <see cref="RenderTarget" /> class.</returns>
        /// <msdn-id>ff476521</msdn-id>
        ///   <unmanaged>HRESULT ID3D11Device::CreateTexture2D([In] const D3D11_TEXTURE2D_DESC* pDesc,[In, Buffer, Optional] const D3D11_SUBRESOURCE_DATA* pInitialData,[Out, Fast] ID3D11Texture2D** ppTexture2D)</unmanaged>
        ///   <unmanaged-short>ID3D11Device::CreateTexture2D</unmanaged-short>
        protected Texture NewScopedRenderTarget2D(int width, int height, PixelFormat format, MipMapCount mipCount, TextureFlags flags = TextureFlags.RenderTarget | TextureFlags.ShaderResource, int arraySize = 1)
        {
            CheckIsInDrawCore();
            return PushScopedRenderTarget2D(Context.Allocator.GetTemporaryTexture2D(width, height, format, mipCount, flags, arraySize));
        }

        private void CheckIsInDrawCore()
        {
            if (!isInDrawCore)
            {
                throw new InvalidOperationException("The method NewScopedRenderTarget2D can only be called within a DrawCore operation");
            }
        }

        private Texture PushScopedRenderTarget2D(Texture renderTarget)
        {
            scopedRenderTargets.Add(renderTarget);
            return renderTarget;
        }

        private void ReleaseAllScopedRenderTarget2D()
        {
            foreach (var scopedRenderTarget2D in scopedRenderTargets)
            {
                Context.Allocator.ReleaseReference(scopedRenderTarget2D);
            }
            scopedRenderTargets.Clear();
        }

        private void SetOutputInternal(Texture view)
        {
            // TODO: Do we want to handle the output the same way we handle the input textures?
            outputRenderTargetView = view;
            outputRenderTargetViews = null;
        }

        private void SetOutputInternal(params Texture[] views)
        {
            // TODO: Do we want to handle the output the same way we handle the input textures?
            outputRenderTargetView = null;
            outputRenderTargetViews = views;
        }
    }
}