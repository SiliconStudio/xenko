// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Texture
{
    [DataContract]
    public enum AlphaFormat
    {
        /// <summary>
        /// Alpha channel should be ignored.
        /// </summary>
        None,

        /// <summary>
        /// Alpha channel should be stored as 1-bit mask if possible.
        /// </summary>
        Mask,

        /// <summary>
        /// Alpha channel should be stored with explicit compression. Well suited to sharp alpha transitions between translucent and opaque areas.
        /// </summary>
        Explicit,

        /// <summary>
        /// Alpha channel should be stored using interpolation. Well suited for alpha gradient.
        /// </summary>
        Interpolated,
    }
}