// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class GeometricMultiTexcoordPrimitive
    {
        /// <summary>
        /// A cube has six faces, each one pointing in a different direction.
        /// </summary>
        public struct Cube
        {
            private const int CubeFaceCount = 6;

            private static readonly Vector3[] faceNormals =
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 0, -1),
                new Vector3(1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, -1, 0)
            };

            private static readonly Vector2[] textureCoordinates =
            {
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
                new Vector2(0, 0)
            };

            /// <summary>
            /// Creates a cube with six faces each one pointing in a different direction.
            /// </summary>
            /// <param name="device">The device.</param>
            /// <param name="size">The size.</param>
            /// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is false.</param>
            /// <returns>A cube.</returns>
            public static GeometricMultiTexcoordPrimitive New(GraphicsDevice device, float size = 1.0f, bool toLeftHanded = false)
            {
                // Create the primitive object.
                return new GeometricMultiTexcoordPrimitive(device, New(size, toLeftHanded));
            }
            
            /// <summary>
            /// Creates a cube with six faces each one pointing in a different direction.
            /// </summary>
            /// <param name="size">The size.</param>
            /// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is false.</param>
            /// <returns>A cube.</returns>
            public static GeometricMeshData<VertexPositionNormalTangentMultiTexture> New(float size = 1.0f, bool toLeftHanded = false)
            {
                var vertices = new VertexPositionNormalTangentMultiTexture[CubeFaceCount * 4];
                var indices = new int[CubeFaceCount * 6];

                size /= 2.0f;

                int vertexCount = 0;
                int indexCount = 0;
                // Create each face in turn.
                for (int i = 0; i < CubeFaceCount; i++)
                {
                    Vector3 normal = faceNormals[i];

                    // Get two vectors perpendicular both to the face normal and to each other.
                    Vector3 basis = (i >= 4) ? Vector3.UnitZ : Vector3.UnitY;

                    Vector3 side1;
                    Vector3.Cross(ref normal, ref basis, out side1);

                    Vector3 side2;
                    Vector3.Cross(ref normal, ref side1, out side2);

                    // Six indices (two triangles) per face.
                    int vbase = i * 4;
                    indices[indexCount++] = (vbase + 0);
                    indices[indexCount++] = (vbase + 1);
                    indices[indexCount++] = (vbase + 2);

                    indices[indexCount++] = (vbase + 0);
                    indices[indexCount++] = (vbase + 2);
                    indices[indexCount++] = (vbase + 3);

                    // Four vertices per face.
                    vertices[vertexCount++] = new VertexPositionNormalTangentMultiTexture((normal - side1 - side2) * size, normal, new Vector4(-side1, 0), textureCoordinates[0]);
                    vertices[vertexCount++] = new VertexPositionNormalTangentMultiTexture((normal - side1 + side2) * size, normal, new Vector4(-side1, 0), textureCoordinates[1]);
                    vertices[vertexCount++] = new VertexPositionNormalTangentMultiTexture((normal + side1 + side2) * size, normal, new Vector4(-side1, 0), textureCoordinates[2]);
                    vertices[vertexCount++] = new VertexPositionNormalTangentMultiTexture((normal + side1 - side2) * size, normal, new Vector4(-side1, 0), textureCoordinates[3]);
                }

                // Create the primitive object.
                return new GeometricMeshData<VertexPositionNormalTangentMultiTexture>(vertices, indices, toLeftHanded) { Name = "Cube" };
            }
        }
    }
}