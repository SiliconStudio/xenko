// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Runtime.InteropServices;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Physics
{
    /// <summary>
    /// The result of a Physics Raycast or ShapeSweep operation
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct HitResult
    {
        public Vector3 Normal;

        public Vector3 Point;

        public float HitFraction;

        public bool Succeeded;

        /// <summary>
        /// The Collider hit if Succeeded
        /// </summary>
        public PhysicsComponent Collider;
    }
}
