// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using BulletSharp;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using System;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Physics
{
    public class CapsuleColliderShape : ColliderShape
    {
        private readonly float capsuleLength;
        private readonly float capsuleRadius;

        /// <summary>
        /// Initializes a new instance of the <see cref="CapsuleColliderShape"/> class.
        /// </summary>
        /// <param name="is2D">if set to <c>true</c> [is2 d].</param>
        /// <param name="radius">The radius.</param>
        /// <param name="length">The length of the capsule.</param>
        /// <param name="orientation">Up axis.</param>
        public CapsuleColliderShape(bool is2D, float radius, float length, ShapeOrientation orientation)
        {
            Type = ColliderShapeTypes.Capsule;
            Is2D = is2D;

            capsuleLength = length;
            capsuleRadius = radius;

            Matrix rotation;
            CapsuleShape shape;

            switch (orientation)
            {
                case ShapeOrientation.UpX:
                    shape = new CapsuleShapeZ(radius, length)
                    {
                        LocalScaling = Vector3.One
                    };
                    rotation = Matrix.RotationX((float)Math.PI / 2.0f);
                    break;
                case ShapeOrientation.UpY:
                    shape = new CapsuleShape(radius, length)
                    {
                        LocalScaling = Vector3.One
                    };
                    rotation = Matrix.Identity;
                    break;
                case ShapeOrientation.UpZ:
                    shape = new CapsuleShapeX(radius, length)
                    {
                        LocalScaling = Vector3.One
                    };
                    rotation = Matrix.RotationZ((float)Math.PI / 2.0f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("orientation");
            }

            InternalShape = Is2D ? (CollisionShape)new Convex2DShape(shape) { LocalScaling = new Vector3(1, 1, 0) }: shape;

            DebugPrimitiveMatrix = Matrix.Scaling(new Vector3(1.01f)) * rotation;
        }

        public override MeshDraw CreateDebugPrimitive(GraphicsDevice device)
        {
            return GeometricPrimitive.Capsule.New(device, capsuleLength, capsuleRadius).ToMeshDraw();
        }
    }
}