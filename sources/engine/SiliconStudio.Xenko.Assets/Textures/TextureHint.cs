// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Textures
{
    /// <summary>
    /// Gives a hint to the texture compressor on the kind of textures to select the appropriate compression format depending
    /// on the HW Level and platform.
    /// </summary>
    [DataContract("TextureHint")]
    public enum TextureHint
    {
        /// <summary>
        /// The texture is using the full color.
        /// </summary>
        Color,

        /// <summary>
        /// The texture is a grayscale.
        /// </summary>
        Grayscale,

        /// <summary>
        /// The texture is a normal map.
        /// </summary>
        NormalMap
    }
}