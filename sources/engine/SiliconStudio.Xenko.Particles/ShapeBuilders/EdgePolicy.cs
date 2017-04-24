// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
