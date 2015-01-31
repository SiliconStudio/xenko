// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class GeometricPrimitive
    {
        /// <summary>
        /// A cone with a circular base and rolled face.
        /// </summary>
        public static class Cone
        {
            /// <summary>
            /// Creates a cone a circular base and a rolled face.
            /// </summary>
            /// <param name="device">The device.</param>
            /// <param name="radius">The radius or the base</param>
            /// <param name="height">The height of the cone</param>
            /// <param name="tessellation">The number of segments composing the base</param>
            /// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is false.</param>
            /// <returns>A cone.</returns>
            public static GeometricPrimitive<VertexPositionNormalTexture> New(GraphicsDevice device, float radius = 0.5f, float height = 1.0f, int tessellation = 32, bool toLeftHanded = false)
            {
                // Create the primitive object.
                return new GeometricPrimitive<VertexPositionNormalTexture>(device, New(radius, height, tessellation, toLeftHanded));
            }

            /// <summary>
            /// Creates a cone a circular base and a rolled face.
            /// </summary>
            /// <param name="radius">The radius or the base</param>
            /// <param name="height">The height of the cone</param>
            /// <param name="tessellation">The number of segments composing the base</param>
            /// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is false.</param>
            /// <returns>A cone.</returns>
            public static GeometricMeshData<VertexPositionNormalTexture> New(float radius = 0.5f, float height = 1.0f, int tessellation = 32, bool toLeftHanded = false)
            {
                var indices = new int[6*tessellation];
                var vertices = new VertexPositionNormalTexture[3 * tessellation + 1];

                var slopeLength = Math.Sqrt(radius * radius + height * height);
                var slopeCos = radius / slopeLength;
                var slopeSin = height / slopeLength;

                var index = 0;
                var vertice = 0;

                // Cone
                for (int i = 0; i < tessellation; ++i)
                {
                    var angle = i / (double)tessellation * 2.0 * Math.PI;
                    var angleTop = (i + 0.5) / tessellation * 2.0 * Math.PI;

                    var position = new Vector3(0.0f, (float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
                    var normal = new Vector3((float)slopeSin, (float)(Math.Cos(angle) * slopeCos), (float)(Math.Sin(angle) * slopeSin));
                    var normalTop = new Vector3((float)slopeSin, (float)(Math.Cos(angleTop) * slopeCos), (float)(Math.Sin(angleTop) * slopeSin));

                    var textureCoordinate = new Vector2((float)i / tessellation, 0);

                    vertices[vertice++] = new VertexPositionNormalTexture { Position = new Vector3(height, 0.0f, 0.0f), Normal = normalTop, TextureCoordinate = textureCoordinate };
                    vertices[vertice++] = new VertexPositionNormalTexture { Position = position, Normal = normal, TextureCoordinate = textureCoordinate + Vector2.UnitY };

                    indices[index++] = i * 2;
                    indices[index++] = (i * 2 + 3) % (tessellation * 2);
                    indices[index++] = i * 2 + 1;
                }

                // End cap
                vertices[vertice++] = new VertexPositionNormalTexture { Position = new Vector3(), Normal = -Vector3.UnitX };
                for (int i = 0; i < tessellation; ++i)
                {
                    var angle = i / (double)tessellation * 2 * Math.PI;
                    var position = new Vector3(0.0f, (float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
                    vertices[vertice++] = (new VertexPositionNormalTexture { Position = position, Normal = -Vector3.UnitX });

                    indices[index++] = tessellation * 2;
                    indices[index++] = tessellation * 2 + 1 + i;
                    indices[index++] = tessellation * 2 + 1 + ((i + 1) % tessellation);
                }

                return new GeometricMeshData<VertexPositionNormalTexture>(vertices, indices, toLeftHanded) { Name = "Cone" };
            }
        }
    }
}