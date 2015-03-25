// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.GeometricPrimitives;

namespace SiliconStudio.Paradox.Physics
{
    public class Box2DColliderShape : ColliderShape
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Box2DColliderShape"/> class.
        /// </summary>
        /// <param name="halfExtents">The half extents.</param>
        public Box2DColliderShape(Vector2 halfExtents)
        {
            Type = ColliderShapeTypes.Box;
            Is2D = true;

            InternalShape = new BulletSharp.Box2DShape(halfExtents) { LocalScaling = new Vector3(1, 1, 0) };

            DebugPrimitiveMatrix = Matrix.Scaling(new Vector3(halfExtents.X * 2, halfExtents.Y * 2, 1.0f) * 1.01f);
        }

        public override GeometricPrimitive CreateDebugPrimitive(GraphicsDevice device)
        {
            return GeometricPrimitive.Cube.New(device);
        }
    }
}
