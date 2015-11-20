// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.IO;
using SiliconStudio.Xenko.Assets.SpriteFont.Compiler;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Font;
using Glyph = SiliconStudio.Xenko.Graphics.Font.Glyph;

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
            var staticFont = (StaticSpriteFont)StaticFontCompiler.Compile(FontDataFactory, asset, srgb);

            var glyphs = new List<Glyph>(staticFont.CharacterToGlyph.Values);
            var textures = staticFont.Textures;
            
            var imageType = ImageFileType.Xenko;
            var textureFileName = texturePath + imageType.ToFileExtension();

            if (textures != null && textures.Count == 0)
            {
                // save the texture   TODO support for multi-texture
                using (var stream = File.OpenWrite(textureFileName))
                    staticFont.Textures[0].Save(stream, imageType);
            }

            var precompiledAsset = new PrecompiledSpriteFontAsset
            {
                Glyphs = glyphs,
                Size = staticFont.Size,
                Source = textureFileName,
                BaseOffset = staticFont.BaseOffsetY,
                DefaultLineSpacing = staticFont.DefaultLineSpacing,
                ExtraSpacing = staticFont.ExtraSpacing,
                ExtraLineSpacing = staticFont.ExtraLineSpacing,
                DefaultCharacter = asset.DefaultCharacter,
            };

            return precompiledAsset;
        }
    }
}