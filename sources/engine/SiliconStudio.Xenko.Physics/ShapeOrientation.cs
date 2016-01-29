// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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