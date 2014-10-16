// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Physics
{
    public class BoxColliderShape : ColliderShape 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoxColliderShape"/> class.
        /// </summary>
        /// <param name="halfExtents">The half extents.</param>
        public BoxColliderShape(Vector3 halfExtents)
        {
            Type = ColliderShapeTypes.Box;
            Is2D = false;

            HalfExtents = halfExtents;

            InternalShape = new BulletSharp.BoxShape(halfExtents);

            if (!PhysicsEngine.Singleton.CreateDebugPrimitives) return;
            DebugPrimitive = GeometricPrimitive.Cube.New(PhysicsEngine.Singleton.DebugGraphicsDevice);
            DebugPrimitiveScaling = Matrix.Scaling((halfExtents * 2.0f) * 1.01f);
        }

        /// <summary>
        /// Gets the half extents.
        /// </summary>
        /// <value>
        /// The half extents.
        /// </value>
        public Vector3 HalfExtents { get; private set; }
    }
}
