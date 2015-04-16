// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using System;

using SiliconStudio.Paradox.Graphics.GeometricPrimitives;

namespace SiliconStudio.Paradox.Physics
{
    public class CapsuleColliderShape : ColliderShape
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CapsuleColliderShape"/> class.
        /// </summary>
        /// <param name="is2D">if set to <c>true</c> [is2 d].</param>
        /// <param name="radius">The radius.</param>
        /// <param name="height">The height.</param>
        /// <param name="upAxis">Up axis.</param>
        public CapsuleColliderShape(bool is2D, float radius, float height, Vector3 upAxis)
        {
            Type = ColliderShapeTypes.Capsule;
            Is2D = is2D;

            BulletSharp.CapsuleShape shape;

            Matrix rotation;

            //http://en.wikipedia.org/wiki/Capsule_(geometry)
            var h = radius * 2 + height;

            if (upAxis == Vector3.UnitX)
            {
                shape = new BulletSharp.CapsuleShapeX(radius, height);

                rotation = Matrix.RotationZ((float)Math.PI / 2.0f);
            }
            else if (upAxis == Vector3.UnitZ)
            {
                shape = new BulletSharp.CapsuleShapeZ(radius, height);

                rotation = Matrix.RotationX((float)Math.PI / 2.0f);
            }
            else //default to Y
            {
                shape = new BulletSharp.CapsuleShape(radius, height);

                rotation = Matrix.Identity;
            }

            if (Is2D)
            {
                InternalShape = new BulletSharp.Convex2DShape(shape) { LocalScaling = new Vector3(1, 1, 0) };
            }
            else
            {
                InternalShape = shape;
            }

            DebugPrimitiveMatrix = Matrix.Scaling(new Vector3(radius * 2, h / 2, Is2D ? 1.0f : radius * 2) * 1.01f) * rotation;
        }

        public override GeometricPrimitive CreateDebugPrimitive(GraphicsDevice device)
        {
            return GeometricPrimitive.Capsule.New(device);
        }
    }
}