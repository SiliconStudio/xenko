// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Xenko.Physics
{
    /// <summary>
    /// Defines the different possible orientations of a shape.
    /// </summary>
    public enum ShapeOrientation
    {
        /// <summary>
        /// The shape is aligned with the Ox axis.
        /// </summary>
        /// <userdoc>The top of shape is aligned with the Ox axis.</userdoc>
        UpX,

        /// <summary>
        /// The shape is aligned with the Oy axis.
        /// </summary>
        /// <userdoc>The top shape is aligned with the Oy axis.</userdoc>
        UpY,

        /// <summary>
        /// The shape is aligned with the Oz axis.
        /// </summary>
        /// <userdoc>The top shape is aligned with the Oz axis.</userdoc>
        UpZ,
    }
}
