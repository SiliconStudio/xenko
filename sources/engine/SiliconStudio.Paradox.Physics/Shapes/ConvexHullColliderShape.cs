using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Physics
{
    public class ConvexHullColliderShape : ColliderShape
    {
        public ConvexHullColliderShape(IReadOnlyList<Vector3> points, IEnumerable<uint> indices)
        {
            Type = ColliderShapeTypes.ConvexHull;
            Is2D = false;

            InternalShape = new BulletSharp.ConvexHullShape(points);

            if (!PhysicsEngine.Singleton.CreateDebugPrimitives) return;

            var verts = new VertexPositionNormalTexture[points.Count];
            for (var i = 0; i < points.Count; i++)
            {
                verts[i].Position = points[i];
                verts[i].TextureCoordinate = Vector2.Zero;
                verts[i].Normal = Vector3.Zero;
            }

            var intIndices = Array.ConvertAll(indices.ToArray(), input => (int)input);
            var meshData = new GeometricMeshData<VertexPositionNormalTexture>(verts, intIndices, false, VertexPositionNormalTexture.Layout);

            DebugPrimitive = new GeometricPrimitive(PhysicsEngine.Singleton.DebugGraphicsDevice, meshData);
            DebugPrimitiveScaling = Matrix.Scaling(new Vector3(1, 1, 1) * 1.01f);
        }
    }
}
