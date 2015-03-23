// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using System;

using SiliconStudio.Paradox.Graphics.GeometricPrimitives;

namespace SiliconStudio.Paradox.Physics
{
    public class CylinderColliderShape : ColliderShape
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CylinderColliderShape"/> class.
        /// </summary>
        /// <param name="halfExtents">The half extents.</param>
        /// <param name="upAxis">Up axis.</param>
        public CylinderColliderShape(Vector3 halfExtents, Vector3 upAxis)
        {
            Type = ColliderShapeTypes.Cylinder;
            Is2D = false; //always false for cylinders

            Matrix rotation;
            Vector3 scaling;

            if (upAxis == Vector3.UnitX)
            {
                InternalShape = new BulletSharp.CylinderShapeX(halfExtents);

                rotation = Matrix.RotationZ((float)Math.PI / 2.0f);
                scaling = new Vector3(halfExtents.Y * 2.0f, halfExtents.X * 2.0f, halfExtents.Z * 2.0f);
            }
            else if (upAxis == Vector3.UnitZ)
            {
                InternalShape = new BulletSharp.CylinderShapeZ(halfExtents);

                rotation = Matrix.RotationX((float)Math.PI / 2.0f);
                scaling = new Vector3(halfExtents.X * 2.0f, halfExtents.Z * 2.0f, halfExtents.Y * 2.0f);
            }
            else //default to Y
            {
                InternalShape = new BulletSharp.CylinderShape(halfExtents);

                rotation = Matrix.Identity;
                scaling = halfExtents * 2.0f;
            }

            DebugPrimitiveMatrix = Matrix.Scaling(scaling * 1.01f) * rotation;
        }

        public override GeometricPrimitive CreateDebugPrimitive(GraphicsDevice device)
        {
            return GeometricPrimitive.Cylinder.New(device);
        }
    }
}