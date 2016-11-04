// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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