// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Resolves a DepthRenderTarget from one render pass to be used as an input to another render pass
    /// </summary>
    public class ResourceResolver
    {
        public RenderDrawContext renderContext { get; set; }

        /// <summary>
        /// DONE
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public Texture GetDepthStencilAsRenderTarget(Texture texture)
        {
            if (!renderContext.GraphicsDevice.Features.HasDepthAsSRV || !renderContext.GraphicsDevice.Features.HasDepthAsReadOnlyRT)
                return texture;

            return texture.ToDepthStencilReadOnlyTexture();
        }

        /// <summary>
        /// DONE
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public Texture GetDepthStenctilAsShaderResource(Texture texture)
        {
            if (!renderContext.GraphicsDevice.Features.HasDepthAsSRV)
                return null;

            if (renderContext.GraphicsDevice.Features.HasDepthAsReadOnlyRT)
                return texture;

            return GetDepthStenctilAsShaderResource_Copy(texture);
        }

        public void ReleaseDepthStenctilAsShaderResource(Texture depthAsSR)
        {
            // If no resources were allocated in the first place there is nothing to release
            if (!renderContext.GraphicsDevice.Features.HasDepthAsSRV || renderContext.GraphicsDevice.Features.HasDepthAsReadOnlyRT)
                return;

            renderContext.RenderContext.Allocator.ReleaseReference(depthAsSR);
        }

        /// <summary>
        /// DONE
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        private Texture GetDepthStenctilAsShaderResource_Copy(Texture texture)
        {
            var textureDescription = texture.Description;
            textureDescription.Flags = TextureFlags.ShaderResource;
            textureDescription.Format = PixelFormat.R24_UNorm_X8_Typeless;

            return renderContext.RenderContext.Allocator.GetTemporaryTexture2D(textureDescription);
        }

        public Texture ResolveDepthStencil(Texture texture)
        {
            if (!renderContext.GraphicsDevice.Features.HasDepthAsSRV)
                return null;

            if (renderContext.GraphicsDevice.Features.HasDepthAsReadOnlyRT)
                return texture;

            var depthStencil = GetDepthStenctilAsShaderResource_Copy(texture);

            renderContext.CommandList.Copy(texture, depthStencil);

            return depthStencil;
        }
        
        
    }
}
