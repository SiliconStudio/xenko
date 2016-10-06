using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    public class NavigationMeshInputBuilder
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

            // Send indices
            foreach (int index in other.Indices)
                Indices.Add(index + vbase);
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

            // Send indices
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

            // Send indices
            for (int i = 0; i < meshData.Indices.Length; i++)
            {
                Indices.Add(meshData.Indices[i] + vbase);
            }
        }
    }
}
