// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Textures
{
    /// <summary>
    /// Defines how a texture is imported and used at runtime.
    /// </summary>
    [DataContract("TextureColorSpace")]
    public enum TextureColorSpace
    {
        /// <summary>
        /// Depending on the <see cref="GameSettingsAsset.ColorSpace"/>, the texture will be imported in <see cref="Linear"/> or <see cref="Gamma"/> space.
        /// </summary>
        Auto,

        /// <summary>
        /// The texture will be used in linear space with a gamma correct rendering, applying the gamma correction automatically at compile time and when sampling the texture at runtime.
        /// </summary>
        Linear,

        /// <summary>
        /// The texture will be used in gamma space for cases where the texture doesn't need to be gamma correct.
        /// </summary>
        Gamma,
    }

    public static class TextureColorSpaceHelper
    {
        public static ColorSpace ToColorSpace(this TextureColorSpace textureColorSpace, ColorSpace colorSpaceReference, TextureHint textureHint)
        {
            var colorSpace = ColorSpace.Gamma;
            if (textureHint == TextureHint.Color)
            {
                colorSpace = colorSpaceReference;
                if (textureColorSpace == TextureColorSpace.Linear)
                {
                    colorSpace = ColorSpace.Linear;
                }
                else if (textureColorSpace == TextureColorSpace.Gamma)
                {
                    colorSpace = ColorSpace.Gamma;
                }
            }
            return colorSpace;
        }
    }
}