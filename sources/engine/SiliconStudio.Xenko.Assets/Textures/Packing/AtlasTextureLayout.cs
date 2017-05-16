// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Assets.Textures.Packing
{
    /// <summary>
    /// This specifies the full layout of an atlas texture.
    /// </summary>
    public class AtlasTextureLayout
    {
        /// <summary>
        /// Gets or sets a list of packed AtlasTextureElement
        /// </summary>
        public readonly List<AtlasTextureElement> Textures = new List<AtlasTextureElement>();

        /// <summary>
        /// Gets or sets Width of the texture atlas
        /// </summary>
        public int Width;

        /// <summary>
        /// Gets or sets Height of the texture atlas
        /// </summary>
        public int Height;
    }
}
