// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects.Processors;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.ShadowMaps
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
                ShadowMapDepthTexture = Texture.New2D(graphicsDevice, shadowMapSize, shadowMapSize, PixelFormat.D32_Float, TextureFlags.DepthStencil | TextureFlags.ShaderResource);
                ShadowMapTargetTexture = Texture.New2D(graphicsDevice, shadowMapSize, shadowMapSize, PixelFormat.R32G32_Float, TextureFlags.RenderTarget | TextureFlags.ShaderResource);

                IntermediateBlurTexture = Texture.New2D(graphicsDevice, shadowMapSize, shadowMapSize, PixelFormat.R32G32_Float, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
            }
            else
                ShadowMapDepthTexture = Texture.New2D(graphicsDevice, shadowMapSize, shadowMapSize, PixelFormat.D32_Float, TextureFlags.DepthStencil | TextureFlags.ShaderResource);
            
            GuillotinePacker = new GuillotinePacker();
        }

        internal Texture ShadowMapDepthTexture;
        internal GuillotinePacker GuillotinePacker;
        internal bool IsVarianceShadowMap;

        // VSM only
        internal Texture ShadowMapTargetTexture;
        internal Texture IntermediateBlurTexture;
    }
}