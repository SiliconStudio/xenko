// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Extensions;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.GeometricPrimitives;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Physics
{
    public class BoxColliderShape : ColliderShape
    {
        private static MeshDraw cachedDebugPrimitive;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxColliderShape"/> class.
        /// </summary>
        /// <param name="size">The size of the cube</param>
        public BoxColliderShape(Vector3 size)
        {
            Type = ColliderShapeTypes.Box;
            Is2D = false;

            InternalShape = new BulletSharp.BoxShape(size/2)
            {
                LocalScaling = Vector3.One
            };

            DebugPrimitiveMatrix = Matrix.Scaling(size * 1.01f);
        }

        public override MeshDraw CreateDebugPrimitive(GraphicsDevice device)
        {
            return cachedDebugPrimitive ?? (cachedDebugPrimitive = GeometricPrimitive.Cube.New(device).ToMeshDraw());
        }
    }
}
