// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using System;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Physics
{
    public class CylinderColliderShape : ColliderShape
    {
        private static MeshDraw cachedDebugPrimitive;

        /// <summary>
        /// Initializes a new instance of the <see cref="CylinderColliderShape"/> class.
        /// </summary>
        /// <param name="orientation">Up axis.</param>
        /// <param name="radius">The radius of the cylinder</param>
        /// <param name="height">The height of the cylinder</param>
        public CylinderColliderShape(float height, float radius, ShapeOrientation orientation)
        {
            Type = ColliderShapeTypes.Cylinder;
            Is2D = false; //always false for cylinders

            Matrix rotation;

            switch (orientation)
            {
                case ShapeOrientation.UpX:
                    InternalShape = new BulletSharp.CylinderShapeX(new Vector3(height/2, radius, 0))
                    {
                        LocalScaling = Vector3.One
                    };
                    rotation = Matrix.RotationZ((float)Math.PI / 2.0f);
                    break;
                case ShapeOrientation.UpY:
                    InternalShape = new BulletSharp.CylinderShape(new Vector3(radius, height/2, 0))
                    {
                        LocalScaling = Vector3.One
                    };
                    rotation = Matrix.Identity;
                    break;
                case ShapeOrientation.UpZ:
                    InternalShape = new BulletSharp.CylinderShapeZ(new Vector3(radius, 0, height/2))
                    {
                        LocalScaling = Vector3.One
                    };
                    rotation = Matrix.RotationX((float)Math.PI / 2.0f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("orientation");
            }

            DebugPrimitiveMatrix = Matrix.Scaling(new Vector3(2*radius, height, 2*radius) * 1.01f) * rotation;
        }

        public override MeshDraw CreateDebugPrimitive(GraphicsDevice device)
        {
            return cachedDebugPrimitive ?? (cachedDebugPrimitive = GeometricPrimitive.Cylinder.New(device).ToMeshDraw());
        }
    }
}