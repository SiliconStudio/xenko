// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Textures
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
