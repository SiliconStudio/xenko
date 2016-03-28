// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders
{
    /// <summary>
    /// Specifies if the trail lies on one edge on the axis or is the axis in its center.
    /// </summary>
    [DataContract("EdgePolicy")]
    [Display("Edge")]
    public enum EdgePolicy
    {
        /// <summary>
        /// The line between the control points will be used as an edge for the trail or ribbon
        /// </summary>
        Edge,

        /// <summary>
        /// The line between the control points will be used as a central axis for the trail or ribbon
        /// </summary>
        Center,
    }
}
