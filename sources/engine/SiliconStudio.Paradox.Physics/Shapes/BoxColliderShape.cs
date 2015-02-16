// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
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

            InternalShape = new BulletSharp.BoxShape(halfExtents);

            if (!Simulation.CreateDebugPrimitives) return;
            DebugPrimitive = GeometricPrimitive.Cube.New(Simulation.DebugGraphicsDevice);
            DebugPrimitiveScaling = Matrix.Scaling((halfExtents * 2.0f) * 1.01f);
        }
    }
}
