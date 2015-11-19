// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    public static class SpriteFontAssetExtensions
    {
        private static readonly FontDataFactory FontDataFactory = new FontDataFactory();

        /// <summary>
        /// Generate a precompiled sprite font from the current sprite font asset.
        /// </summary>
        /// <param name="asset">The sprite font asset</param>
        /// <param name="texturePath">The path of the source texture</param>
        /// <param name="srgb">Indicate if the generated texture should be srgb</param>
        /// <returns>The precompiled sprite font asset</returns>
        public static PrecompiledSpriteFontAsset GeneratePrecompiledSpriteFont(this SpriteFontAsset asset, string texturePath, bool srgb)
        {
            // TODO actually generate the asset
            //var staticFont = StaticFontCompiler.Compile(FontDataFactory, asset, srgb);
            
            // save the texture
            using (var stream = File.OpenWrite(texturePath))
            {

            }

            var precompiledAsset = new PrecompiledSpriteFontAsset
            {
                Source = texturePath,
            };

            return precompiledAsset;
        }
    }
}