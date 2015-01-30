// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class GeometricMultiTexcoordPrimitive
    {

        /// <summary>
        /// A sphere primitive.
        /// </summary>
        public struct Sphere
        {
            /// <summary>
            /// Creates a sphere primitive.
            /// </summary>
            /// <param name="device">The device.</param>
            /// <param name="diameter">The diameter.</param>
            /// <param name="tessellation">The tessellation.</param>
            /// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is false.</param>
            /// <returns>A sphere primitive.</returns>
            /// <exception cref="System.ArgumentOutOfRangeException">tessellation;Must be >= 3</exception>
            public static GeometricMultiTexcoordPrimitive New(GraphicsDevice device, float diameter = 1.0f, int tessellation = 16, bool toLeftHanded = false)
            {
                return new GeometricMultiTexcoordPrimitive(device, New(diameter, tessellation, toLeftHanded));
            }

            /// <summary>
            /// Creates a sphere primitive.
            /// </summary>
            /// <param name="diameter">The diameter.</param>
            /// <param name="tessellation">The tessellation.</param>
            /// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is false.</param>
            /// <returns>A sphere primitive.</returns>
            /// <exception cref="System.ArgumentOutOfRangeException">tessellation;Must be >= 3</exception>
            public static GeometricMeshData<VertexPositionNormalTangentMultiTexture> New(float diameter = 1.0f, int tessellation = 16, bool toLeftHanded = false)
            {
                if (tessellation < 3) throw new ArgumentOutOfRangeException("tessellation", "Must be >= 3");

                int verticalSegments = tessellation;
                int horizontalSegments = tessellation * 2;

                var vertices = new VertexPositionNormalTangentMultiTexture[(verticalSegments + 1) * (horizontalSegments + 1)];
                var indices = new int[(verticalSegments) * (horizontalSegments + 1) * 6];

                float radius = diameter / 2;

                int vertexCount = 0;
                // Create rings of vertices at progressively higher latitudes.
                for (int i = 0; i <= verticalSegments; i++)
                {
                    float v = 1.0f - (float)i / verticalSegments;

                    var latitude = (float)((i * Math.PI / verticalSegments) - Math.PI / 2.0);
                    var dy = (float)Math.Sin(latitude);
                    var dxz = (float)Math.Cos(latitude);

                    // Create a single ring of vertices at this latitude.
                    for (int j = 0; j <= horizontalSegments; j++)
                    {
                        float u = (float)j / horizontalSegments;

                        var longitude = (float)(j * 2.0 * Math.PI / horizontalSegments);
                        var dx = (float)Math.Sin(longitude);
                        var dz = (float)Math.Cos(longitude);

                        dx *= dxz;
                        dz *= dxz;

                        var normal = new Vector3(dx, dy, dz);
                        var textureCoordinate = new Vector2(u, v);

                        var tangent = new Vector4(normal.Z, 0, -normal.X, 0); // Y ^ normal
                        vertices[vertexCount++] = new VertexPositionNormalTangentMultiTexture(normal * radius, normal, tangent, textureCoordinate);
                    }
                }

                // Fill the index buffer with triangles joining each pair of latitude rings.
                int stride = horizontalSegments + 1;

                int indexCount = 0;
                for (int i = 0; i < verticalSegments; i++)
                {
                    for (int j = 0; j <= horizontalSegments; j++)
                    {
                        int nextI = i + 1;
                        int nextJ = (j + 1) % stride;

                        indices[indexCount++] = (i * stride + j);
                        indices[indexCount++] = (nextI * stride + j);
                        indices[indexCount++] = (i * stride + nextJ);

                        indices[indexCount++] = (i * stride + nextJ);
                        indices[indexCount++] = (nextI * stride + j);
                        indices[indexCount++] = (nextI * stride + nextJ);
                    }
                }

                // Create the primitive object.
                // Create the primitive object.
                return new GeometricMeshData<VertexPositionNormalTangentMultiTexture>(vertices, indices, toLeftHanded) { Name = "Sphere" };
            }
        }
    }
}
