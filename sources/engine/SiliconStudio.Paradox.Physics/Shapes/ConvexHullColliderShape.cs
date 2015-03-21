// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Paradox.Physics
{
    public class ConvexHullColliderShape : ColliderShape
    {
        private readonly IReadOnlyList<Vector3> pointsList;
        private readonly IReadOnlyCollection<uint> indicesList; 

        public ConvexHullColliderShape(IReadOnlyList<Vector3> points, IReadOnlyCollection<uint> indices)
        {
            Type = ColliderShapeTypes.ConvexHull;
            Is2D = false;

            InternalShape = new BulletSharp.ConvexHullShape(points);

            DebugPrimitiveMatrix = Matrix.Scaling(new Vector3(1, 1, 1) * 1.01f);

            pointsList = points;
            indicesList = indices;
        }

        public override GeometricPrimitive CreateDebugPrimitive(GraphicsDevice device)
        {
            var verts = new VertexPositionNormalTexture[pointsList.Count];
            for (var i = 0; i < pointsList.Count; i++)
            {
                verts[i].Position = pointsList[i];
                verts[i].TextureCoordinate = Vector2.Zero;
                verts[i].Normal = Vector3.Zero;
            }

            //calculate basic normals
            //todo verify, winding order might be wrong?
            for (var i = 0; i < indicesList.Count; i += 3)
            {
                var a = verts[i];
                var b = verts[i + 1];
                var c = verts[i + 2];
                var n = Vector3.Cross((b.Position - a.Position), (c.Position - a.Position));
                n.Normalize();
                verts[i].Normal = verts[i + 1].Normal = verts[i + 2].Normal = n;
            }

            var intIndices = indicesList.Select(x => (int)x).ToArray();
            var meshData = new GeometricMeshData<VertexPositionNormalTexture>(verts, intIndices, false);

            return new GeometricPrimitive(device, meshData);
        }
    }
}