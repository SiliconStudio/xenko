// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Physics
{
    /// <summary>
    /// The result of a Physics Raycast or ShapeSweep operation
    /// </summary>
    public struct HitResult
    {
        /// <summary>
        /// The collider hit by the 
        /// </summary>
        public PhysicsComponent Collider;

        public Vector3 Normal;

        public Vector3 Point;

        /// <summary>
        /// used to compute NormalizedDistance
        /// </summary>
        internal float FullLength;

        internal Vector3 StartPoint;

        private float normalizedDistance;

        public float NormalizedDistance
        {
            get
            {
                if (normalizedDistance != -1.0f || FullLength == 0.0f) return normalizedDistance;

                var dist = (Point - StartPoint).LengthSquared();
                normalizedDistance = dist/FullLength;

                return normalizedDistance;
            }
            internal set
            {
                normalizedDistance = value;
            }
        }

        public bool Succeeded;
    }
}