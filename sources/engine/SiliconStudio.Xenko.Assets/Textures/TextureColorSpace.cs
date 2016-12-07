// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets.Textures
{
    /// <summary>
    /// Defines how a texture is imported and used at runtime.
    /// </summary>
    [DataContract("TextureColorSpace")]
    public enum TextureColorSpace
    {
        /// <summary>
        /// Depending on the <see cref="RenderingSettings.ColorSpace"/>, the texture will be imported in <see cref="Linear"/> or <see cref="Gamma"/> space.
        /// </summary>
        Auto,

        /// <summary>
        /// The texture will be used in linear space for gamma correct rendering, applying the gamma correction automatically at compile time and when sampling the texture at runtime.
        /// </summary>
        Linear,

        /// <summary>
        /// The texture will be used in gamma space for cases where the texture doesn't need to be gamma correct.
        /// </summary>
        Gamma,
    }

    public static class TextureColorSpaceHelper
    {
        public static ColorSpace ToColorSpace(this TextureColorSpace textureColorSpace, ColorSpace colorSpaceReference)
        {
            switch (textureColorSpace)
            {
                case TextureColorSpace.Linear:
                    return ColorSpace.Linear;
                case TextureColorSpace.Gamma:
                    return ColorSpace.Gamma;
                default:
                    return colorSpaceReference;
            }
        }
    }
}