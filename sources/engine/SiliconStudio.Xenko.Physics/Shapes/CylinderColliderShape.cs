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
        private readonly ShapeOrientation shapeOrientation;

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

            CachedScaling = Vector3.One;
            shapeOrientation = orientation;

            switch (orientation)
            {
                case ShapeOrientation.UpX:
                    InternalShape = new BulletSharp.CylinderShapeX(new Vector3(height/2, radius, radius))
                    {
                        LocalScaling = CachedScaling
                    };
                    rotation = Matrix.RotationZ((float)Math.PI / 2.0f);
                    break;
                case ShapeOrientation.UpY:
                    InternalShape = new BulletSharp.CylinderShape(new Vector3(radius, height/2, radius))
                    {
                        LocalScaling = CachedScaling
                    };
                    rotation = Matrix.Identity;
                    break;
                case ShapeOrientation.UpZ:
                    InternalShape = new BulletSharp.CylinderShapeZ(new Vector3(radius, radius, height/2))
                    {
                        LocalScaling = CachedScaling
                    };
                    rotation = Matrix.RotationX((float)Math.PI / 2.0f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orientation));
            }

            DebugPrimitiveMatrix = Matrix.Scaling(new Vector3(radius * 2, height, radius * 2) * 1.01f) * rotation;
        }

        public override MeshDraw CreateDebugPrimitive(GraphicsDevice device)
        {
            return GeometricPrimitive.Cylinder.New(device).ToMeshDraw();
        }
    }
}
