// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// Enumerates the different ways to interpret a visual resolution value.
    /// </summary>
    [DataContract]
    public enum VirtualResolutionMode
    {
        /// <summary>
        /// The resolution is determined by the width, height and depth of the field.
        /// </summary>
        WidthHeightDepth,

        /// <summary>
        /// The resolution is determined by the width, the ratio of the target, and the depth.
        /// </summary>
        WidthDepthTargetRatio,

        /// <summary>
        /// The resolution is determined by the height, the ratio of the target, and the depth.
        /// </summary>
        HeightDepthTargetRatio,
    }
}