// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// Post effect base class.
    /// </summary>
    [DataContract]
    public abstract class ImageEffect : DrawEffect, IImageEffect
    {
        private readonly Texture[] inputTextures;
        private int maxInputTextureIndex;

        private Texture outputRenderTargetView;
        private Texture[] outputRenderTargetViews;
        private Texture[] createdOutputRenderTargetViews;

        private Viewport? viewport;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffect" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="System.ArgumentNullException">context</exception>
        protected ImageEffect(string name)
            : base(name)
        {
            inputTextures = new Texture[128];
            maxInputTextureIndex = -1;
            EnableSetRenderTargets = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffect" /> class.
        /// </summary>
        protected ImageEffect()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffect" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="name">The name.</param>
        /// <exception cref="System.ArgumentNullException">context</exception>
        protected ImageEffect(RenderContext context, string name = null) 
            : this(name)
        {
            Initialize(context);
        }

        /// <summary>
        /// Gets or sets a boolean to enable GraphicsDevice.SetDepthAndRenderTargets from output. Default is <c>true</c>.
        /// </summary>
        /// <value>A boolean to enable GraphicsDevice.SetDepthAndRenderTargets from output. Default is <c>true</c></value>
        protected bool EnableSetRenderTargets { get; set; }
        
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
        public override void Reset()
        {
            base.Reset();

            maxInputTextureIndex = -1;
            Array.Clear(inputTextures, 0, inputTextures.Length);
            outputRenderTargetView = null;
            outputRenderTargetViews = null;
        }

        /// <summary>
        /// Sets the render target output.
        /// </summary>
        /// <param name="view">The render target output view.</param>
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
        /// Sets the viewport to use .
        /// </summary>
        /// <param name="viewport">The viewport.</param>
        public void SetViewport(Viewport? viewport)
        {
            this.viewport = viewport; // TODO: support multiple viewport?
        }

        protected override void PreDrawCore(RenderDrawContext context)
        {
            base.PreDrawCore(context);

            if (EnableSetRenderTargets)
            {
                SetRenderTargets(context);
            }
        }

        /// <summary>
        /// Set the render targets for the image effect.
        /// </summary>
        /// <param name="context"></param>
        protected virtual void SetRenderTargets(RenderDrawContext context)
        {
            // Transtion inputs to read sources
            for (int i = 0; i <= maxInputTextureIndex; ++i)
            {
                if (inputTextures[i] != null)
                    context.CommandList.ResourceBarrierTransition(inputTextures[i], GraphicsResourceState.PixelShaderResource);
            }

            if (outputRenderTargetView != null)
            {
                // Transition render target
                context.CommandList.ResourceBarrierTransition(outputRenderTargetView, GraphicsResourceState.RenderTarget);

                if (outputRenderTargetView.Dimension == TextureDimension.TextureCube)
                {
                    if(createdOutputRenderTargetViews == null)
                        createdOutputRenderTargetViews = new Texture[6];

                    for (int i = 0; i < createdOutputRenderTargetViews.Length; i++)
                        createdOutputRenderTargetViews[i] = outputRenderTargetView.ToTextureView(ViewType.Single, i, 0);

                    context.CommandList.SetRenderTargetsAndViewport(createdOutputRenderTargetViews);

                    if (viewport.HasValue)
                    {
                        for (int i = 0; i < createdOutputRenderTargetViews.Length; i++)
                        {
                            context.CommandList.SetViewport(i, viewport.Value);
                        }
                    }
                }
                else
                {
                    context.CommandList.SetRenderTargetAndViewport(null, outputRenderTargetView);
                    if (viewport.HasValue)
                    {
                        context.CommandList.SetViewport(viewport.Value);
                    }
                }
            }
            else if (outputRenderTargetViews != null)
            {
                // Transition render targets
                foreach (var renderTarget in outputRenderTargetViews)
                    context.CommandList.ResourceBarrierTransition(renderTarget, GraphicsResourceState.RenderTarget);

                context.CommandList.SetRenderTargetsAndViewport(outputRenderTargetViews);

                if (viewport.HasValue)
                {
                    for (int i = 0; i < outputRenderTargetViews.Length; i++)
                    {
                        context.CommandList.SetViewport(i, viewport.Value);
                    }
                }
            }
        }

        protected override void PostDrawCore(RenderDrawContext context)
        {
            if (EnableSetRenderTargets)
            {
                DisposeCreatedRenderTargetViews(context);
            }

            base.PostDrawCore(context);
        }

        /// <summary>
        /// Dispose the render target views that have been created.
        /// </summary>
        protected virtual void DisposeCreatedRenderTargetViews(RenderDrawContext context)
        {
            if(createdOutputRenderTargetViews == null)
                return;

            for (int i = 0; i < createdOutputRenderTargetViews.Length; i++)
            {
                createdOutputRenderTargetViews[i].Dispose();
                createdOutputRenderTargetViews[i] = null;
            }
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
        /// Gets a render target with the specified description, scoped for the duration of the <see cref="DrawEffect.DrawCore"/>.
        /// </summary>
        /// <returns>A new instance of texture.</returns>
        protected Texture NewScopedRenderTarget2D(TextureDescription description)
        {
            // TODO: Check if we should introduce an enum for the kind of scope (per DrawCore, per Frame...etc.)
            CheckIsInDrawCore();
            return PushScopedResource(Context.Allocator.GetTemporaryTexture2D(description));
        }

        /// <summary>
        /// Gets a render target output for the specified description with a single mipmap, scoped for the duration of the <see cref="DrawEffect.DrawCore"/>.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="flags">Sets the texture flags (for unordered access...etc.)</param>
        /// <param name="arraySize">Size of the texture 2D array, default to 1.</param>
        /// <returns>A new instance of texture class.</returns>
        /// <msdn-id>ff476521</msdn-id>
        ///   <unmanaged>HRESULT ID3D11Device::CreateTexture2D([In] const D3D11_TEXTURE2D_DESC* pDesc,[In, Buffer, Optional] const D3D11_SUBRESOURCE_DATA* pInitialData,[Out, Fast] ID3D11Texture2D** ppTexture2D)</unmanaged>
        ///   <unmanaged-short>ID3D11Device::CreateTexture2D</unmanaged-short>
        protected Texture NewScopedRenderTarget2D(int width, int height, PixelFormat format, TextureFlags flags = TextureFlags.RenderTarget | TextureFlags.ShaderResource, int arraySize = 1)
        {
            CheckIsInDrawCore();
            return PushScopedResource(Context.Allocator.GetTemporaryTexture2D(width, height, format, flags, arraySize));
        }

        /// <summary>
        /// Gets a render target output for the specified description, scoped for the duration of the <see cref="DrawEffect.DrawCore"/>.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="mipCount">Number of mipmaps, set to true to have all mipmaps, set to an int &gt;=1 for a particular mipmap count.</param>
        /// <param name="flags">Sets the texture flags (for unordered access...etc.)</param>
        /// <param name="arraySize">Size of the texture 2D array, default to 1.</param>
        /// <returns>A new instance of texture class.</returns>
        /// <msdn-id>ff476521</msdn-id>
        ///   <unmanaged>HRESULT ID3D11Device::CreateTexture2D([In] const D3D11_TEXTURE2D_DESC* pDesc,[In, Buffer, Optional] const D3D11_SUBRESOURCE_DATA* pInitialData,[Out, Fast] ID3D11Texture2D** ppTexture2D)</unmanaged>
        ///   <unmanaged-short>ID3D11Device::CreateTexture2D</unmanaged-short>
        protected Texture NewScopedRenderTarget2D(int width, int height, PixelFormat format, MipMapCount mipCount, TextureFlags flags = TextureFlags.RenderTarget | TextureFlags.ShaderResource, int arraySize = 1)
        {
            CheckIsInDrawCore();
            return PushScopedResource(Context.Allocator.GetTemporaryTexture2D(width, height, format, mipCount, flags, arraySize));
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