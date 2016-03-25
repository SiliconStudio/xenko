// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders
{
    /// <summary>
    /// Specifies how texture coordinates should be assigned to the ribbonized mesh.
    /// <see cref="AsIs"/> will assign a (0, 0, 1, 1) quad to each segment along the ribbon.
    /// <see cref="Stretched"/> will assign a (0, 0, 1, X) quad stretched over the entire ribbon, where X is user-defined.
    /// <see cref="DistanceBased"/> will assign a (0, 0, 1, Length) quad stretched over the entire ribbon, where Length is the actual length of the ribbon.
    /// </summary>
    [DataContract("TextureCoordinatePolicy")]
    [Display("Tex Coordinates")]
    public enum TextureCoordinatePolicy
    {
        AsIs,
        Stretched,
        DistanceBased,
    }


    /// <summary>
    /// Specifies if the ribbon should be additionally smoothed or rendered as is.
    /// </summary>
    [DataContract("SmoothingPolicy")]
    [Display("Smoothing")]
    public enum SmoothingPolicy
    {
        None,   // Ribbons only use control points and edges are hard. Good for straight lines
        Fast,   // Smoothing using Catmull-Rom interpolation. Generally looks good
        Best,   // Smoothing based on circumcircles generated around every three adjacent points. Best suited for rapid, circular motions
    }


    /// <summary>
    /// Specifies if the trail lies on one edge on the axis or is the axis in its center.
    /// </summary>
    [DataContract("EdgePolicy")]
    [Display("Edge")]
    public enum EdgePolicy
    {
        Edge,
        Center,
    }
}
