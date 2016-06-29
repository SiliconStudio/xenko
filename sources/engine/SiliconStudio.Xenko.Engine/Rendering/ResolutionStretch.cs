// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Enumerates the different ways to interpret a visual resolution value.
    /// </summary>
    [DataContract]
    public enum ResolutionStretch
    {
        /// <summary>
        /// The resolution is determined by the width, height and depth of the field.
        /// </summary>
        FixedWidthFixedHeight,

        /// <summary>
        /// The resolution is determined by the width, the ratio of the target, and the depth.
        /// </summary>
        FixedWidthAdaptableHeight,

        /// <summary>
        /// The resolution is determined by the height, the ratio of the target, and the depth.
        /// </summary>
        FixedHeightAdaptableWidth,
    }
}
