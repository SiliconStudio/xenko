// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects.Modules.Renderers;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Modules.Processors
{
    /// <summary>
    /// Represents a texture to use with <see cref="ShadowMapRenderer"/>.
    /// </summary>
    public class ShadowMapTexture
    {
        public ShadowMapTexture(GraphicsDevice graphicsDevice, ShadowMapFilterType filterType, int shadowMapSize)
        {
            IsVarianceShadowMap = filterType == ShadowMapFilterType.Variance;

            if (filterType == ShadowMapFilterType.Variance)
            {
                ShadowMapDepthTexture = Texture2D.New(graphicsDevice, shadowMapSize, shadowMapSize, PixelFormat.D32_Float, TextureFlags.DepthStencil | TextureFlags.ShaderResource);
                ShadowMapTargetTexture = Texture2D.New(graphicsDevice, shadowMapSize, shadowMapSize, PixelFormat.R32G32_Float, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
                ShadowMapRenderTarget = ShadowMapTargetTexture.ToRenderTarget();

                IntermediateBlurTexture = Texture2D.New(graphicsDevice, shadowMapSize, shadowMapSize, PixelFormat.R32G32_Float, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
                IntermediateBlurRenderTarget = IntermediateBlurTexture.ToRenderTarget();
            }
            else
                ShadowMapDepthTexture = Texture2D.New(graphicsDevice, shadowMapSize, shadowMapSize, PixelFormat.D32_Float, TextureFlags.DepthStencil | TextureFlags.ShaderResource);
            
            ShadowMapDepthBuffer = ShadowMapDepthTexture.ToDepthStencilBuffer(false);
            GuillotinePacker = new GuillotinePacker();
        }

        internal Texture2D ShadowMapDepthTexture;
        internal DepthStencilBuffer ShadowMapDepthBuffer;
        internal GuillotinePacker GuillotinePacker;
        internal bool IsVarianceShadowMap;

        // VSM only
        internal Texture2D ShadowMapTargetTexture;
        internal RenderTarget ShadowMapRenderTarget;
        internal Texture2D IntermediateBlurTexture;
        internal RenderTarget IntermediateBlurRenderTarget;
    }
}