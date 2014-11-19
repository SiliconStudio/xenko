// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class GeometricMultiTexcoordPrimitive
    {
        /// <summary>
        /// A Cylinder primitive.
        /// </summary>
        public struct Cylinder
        {
            // Helper computes a point on a unit circle, aligned to the x/z plane and centered on the origin.
            private static Vector3 GetCircleVector(int i, int tessellation)
            {
                var angle = (float)(i * 2.0 * Math.PI / tessellation);
                var dx = (float)Math.Sin(angle);
                var dz = (float)Math.Cos(angle);

                return new Vector3(dx, 0, dz);
            }

            // Helper creates a triangle fan to close the end of a cylinder.
            private static void CreateCylinderCap(List<VertexPositionNormalTangentMultiTexture> vertices, List<int> indices, int tessellation, float height, float radius, bool isTop)
            {
                // Create cap indices.
                for (int i = 0; i < tessellation - 2; i++)
                {
                    int i1 = (i + 1) % tessellation;
                    int i2 = (i + 2) % tessellation;

                    if (isTop)
                    {
                        Utilities.Swap(ref i1, ref i2);
                    }

                    int vbase = vertices.Count;
                    indices.Add(vbase);
                    indices.Add(vbase + i1);
                    indices.Add(vbase + i2);
                }

                // Which end of the cylinder is this?
                var normal = Vector3.UnitY;
                var textureScale = new Vector2(-0.5f);

                if (!isTop)
                {
                    normal = -normal;
                    textureScale.X = -textureScale.X;
                }

                // Create cap vertices.
                for (int i = 0; i < tessellation; i++)
                {
                    var circleVector = GetCircleVector(i, tessellation);
                    var position = (circleVector * radius) + (normal * height);
                    var textureCoordinate = new Vector2(circleVector.X * textureScale.X + 0.5f, circleVector.Z * textureScale.Y + 0.5f);

                    vertices.Add(new VertexPositionNormalTangentMultiTexture(position, normal, new Vector4(isTop ? -1 : 1, 0, 0, 0), textureCoordinate));
                }
            }

            /// <summary>
            /// Creates a cylinder primitive.
            /// </summary>
            /// <param name="device">The device.</param>
            /// <param name="height">The height.</param>
            /// <param name="diameter">The diameter.</param>
            /// <param name="tessellation">The tessellation.</param>
            /// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is false.</param>
            /// <returns>A cylinder primitive.</returns>
            /// <exception cref="System.ArgumentOutOfRangeException">tessellation;tessellation must be &gt;= 3</exception>
            public static GeometricMultiTexcoordPrimitive New(GraphicsDevice device, float height = 1.0f, float diameter = 1.0f, int tessellation = 32, bool toLeftHanded = false)
            {
                // Create the primitive object.
                return new GeometricMultiTexcoordPrimitive(device, New(height, diameter, tessellation, toLeftHanded));
            }

            /// <summary>
            /// Creates a cylinder primitive.
            /// </summary>
            /// <param name="height">The height.</param>
            /// <param name="diameter">The diameter.</param>
            /// <param name="tessellation">The tessellation.</param>
            /// <param name="toLeftHanded">if set to <c>true</c> vertices and indices will be transformed to left handed. Default is false.</param>
            /// <returns>A cylinder primitive.</returns>
            /// <exception cref="System.ArgumentOutOfRangeException">tessellation;tessellation must be &gt;= 3</exception>
            public static GeometricMeshData<VertexPositionNormalTangentMultiTexture> New(float height = 1.0f, float diameter = 1.0f, int tessellation = 32, bool toLeftHanded = false)
            {
                if (tessellation < 3)
                    throw new ArgumentOutOfRangeException("tessellation", @"tessellation must be >= 3");

                var vertices = new List<VertexPositionNormalTangentMultiTexture>();
                var indices = new List<int>();

                height /= 2;

                var topOffset = Vector3.UnitY * height;

                float radius = diameter / 2;
                int stride = tessellation + 1;

                // Create a ring of triangles around the outside of the cylinder.
                for (int i = 0; i <= tessellation; i++)
                {
                    var normal = GetCircleVector(i, tessellation);

                    var sideOffset = normal * radius;

                    var textureCoordinate = new Vector2((float)i / tessellation, 0);

                    var tangent = new Vector4(normal.Z, 0, -normal.X, 0); // Y ^ normal
                    vertices.Add(new VertexPositionNormalTangentMultiTexture(sideOffset + topOffset, normal, tangent, textureCoordinate));
                    vertices.Add(new VertexPositionNormalTangentMultiTexture(sideOffset - topOffset, normal, tangent, textureCoordinate + Vector2.UnitY));

                    indices.Add(i * 2);
                    indices.Add((i * 2 + 2) % (stride * 2));
                    indices.Add(i * 2 + 1);

                    indices.Add(i * 2 + 1);
                    indices.Add((i * 2 + 2) % (stride * 2));
                    indices.Add((i * 2 + 3) % (stride * 2));
                }

                // Create flat triangle fan caps to seal the top and bottom.
                CreateCylinderCap(vertices, indices, tessellation, height, radius, true);
                CreateCylinderCap(vertices, indices, tessellation, height, radius, false);

                // Create the primitive object.
                return new GeometricMeshData<VertexPositionNormalTangentMultiTexture>(vertices.ToArray(), indices.ToArray(), toLeftHanded) { Name = "Cylinder" };
            }
        }
    }
}