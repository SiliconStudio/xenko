// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.ShapeBuilders
{
    /// <summary>
    /// Specifies how texture coordinates should be assigned to the ribbonized mesh.
    /// </summary>
    [DataContract("TextureCoordinatePolicy")]
    [Display("Tex Coordinates")]
    public enum TextureCoordinatePolicy
    {
        /// <summary>
        /// <see cref="AsIs"/> will assign a (0, 0, 1, 1) quad to each segment along the ribbon.
        /// </summary>
        AsIs,

        /// <summary>
        /// <see cref="Stretched"/> will assign a (0, 0, 1, X) quad stretched over the entire ribbon, where X is user-defined.
        /// </summary>
        Stretched,

        /// <summary>
        /// <see cref="DistanceBased"/> will assign a (0, 0, 1, Length) quad stretched over the entire ribbon, where Length is the actual length of the ribbon.
        /// </summary>
        DistanceBased,
    }
}
