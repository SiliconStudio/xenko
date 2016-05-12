// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    public class DepthBufferResolver : BufferResolver
    {
        ///// <summary>
        ///// Property key to access the Current <see cref="DepthBufferResolver"/> from <see cref="RenderContext.Tags"/>.
        ///// </summary>
        //public static readonly PropertyKey<DepthBufferResolver> Current = new PropertyKey<DepthBufferResolver>("DepthBufferResolver.Current", typeof(DepthBufferResolver));

        private Texture depthStencilAsRT;

        private Texture depthStencilAsSR;

        public override Texture AsRenderTarget()
        {
            return depthStencilAsRT;
        }

        public override Texture AsShaderResourceView()
        {
            return depthStencilAsSR;
        }

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
            depthStencilAsSR?.Dispose();

            var textureDescription = texture.Description;
            textureDescription.Flags = TextureFlags.ShaderResource;
            textureDescription.Format = PixelFormat.R24_UNorm_X8_Typeless;

            depthStencilAsSR = Texture.New(renderContext.GraphicsDevice, textureDescription);

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
    }
}
