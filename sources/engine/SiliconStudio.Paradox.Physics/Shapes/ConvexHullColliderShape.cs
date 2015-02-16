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
        public ConvexHullColliderShape(IReadOnlyList<Vector3> points, IReadOnlyCollection<uint> indices)
        {
            Type = ColliderShapeTypes.ConvexHull;
            Is2D = false;

            InternalShape = new BulletSharp.ConvexHullShape(points);

            if (!Simulation.CreateDebugPrimitives) return;

            var verts = new VertexPositionNormalTexture[points.Count];
            for (var i = 0; i < points.Count; i++)
            {
                verts[i].Position = points[i];
                verts[i].TextureCoordinate = Vector2.Zero;
                verts[i].Normal = Vector3.Zero;
            }
            
            //calculate basic normals
            //todo verify, winding order might be wrong?
            for (var i = 0; i < indices.Count; i += 3)
            {
                var a = verts[i];
                var b = verts[i + 1];
                var c = verts[i + 2];
                var n = Vector3.Cross((b.Position - a.Position), (c.Position - a.Position));
                n.Normalize();
                verts[i].Normal = verts[i + 1].Normal = verts[i + 2].Normal = n;
            }

            var intIndices = indices.Select(x => (int)x).ToArray();
            var meshData = new GeometricMeshData<VertexPositionNormalTexture>(verts, intIndices, false);

            DebugPrimitive = new GeometricPrimitive(Simulation.DebugGraphicsDevice, meshData);
            DebugPrimitiveScaling = Matrix.Scaling(new Vector3(1, 1, 1) * 1.01f);
        }
    }
}