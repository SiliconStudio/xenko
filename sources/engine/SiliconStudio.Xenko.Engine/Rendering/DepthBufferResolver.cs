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
    public class DepthBufferResolver : BufferResolver
    {
        private Texture depthStencilAsRT;

        private Texture depthStencilAsSR;

        /// <inheritdoc/>
        public override Texture AsRenderTarget()
        {
            return depthStencilAsRT;
        }

        /// <inheritdoc/>
        public override Texture AsShaderResourceView()
        {
            return depthStencilAsSR;
        }

        /// <inheritdoc/>
        public override void Resolve(RenderDrawContext renderContext, Texture texture)
        {
            if (!renderContext.GraphicsDevice.Features.HasDepthAsSRV)
            {
                depthStencilAsRT = texture;
                depthStencilAsSR = null;
                return;
            }

            if (renderContext.GraphicsDevice.Features.HasDepthAsReadOnlyRT)
            {
                if (!HasDepthStencilChanged(renderContext, texture))
                    return;

                depthStencilAsRT = texture.ToDepthStencilReadOnlyTexture();

                depthStencilAsSR = texture;

                return;
            }

            // Depth as read-only render target AND shader resource is not supported - we have to copy it

            if (!HasDepthStencilChanged(renderContext, texture))
                return;

            // Depth as a RenderTarget is the same
            depthStencilAsRT = texture;

            // Depth as a ShaderResource is a copy
            if (depthStencilAsSR != null)
            {
                renderContext.RenderContext.Allocator.ReleaseReference(depthStencilAsSR);
                depthStencilAsSR = null;
            }

            var textureDescription = texture.Description;
            textureDescription.Flags = TextureFlags.ShaderResource;
            textureDescription.Format = PixelFormat.R24_UNorm_X8_Typeless;

            // We want this texture to persist, so we don't release it immediately after we used it
            depthStencilAsSR = renderContext.RenderContext.Allocator.GetTemporaryTexture2D(textureDescription);

            if (depthStencilAsSR != null)
            {
                renderContext.CommandList.Copy(texture, depthStencilAsSR);
            }
        }

        private bool HasDepthStencilChanged(RenderDrawContext renderContext, Texture original)
        {
            if (original == null)
                return false;   // It's null so we can't copy it

            if (original.IsDisposed)
                return false;

            if (renderContext.GraphicsDevice.Features.HasDepthAsReadOnlyRT)
            {
                if (depthStencilAsRT == null)
                    return true;

                if (depthStencilAsSR == null || depthStencilAsSR != original)
                    return true;
            }
            else
            {
                if (depthStencilAsRT == null || depthStencilAsRT != original)
                    return true;

                if (depthStencilAsSR == null)
                    return true;

                if (depthStencilAsSR.Width != original.Width || depthStencilAsSR.Height != original.Height)
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override void Reset(RenderDrawContext renderContext)
        {

        }

    }
}
