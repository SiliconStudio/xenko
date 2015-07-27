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
        private float capsuleLength;
        private float capsuleRadius;

        /// <summary>
        /// Initializes a new instance of the <see cref="CapsuleColliderShape"/> class.
        /// </summary>
        /// <param name="is2D">if set to <c>true</c> [is2 d].</param>
        /// <param name="radius">The radius.</param>
        /// <param name="length">The length of the capsule.</param>
        /// <param name="upAxis">Up axis.</param>
        public CapsuleColliderShape(bool is2D, float radius, float length, Vector3 upAxis)
        {
            Type = ColliderShapeTypes.Capsule;
            Is2D = is2D;

            capsuleLength = length;
            capsuleRadius = radius;

            Matrix rotation;
            BulletSharp.CapsuleShape shape;

            if (upAxis == Vector3.UnitX)
            {
                shape = new BulletSharp.CapsuleShapeX(radius, length);

                rotation = Matrix.RotationZ((float)Math.PI / 2.0f);
            }
            else if (upAxis == Vector3.UnitZ)
            {
                shape = new BulletSharp.CapsuleShapeZ(radius, length);

                rotation = Matrix.RotationX((float)Math.PI / 2.0f);
            }
            else //default to Y
            {
                shape = new BulletSharp.CapsuleShape(radius, length);

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

            DebugPrimitiveMatrix = Matrix.Scaling(new Vector3(1.01f)) * rotation;
        }

        public override GeometricPrimitive CreateDebugPrimitive(GraphicsDevice device)
        {
            return GeometricPrimitive.Capsule.New(device, capsuleLength, capsuleRadius);
        }
    }
}