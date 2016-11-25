// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    [DataContract]
    internal class NavigationMeshInputBuilder
    {
        public BoundingBox BoundingBox = BoundingBox.Empty;
        public List<Vector3> Points = new List<Vector3>();
        public List<int> Indices = new List<int>();

        /// <summary>
        /// Appends another vertex data builder
        /// </summary>
        /// <param name="other"></param>
        public void AppendOther(NavigationMeshInputBuilder other)
        {
            // Copy vertices
            int vbase = Points.Count;
            for (int i = 0; i < other.Points.Count; i++)
            {
                Vector3 point = other.Points[i];
                Points.Add(point);
                BoundingBox.Merge(ref BoundingBox, ref point, out BoundingBox);
            }

            // Copy indices with offset applied
            foreach (int index in other.Indices)
                Indices.Add(index + vbase);
        }

        public void AppendArrays(Vector3[] vertices, int[] indices, Matrix objectTransform)
        {
            // Copy vertices
            int vbase = Points.Count;
            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = Vector3.Transform(vertices[i], objectTransform).XYZ();
                Points.Add(vertex);
                BoundingBox.Merge(ref BoundingBox, ref vertex, out BoundingBox);
            }

            // Copy indices with offset applied
            foreach (int index in indices)
            {
                Indices.Add(index + vbase);
            }
        }

        public void AppendArrays(Vector3[] vertices, int[] indices)
        {
            // Copy vertices
            int vbase = Points.Count;
            for (int i = 0; i < vertices.Length; i++)
            {
                Points.Add(vertices[i]);
                BoundingBox.Merge(ref BoundingBox, ref vertices[i], out BoundingBox);
            }

            // Copy indices with offset applied
            foreach (int index in indices)
            {
                Indices.Add(index + vbase);
            }
        }

        /// <summary>
        /// Appends local mesh data transformed with and object transform
        /// </summary>
        /// <param name="meshData"></param>
        /// <param name="objectTransform"></param>
        public void AppendMeshData(GeometricMeshData<VertexPositionNormalTexture> meshData, Matrix objectTransform)
        {
            // Transform box points
            int vbase = Points.Count;
            for (int i = 0; i < meshData.Vertices.Length; i++)
            {
                VertexPositionNormalTexture point = meshData.Vertices[i];
                point.Position = Vector3.Transform(point.Position, objectTransform).XYZ();
                Points.Add(point.Position);
                BoundingBox.Merge(ref BoundingBox, ref point.Position, out BoundingBox);
            }

            // Copy indices with offset applied
            for (int i = 0; i < meshData.Indices.Length; i++)
            {
                Indices.Add(meshData.Indices[i] + vbase);
            }
        }
    }
}