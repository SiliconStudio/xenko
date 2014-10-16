// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

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

            HalfExtents = halfExtents;
            UpAxis = upAxis;

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
                UpAxis = Vector3.UnitY;
                InternalShape = new BulletSharp.CylinderShape(halfExtents);

                rotation = Matrix.Identity;
                scaling = halfExtents * 2.0f;
            }

            if (!PhysicsEngine.Singleton.CreateDebugPrimitives) return;
            DebugPrimitive = GeometricPrimitive.Cylinder.New(PhysicsEngine.Singleton.DebugGraphicsDevice);
            DebugPrimitiveScaling = Matrix.Scaling(scaling * 1.01f) * rotation;
        }

        /// <summary>
        /// Gets the half extents.
        /// </summary>
        /// <value>
        /// The half extents.
        /// </value>
        public Vector3 HalfExtents { get; private set; }

        public float Radius
        {
            get
            {
                return ((BulletSharp.CylinderShape)InternalShape).Radius;
            }
        }

        /// <summary>
        /// Gets up axis.
        /// </summary>
        /// <value>
        /// Up axis.
        /// </value>
        public Vector3 UpAxis { get; private set; }
    }
}
