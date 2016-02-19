// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Physics
{
    public enum ColliderShapeTypes
    {
        /// <summary>
        ///     3D and 2D ( a plane )
        /// </summary>
        Box,

        /// <summary>
        ///     3D and 2D ( a circle )
        /// </summary>
        Sphere,

        /// <summary>
        ///     3D only
        /// </summary>
        Cylinder,

        /// <summary>
        ///     3D and 2D
        /// </summary>
        Capsule,

        ConvexHull,

        Compound,

        StaticPlane
    }
}