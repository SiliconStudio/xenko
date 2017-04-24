// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using System;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Physics
{
    public class ConeColliderShape : ColliderShape
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConeColliderShape"/> class.
        /// </summary>
        /// <param name="orientation">Up axis.</param>
        /// <param name="radius">The radius of the cone</param>
        /// <param name="height">The height of the cone</param>
        public ConeColliderShape(float height, float radius, ShapeOrientation orientation)
        {
            Type = ColliderShapeTypes.Cone;
            Is2D = false; //always false for cone

            Matrix rotation;

            CachedScaling = Vector3.One;

            switch (orientation)
            {
                case ShapeOrientation.UpX:
                    InternalShape = new BulletSharp.ConeShapeX(radius, height)
                    {
                        LocalScaling = CachedScaling
                    };
                    rotation = Matrix.RotationZ((float)Math.PI / 2.0f);
                    break;
                case ShapeOrientation.UpY:
                    InternalShape = new BulletSharp.ConeShape(radius, height)
                    {
                        LocalScaling = CachedScaling
                    };
                    rotation = Matrix.Identity;
                    break;
                case ShapeOrientation.UpZ:
                    InternalShape = new BulletSharp.ConeShapeZ(radius, height)
                    {
                        LocalScaling = CachedScaling
                    };
                    rotation = Matrix.RotationX((float)Math.PI / 2.0f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orientation));
            }

            DebugPrimitiveMatrix = Matrix.Scaling(new Vector3(radius * 2, height, radius * 2) * DebugScaling) * rotation;
        }

        public override MeshDraw CreateDebugPrimitive(GraphicsDevice device)
        {
            return GeometricPrimitive.Cone.New(device).ToMeshDraw();
        }
    }
}
