// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class GeometricMultiTexcoordPrimitive
    {
        /// <summary>
        /// A Torus primitive.
        /// </summary>
        public struct Torus
        {
            /// <summary>
            /// Creates a torus primitive.
            /// </summary>
            /// <param name="device">The device.</param>
            /// <param name="diameter">The diameter.</param>
            /// <param name="thickness">The thickness.</param>
            /// <param name="tessellation">The tessellation.</param>
            /// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is false.</param>
            /// <returns>A Torus primitive.</returns>
            /// <exception cref="System.ArgumentOutOfRangeException">tessellation;tessellation parameter out of range</exception>
            public static GeometricMultiTexcoordPrimitive New(GraphicsDevice device, float diameter = 1.0f, float thickness = 0.33333f, int tessellation = 32, bool toLeftHanded = false)
            {
                return new GeometricMultiTexcoordPrimitive(device, New(diameter, thickness, tessellation, toLeftHanded));
            }

            /// <summary>
            /// Creates a torus primitive.
            /// </summary>
            /// <param name="diameter">The diameter.</param>
            /// <param name="thickness">The thickness.</param>
            /// <param name="tessellation">The tessellation.</param>
            /// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is false.</param>
            /// <returns>A Torus primitive.</returns>
            /// <exception cref="System.ArgumentOutOfRangeException">tessellation;tessellation parameter out of range</exception>
            public static GeometricMeshData<VertexPositionNormalTangentMultiTexture> New(float diameter = 1.0f, float thickness = 0.33333f, int tessellation = 32, bool toLeftHanded = false)
            {
                var vertices = new List<VertexPositionNormalTangentMultiTexture>();
                var indices = new List<int>();

                if (tessellation < 3)
                    throw new ArgumentOutOfRangeException("tessellation");

                int stride = tessellation + 1;

                // First we loop around the main ring of the torus.
                for (int i = 0; i <= tessellation; i++)
                {
                    float u = (float)i / tessellation;

                    float outerAngle = i * MathUtil.TwoPi / tessellation - MathUtil.PiOverTwo;

                    // Create a transform matrix that will align geometry to
                    // slice perpendicularly though the current ring position.
                    var transform = Matrix.Translation(diameter / 2, 0, 0) * Matrix.RotationY(outerAngle);

                    // Now we loop along the other axis, around the side of the tube.
                    for (int j = 0; j <= tessellation; j++)
                    {
                        float v = 1 - (float)j / tessellation;

                        float innerAngle = j * MathUtil.TwoPi / tessellation + MathUtil.Pi;
                        float dx = (float)Math.Cos(innerAngle), dy = (float)Math.Sin(innerAngle);

                        // Create a vertex.
                        var normal = new Vector3(dx, dy, 0);
                        var position = normal * thickness / 2;
                        var textureCoordinate = new Vector2(u, v);

                        Vector3.TransformCoordinate(ref position, ref transform, out position);
                        Vector3.TransformNormal(ref normal, ref transform, out normal);

                        var tangent = new Vector4((float)Math.Sin(outerAngle), 0, -(float)Math.Cos(outerAngle), 0); // Y ^ (cos, 0, sin)
                        vertices.Add(new VertexPositionNormalTangentMultiTexture(position, normal, tangent, textureCoordinate));

                        // And create indices for two triangles.
                        int nextI = (i + 1) % stride;
                        int nextJ = (j + 1) % stride;

                        indices.Add(i * stride + j);
                        indices.Add(i * stride + nextJ);
                        indices.Add(nextI * stride + j);

                        indices.Add(i * stride + nextJ);
                        indices.Add(nextI * stride + nextJ);
                        indices.Add(nextI * stride + j);
                    }
                }

                // Create the primitive object.
                return new GeometricMeshData<VertexPositionNormalTangentMultiTexture>(vertices.ToArray(), indices.ToArray(), toLeftHanded) { Name = "Torus" };
            }
        }
    }
}