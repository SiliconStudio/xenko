// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Context for post effects.
    /// </summary>
    public class ImageEffectContext : ComponentBase
    {
        public TextureDescription DefaultTextureDescription;
        private readonly Dictionary<TextureDescription, List<TextureLink>> textureCache = new Dictionary<TextureDescription, List<TextureLink>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffectContext" /> class.
        /// </summary>
        /// <param name="game">The game.</param>
        public ImageEffectContext(Game game)
            : this(game.Services)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffectContext" /> class.
        /// </summary>
        /// <param name="serviceRegistry">The service registry.</param>
        public ImageEffectContext(IServiceRegistry serviceRegistry)
        {
            Services = serviceRegistry;
            Effects = serviceRegistry.GetSafeServiceAs<EffectSystem>();
            GraphicsDevice = serviceRegistry.GetSafeServiceAs<IGraphicsDeviceService>().GraphicsDevice;
        }

        /// <summary>
        /// Gets the content manager.
        /// </summary>
        /// <value>The content manager.</value>
        public EffectSystem Effects { get; private set; }

        /// <summary>
        /// Gets or sets the graphics device.
        /// </summary>
        /// <value>The graphics device.</value>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the services registry.
        /// </summary>
        /// <value>The services registry.</value>
        public IServiceRegistry Services { get; private set; }

        /// <summary>
        /// Gets a <see cref="RenderTarget" /> output for the specified description.
        /// </summary>
        /// <returns>A new instance of <see cref="RenderTarget" /> class.</returns>
        public RenderTarget GetTemporaryRenderTarget2D(TextureDescription description)
        {
            return GetTemporaryTexture(description).ToRenderTarget();
        }

        /// <summary>
        /// Gets a <see cref="RenderTarget" /> output for the specified description with a single mipmap.
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
        public RenderTarget GetTemporaryRenderTarget2D(int width, int height, PixelFormat format, TextureFlags flags = TextureFlags.RenderTarget | TextureFlags.ShaderResource, int arraySize = 1)
        {
            return GetTemporaryTexture(Texture2DBase.NewDescription(width, height, format, flags, 1, arraySize, GraphicsResourceUsage.Default)).ToRenderTarget();
        }

        /// <summary>
        /// Gets a <see cref="RenderTarget" /> output for the specified description.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="mipCount">Number of mipmaps, set to true to have all mipmaps, set to an int &gt;=1 for a particular mipmap count.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="flags">Sets the texture flags (for unordered access...etc.)</param>
        /// <param name="arraySize">Size of the texture 2D array, default to 1.</param>
        /// <returns>A new instance of <see cref="RenderTarget" /> class.</returns>
        /// <msdn-id>ff476521</msdn-id>
        ///   <unmanaged>HRESULT ID3D11Device::CreateTexture2D([In] const D3D11_TEXTURE2D_DESC* pDesc,[In, Buffer, Optional] const D3D11_SUBRESOURCE_DATA* pInitialData,[Out, Fast] ID3D11Texture2D** ppTexture2D)</unmanaged>
        ///   <unmanaged-short>ID3D11Device::CreateTexture2D</unmanaged-short>
        public RenderTarget GetTemporaryRenderTarget2D(int width, int height, MipMapCount mipCount, PixelFormat format, TextureFlags flags = TextureFlags.RenderTarget | TextureFlags.ShaderResource, int arraySize = 1)
        {
            return GetTemporaryTexture(Texture2DBase.NewDescription(width, height, format, flags, mipCount, arraySize, GraphicsResourceUsage.Default)).ToRenderTarget();
        }

        /// <summary>
        /// Gets a texture for the specified description.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns>A texture</returns>
        public Texture GetTemporaryTexture(TextureDescription description)
        {
            // For a specific description, get allocated textures
            List<TextureLink> textureLinks = null;
            if (!textureCache.TryGetValue(description, out textureLinks))
            {
                textureLinks = new List<TextureLink>();
                textureCache.Add(description, textureLinks);
            }

            // Find a texture available
            foreach (var textureLink in textureLinks)
            {
                if (textureLink.RefCount == 0)
                {
                    textureLink.RefCount = 1;
                    return textureLink.Texture;
                }
            }

            // If no texture available, then creates a new one
            var newTexture = CreateTexture(description);
            if (newTexture.Name == null)
            {
                newTexture.Name = string.Format("PostEffect{0}-{1}", Name == null ? string.Empty : string.Format("-{0}", Name), textureLinks.Count);
            }

            // Add the texture to the allocated textures
            // Start RefCount == 1, because we don't want this texture to be available if a post FxProcessor is calling
            // several times this GetTemporaryTexture method.
            var newTextureLink = new TextureLink(newTexture) { RefCount = 1 };
            textureLinks.Add(newTextureLink);

            return newTexture;
        }

        /// <summary>
        /// Increments the reference to an temporary texture.
        /// </summary>
        /// <param name="texture"></param>
        public void AddReferenceToTemporaryTexture(Texture texture)
        {
            if (texture == null)
            {
                return;
            }

            List<TextureLink> textureLinks = null;
            if (textureCache.TryGetValue(texture.Description, out textureLinks))
            {
                foreach (var textureLink in textureLinks)
                {
                    if (textureLink.Texture == texture)
                    {
                        textureLink.RefCount++;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Decrements the reference to a temporary texture.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <exception cref="System.InvalidOperationException">Unexpected Texture RefCount < 0</exception>
        public void ReleaseTemporaryTexture(Texture texture)
        {
            if (texture == null)
            {
                return;
            }

            List<TextureLink> textureLinks = null;
            if (textureCache.TryGetValue(texture.Description, out textureLinks))
            {
                foreach (var textureLink in textureLinks)
                {
                    if (textureLink.Texture == texture)
                    {
                        textureLink.RefCount--;

                        // If we are back to RefCount == 1, then the texture is 
                        // available.
                        if (textureLink.RefCount < 0)
                        {
                            throw new InvalidOperationException("Unexpected Texture RefCount < 0");
                        }
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a texture for output.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns>Texture.</returns>
        protected virtual Texture CreateTexture(TextureDescription description)
        {
            return Texture.New(GraphicsDevice, description);
        }

        protected override void Destroy()
        {
            foreach (var textureLinks in textureCache.Values)
            {
                foreach (var textureLink in textureLinks)
                {
                    textureLink.Texture.Dispose();
                }
                textureLinks.Clear();
            }
            textureCache.Clear();

            base.Destroy();
        }

        private class TextureLink
        {
            public TextureLink(Texture texture)
            {
                Texture = texture;
            }

            /// <summary>
            /// The texture
            /// </summary>
            public readonly Texture Texture;

            /// <summary>
            /// The number of active reference to this texture
            /// </summary>
            public int RefCount;
        }
    }
}