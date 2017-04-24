// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
