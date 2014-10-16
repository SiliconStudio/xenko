// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Extensions;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics.Data;

namespace SiliconStudio.Paradox.DataModel
{
    public static class MeshDataHelper
    {
        public static MeshDraw ToMeshDraw<T>(this GeometricPrimitive<T> primitive) where T : struct, IVertexWindable
        {
            var vertexDeclaration = VertexPositionNormalTexture.Layout;
            var vertexBufferBinding = new VertexBufferBinding(primitive.VertexBuffer, vertexDeclaration, primitive.VertexBuffer.ElementCount);
            var indexBufferBinding = new IndexBufferBinding(primitive.IndexBuffer, primitive.IsIndex32Bits, primitive.IndexBuffer.ElementCount);
            var data = new MeshDraw();
            data.StartLocation = 0;
            data.PrimitiveType = PrimitiveType.TriangleList;
            data.VertexBuffers = new VertexBufferBinding[] { vertexBufferBinding };
            data.IndexBuffer = indexBufferBinding;
            data.DrawCount = primitive.IndexBuffer.ElementCount;

            return data;
        }

        public static MeshDraw CreateBox(GraphicsDevice graphicsDevice, float size, Color4 mainColor, bool simulateLight = false)
        {
            return CreateBox(graphicsDevice, size, size, size, mainColor, simulateLight);
        }

        public static MeshDraw CreateBox(GraphicsDevice graphicsDevice, float sizeX, float sizeY, float sizeZ, Color4 mainColor, bool simulateLight = false)
        {
            // Prepare colors
            var frontColor = mainColor;
            var backColor = frontColor;

            var basicColorHSV = ColorHSV.FromColor(mainColor);
            if (simulateLight)
                basicColorHSV.V = basicColorHSV.V * 0.8f;

            var leftColor = basicColorHSV.ToColor();
            var rightColor = leftColor;

            if (simulateLight)
                basicColorHSV.V = basicColorHSV.V * 0.8f;

            var bottomColor = basicColorHSV.ToColor();
            var topColor = bottomColor;

            return CreateBox(graphicsDevice, sizeX, sizeY, sizeZ, backColor, frontColor, bottomColor, topColor, leftColor, rightColor);
        }

        public static MeshDraw CreatePlane(GraphicsDevice graphicsDevice, float sizeX, float sizeY, Color4 color, bool inverseYTexCoord = false, float tilingU = 1.0f, float tilingV = 1.0f)
        {
            var normal = new Vector3(0.0f, 0.0f, 1.0f);

            var vertices = new[]
                {
                    new VertexNormalColorTexture(new Vector3(-1.0f, -1.0f, -1.0f), normal, color, new Vector2(0.0f, 0.0f)),
                    new VertexNormalColorTexture(new Vector3(-1.0f, 1.0f, -1.0f),  normal, color, new Vector2(0.0f, tilingV)),
                    new VertexNormalColorTexture(new Vector3(1.0f, 1.0f, -1.0f),   normal, color, new Vector2(tilingU, tilingV)),
                    new VertexNormalColorTexture(new Vector3(-1.0f, -1.0f, -1.0f), normal, color, new Vector2(0.0f, 0.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, 1.0f, -1.0f),   normal, color, new Vector2(tilingU, tilingV)),
                    new VertexNormalColorTexture(new Vector3(1.0f, -1.0f, -1.0f),  normal, color, new Vector2(tilingU, 0.0f)),
                };

            if (inverseYTexCoord)
            {
                vertices[0].TexCoord[1] = 1;
                vertices[1].TexCoord[1] = 0;
                vertices[2].TexCoord[1] = 0;
                vertices[3].TexCoord[1] = 1;
                vertices[4].TexCoord[1] = 0;
                vertices[5].TexCoord[1] = 1;
            }

            var scale = new Vector3(sizeX / 2.0f, sizeY / 2.0f, 1.0f);
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Position = Vector3.Modulate(vertices[i].Position, scale);

            var buffer = Graphics.Buffer.Vertex.New(graphicsDevice, vertices);

            var data = new MeshDraw { DrawCount = vertices.Length, PrimitiveType = PrimitiveType.TriangleList };
            data.VertexBuffers = new[]
                {
                    new VertexBufferBinding(buffer,
                        new VertexDeclaration(
                            VertexElement.Position<Vector3>(),
                            VertexElement.Normal<Vector3>(),
                            VertexElement.Color<Vector4>(),
                            VertexElement.TextureCoordinate<Vector2>()),
                        vertices.Length)
                };

            return data;
        }

        public static MeshDraw CreateBox(GraphicsDevice graphicsDevice, float sizeX, float sizeY, float sizeZ, Color4 backColor, Color4 frontColor, Color4 bottomColor, Color4 topColor, Color4 leftColor, Color4 rightColor)
        {
            var backNormal = new Vector3(0.0f, 0.0f, -1.0f);
            var frontNormal = new Vector3(0.0f, 0.0f, 1.0f);
            var bottomNormal = new Vector3(0.0f, -1.0f, 0.0f);
            var topNormal = new Vector3(0.0f, 1.0f, 0.0f);
            var leftNormal = new Vector3(-1.0f, 0.0f, 0.0f);
            var rightNormal = new Vector3(1.0f, 0.0f, 0.0f);

            var vertices = new[]
                {
                    new VertexNormalColorTexture(new Vector3(-1.0f, -1.0f, -1.0f), backNormal, backColor, new Vector2(0.0f, 0.0f)), // Back
                    new VertexNormalColorTexture(new Vector3(-1.0f, 1.0f, -1.0f),  backNormal, backColor, new Vector2(0.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, 1.0f, -1.0f),   backNormal, backColor, new Vector2(1.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(-1.0f, -1.0f, -1.0f), backNormal, backColor, new Vector2(0.0f, 0.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, 1.0f, -1.0f),   backNormal, backColor, new Vector2(1.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, -1.0f, -1.0f),  backNormal, backColor, new Vector2(1.0f, 0.0f)),

                    new VertexNormalColorTexture(new Vector3(-1.0f, -1.0f, 1.0f),  frontNormal, frontColor, new Vector2(0.0f, 0.0f)), // Front
                    new VertexNormalColorTexture(new Vector3(1.0f, 1.0f, 1.0f),    frontNormal, frontColor, new Vector2(1.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(-1.0f, 1.0f, 1.0f),   frontNormal, frontColor, new Vector2(0.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(-1.0f, -1.0f, 1.0f),  frontNormal, frontColor, new Vector2(0.0f, 0.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, -1.0f, 1.0f),   frontNormal, frontColor, new Vector2(1.0f, 0.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, 1.0f, 1.0f),    frontNormal, frontColor, new Vector2(1.0f, 1.0f)),

                    new VertexNormalColorTexture(new Vector3(-1.0f, -1.0f, -1.0f), bottomNormal, bottomColor, new Vector2(0.0f, 0.0f)), // Bottom
                    new VertexNormalColorTexture(new Vector3(1.0f, -1.0f, 1.0f),   bottomNormal, bottomColor, new Vector2(1.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(-1.0f, -1.0f, 1.0f),  bottomNormal, bottomColor, new Vector2(0.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(-1.0f, -1.0f, -1.0f), bottomNormal, bottomColor, new Vector2(0.0f, 0.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, -1.0f, -1.0f),  bottomNormal, bottomColor, new Vector2(1.0f, 0.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, -1.0f, 1.0f),   bottomNormal, bottomColor, new Vector2(1.0f, 1.0f)),

                    new VertexNormalColorTexture(new Vector3(-1.0f, 1.0f, -1.0f),  topNormal, topColor, new Vector2(0.0f, 0.0f)), // Top
                    new VertexNormalColorTexture(new Vector3(-1.0f, 1.0f, 1.0f),   topNormal, topColor, new Vector2(0.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, 1.0f, 1.0f),    topNormal, topColor, new Vector2(1.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(-1.0f, 1.0f, -1.0f),  topNormal, topColor, new Vector2(0.0f, 0.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, 1.0f, 1.0f),    topNormal, topColor, new Vector2(1.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, 1.0f, -1.0f),   topNormal, topColor, new Vector2(1.0f, 0.0f)),

                    new VertexNormalColorTexture(new Vector3(-1.0f, -1.0f, -1.0f), leftNormal, leftColor, new Vector2(0.0f, 0.0f)), // Left
                    new VertexNormalColorTexture(new Vector3(-1.0f, -1.0f, 1.0f),  leftNormal, leftColor, new Vector2(0.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(-1.0f, 1.0f, 1.0f),   leftNormal, leftColor, new Vector2(1.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(-1.0f, -1.0f, -1.0f), leftNormal, leftColor, new Vector2(0.0f, 0.0f)),
                    new VertexNormalColorTexture(new Vector3(-1.0f, 1.0f, 1.0f),   leftNormal, leftColor, new Vector2(1.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(-1.0f, 1.0f, -1.0f),  leftNormal, leftColor, new Vector2(1.0f, 0.0f)),

                    new VertexNormalColorTexture(new Vector3(1.0f, -1.0f, -1.0f),  rightNormal, rightColor, new Vector2(0.0f, 0.0f)), // Right
                    new VertexNormalColorTexture(new Vector3(1.0f, 1.0f, 1.0f),    rightNormal, rightColor, new Vector2(1.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, -1.0f, 1.0f),   rightNormal, rightColor, new Vector2(0.0f, 1.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, -1.0f, -1.0f),  rightNormal, rightColor, new Vector2(0.0f, 0.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, 1.0f, -1.0f),   rightNormal, rightColor, new Vector2(1.0f, 0.0f)),
                    new VertexNormalColorTexture(new Vector3(1.0f, 1.0f, 1.0f),    rightNormal, rightColor, new Vector2(1.0f, 1.0f)),
                };

            var scale = new Vector3(sizeX / 2.0f, sizeY / 2.0f, sizeZ / 2.0f);
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Position = Vector3.Modulate(vertices[i].Position, scale);

            var buffer = Graphics.Buffer.Vertex.New(graphicsDevice, vertices);

            var data = new MeshDraw { DrawCount = vertices.Length, PrimitiveType = PrimitiveType.TriangleList };
            data.VertexBuffers = new[]
                {
                    new VertexBufferBinding(buffer,
                        new VertexDeclaration(
                            VertexElement.Position<Vector3>(),
                            VertexElement.Normal<Vector3>(),
                            VertexElement.Color<Vector4>(),
                            VertexElement.TextureCoordinate<Vector2>()),
                        vertices.Length)
                };

            return data;
        }


        private struct CosSin
        {
            public double Cos;
            public double Sin;
        }

        private static CosSin[] CosSinTable(int size, bool negate = false)
        {
            // Create a table with size + 1 in order to support indexing + 1 without using modulo
            var result = new CosSin[size + 1];

            // Delta of a slice
            var deltaAngle = 2.0 * Math.PI * ((negate) ? -1.0 : 1.0) / size;

            result[0].Cos = 1.0;
            result[0].Sin = 0.0;

            for(int i = 1; i < size; i++)
            {
                result[i].Cos = Math.Cos(deltaAngle * i);
                result[i].Sin = Math.Sin(deltaAngle * i);
            }

            result[size].Cos = 1.0;
            result[size].Sin = 0.0;

            return result;
        }

        public static MeshDrawData CreateSphere(float radius, int slices, int stacks, Color4 color)
        {
            var vertices = new List<Vector4>();
            var indices = new List<int>();

            var slicesCosSin = CosSinTable(slices, true);
            var stacksCosSin = CosSinTable(stacks*2);

            var colorVec4 = color.ToVector4();

            // Top Vertex
            vertices.Add(new Vector4(0, 0, radius, 1.0f));
            vertices.Add(new Vector4(0, 0, 1, 0));
            vertices.Add(colorVec4);

            // Build vertex table
            for (int stackIndex = 1; stackIndex < stacks; stackIndex++ )
            {
                float stackZ = (float)stacksCosSin[stackIndex].Cos * radius;
                float radiusXY = (float)stacksCosSin[stackIndex].Sin * radius;
                for (int sliceIndex = 0; sliceIndex < slices; sliceIndex++)
                {
                    // Add Position
                    var position = new Vector4((float)slicesCosSin[sliceIndex].Cos * radiusXY, (float)slicesCosSin[sliceIndex].Sin * radiusXY, stackZ, 1.0f);
                    vertices.Add(position);

                    // Add Normal
                    position.W = 0.0f;
                    position.Normalize();
                    vertices.Add(position);

                    // Add Color
                    vertices.Add(colorVec4);
                }
            }

            int bottomIndex = vertices.Count / 3;

            // Bottom VertexNormalColor
            vertices.Add(new Vector4(0, 0, -radius, 1.0f));
            vertices.Add(new Vector4(0, 0, -1, 0));
            vertices.Add(colorVec4);

            // Build Index table for Top 
            for (int sliceIndex = 0; sliceIndex < slices; sliceIndex++)
            {
                indices.Add(0);
                indices.Add(1 + (sliceIndex + 1) % slices);
                indices.Add(1 + sliceIndex);
            }

            // Build Index table for Bottom
            int stackIndexForBottom = 1 + (stacks - 2) * slices;
            for (int sliceIndex = 0; sliceIndex < slices; sliceIndex++)
            {
                indices.Add(stackIndexForBottom + sliceIndex);
                indices.Add(stackIndexForBottom + (sliceIndex + 1) % slices);
                indices.Add(bottomIndex);
            }

            // Build vertex table
            for (int stackIndex = 1; stackIndex < (stacks - 1); stackIndex++)
            {
                int stackOffset = 1 + (stackIndex-1) * slices;
                for (int sliceIndex = 0; sliceIndex < slices; sliceIndex++)
                {
                    int sliceIndex1 = (sliceIndex + 1) % slices;
                    indices.Add(stackOffset + sliceIndex);
                    indices.Add(stackOffset + sliceIndex1);
                    indices.Add(stackOffset + slices + sliceIndex);

                    indices.Add(stackOffset + sliceIndex1);
                    indices.Add(stackOffset + slices + sliceIndex1);
                    indices.Add(stackOffset + slices + sliceIndex);
                }
            }

            var vertexArray = vertices.ToArray();
            var verticesData = new BufferData(BufferFlags.VertexBuffer, new byte[vertexArray.Length * Utilities.SizeOf<Vector4>()]);
            Utilities.Write(verticesData.Content, vertexArray, 0, vertexArray.Length);

            var indexArray = indices.ToArray();
            var indexData = new BufferData(BufferFlags.IndexBuffer, new byte[indexArray.Length * 4]);
            Utilities.Write(indexData.Content, indexArray, 0, indexArray.Length);

            var data = new MeshDrawData { DrawCount = indices.Count, PrimitiveType = PrimitiveType.TriangleList };
            
            data.IndexBuffer = 
                new IndexBufferBindingData
                {
                    Offset = 0,
                    Count = indexArray.Length,
                    Buffer = indexData,
                    Is32Bit = true
                };

            data.VertexBuffers = new VertexBufferBindingData[] {
                new VertexBufferBindingData
                {
                    Offset = 0,
                    Count = vertexArray.Length/3,
                    Buffer = verticesData,
                    Declaration = new VertexDeclaration(
                        VertexElement.Position<Vector4>(),
                        VertexElement.Normal<Vector4>(),
                        VertexElement.Color<Vector4>()
                        )
                }};

            data.CompactIndexBuffer();

            return data;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct VertexNormalColor
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Color4 Color;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct VertexNormalColorTexture
        {
            public VertexNormalColorTexture(Vector3 position, Vector3 normal, Color4 color, Vector2 texCoord)
            {
                Position = position;
                Normal = normal;
                Color = color;
                TexCoord = texCoord;
            }

            public Vector3 Position;
            public Vector3 Normal;
            public Color4 Color;
            public Vector2 TexCoord;
        }
        
        public static MeshDraw CreateCylinder(GraphicsDevice graphicsDevice, float radius, float height, int segments, Color4 color)
        {
            var indices = new List<int>();
            var vertices = new List<VertexNormalColor>();

            // Cone
            for (int i = 0; i < segments; ++i)
            {
                var angle = (double)i / (double)segments * 2.0 * Math.PI;

                var position = new Vector3(0.0f, (float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
                var normal = new Vector3(0.0f, (float)Math.Cos(angle), (float)Math.Sin(angle));

                vertices.Add(new VertexNormalColor { Position = position + Vector3.UnitX * height, Normal = normal, Color = color });
                vertices.Add(new VertexNormalColor { Position = position, Normal = normal, Color = color });

                indices.Add(i * 2);
                indices.Add(i * 2 + 1);
                indices.Add((i * 2 + 2) % (segments * 2));

                indices.Add(i * 2 + 1);
                indices.Add((i * 2 + 3) % (segments * 2));
                indices.Add((i * 2 + 2) % (segments * 2));
            }

            // build the 2 Caps
            var indiceOffset = vertices.Count;

            // 1. The two centers of the caps
            vertices.Add(new VertexNormalColor { Position = Vector3.UnitX * height, Normal = Vector3.UnitX, Color = color });
            vertices.Add(new VertexNormalColor { Position = Vector3.Zero, Normal = -Vector3.UnitX, Color = color });

            // 2. the links
            for (int i = 0; i < segments; ++i)
            {
                var angle = (double) i    / (double)segments * 2.0 * Math.PI;

                var position = new Vector3(0.0f, (float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
                var normal = Vector3.UnitX;

                vertices.Add(new VertexNormalColor { Position = position + Vector3.UnitX * height,  Normal =  normal, Color = color });
                vertices.Add(new VertexNormalColor { Position = position,                           Normal = -normal, Color = color });

                indices.Add(indiceOffset);
                indices.Add((indiceOffset + 2) + (((i + 0) * 2) % (segments * 2)));
                indices.Add((indiceOffset + 2) + (((i + 1) * 2) % (segments * 2)));

                indices.Add(indiceOffset + 1);
                indices.Add((indiceOffset + 2) + (((i + 1) * 2 + 1) % (segments * 2)));
                indices.Add((indiceOffset + 2) + (((i + 0) * 2 + 1) % (segments * 2)));
            }

            var vertexBuffer = Graphics.Buffer.Vertex.New(graphicsDevice, vertices.ToArray());
            var indexBuffer = Graphics.Buffer.Index.New(graphicsDevice, indices.ToArray());

            var data = new MeshDraw { DrawCount = indices.Count, PrimitiveType = PrimitiveType.TriangleList };

            data.IndexBuffer = new IndexBufferBinding(indexBuffer, true, indices.Count);

            data.VertexBuffers = new[]
                {
                    new VertexBufferBinding(vertexBuffer,
                        new VertexDeclaration(
                            VertexElement.Position<Vector3>(),
                            VertexElement.Normal<Vector3>(),
                            VertexElement.Color<Vector4>()),
                        vertices.Count),
                };

            // TODO: Compact
            // data.CompactIndexBuffer();

            return data;
        }

        public static MeshDraw CreateCylinderArc(GraphicsDevice graphicsDevice, float radius, float height, float angleArc, int segments, Color4 color)
        {
            var indices = new List<int>();
            var vertices = new List<VertexNormalColor>();

            var angleOffset = angleArc / 2;

            // Cone
            for (int i = 0; i < segments; ++i)
            {
                var angle = (double)i / (double)segments * angleArc - angleOffset;

                var position = new Vector3((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius, 0);
                var normal = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);

                vertices.Add(new VertexNormalColor { Position = position + Vector3.UnitZ * height, Normal = normal, Color = color });
                vertices.Add(new VertexNormalColor { Position = position, Normal = normal, Color = color });

                if (i != segments - 1)
                {
                    indices.Add(i * 2);
                    indices.Add(i * 2 + 1);
                    indices.Add((i * 2 + 2) % (segments * 2));

                    indices.Add(i * 2 + 1);
                    indices.Add((i * 2 + 3) % (segments * 2));
                    indices.Add((i * 2 + 2) % (segments * 2));
                }
            }

            // build the 2 Caps
            var indiceOffset = vertices.Count;

            // 1. The two centers of the caps
            vertices.Add(new VertexNormalColor { Position = Vector3.UnitZ* height, Normal = Vector3.UnitZ, Color = color });
            vertices.Add(new VertexNormalColor { Position = Vector3.Zero, Normal = -Vector3.UnitZ, Color = color });

            // 2. the links
            for (int i = 0; i < segments; ++i)
            {
                var angle = (double)i / (double)segments * angleArc - angleOffset;

                var position = new Vector3((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius, 0);
                var normal = Vector3.UnitZ;

                vertices.Add(new VertexNormalColor { Position = position + Vector3.UnitZ * height, Normal = normal, Color = color });
                vertices.Add(new VertexNormalColor { Position = position, Normal = -normal, Color = color });

                if (i != segments - 1)
                {
                    indices.Add(indiceOffset);
                    indices.Add((indiceOffset + 2) + (((i + 0) * 2) % (segments * 2)));
                    indices.Add((indiceOffset + 2) + (((i + 1) * 2) % (segments * 2)));

                    indices.Add(indiceOffset + 1);
                    indices.Add((indiceOffset + 2) + (((i + 1) * 2 + 1) % (segments * 2)));
                    indices.Add((indiceOffset + 2) + (((i + 0) * 2 + 1) % (segments * 2)));
                }
            }

            var vertexBuffer = Graphics.Buffer.Vertex.New(graphicsDevice, vertices.ToArray());
            var indexBuffer = Graphics.Buffer.Index.New(graphicsDevice, indices.ToArray());

            var data = new MeshDraw { DrawCount = indices.Count, PrimitiveType = PrimitiveType.TriangleList };

            data.IndexBuffer = new IndexBufferBinding(indexBuffer, true, indices.Count);

            data.VertexBuffers = new[]
                {
                    new VertexBufferBinding(vertexBuffer,
                        new VertexDeclaration(
                            VertexElement.Position<Vector3>(),
                            VertexElement.Normal<Vector3>(),
                            VertexElement.Color<Vector4>()),
                        vertices.Count),
                };

            // TODO: Compact buffers
            //data.CompactIndexBuffer();

            return data;
        }

        public static MeshDrawData CreateCone(GraphicsDevice graphicsDevice, float radius, float height, int segments, Color4 color)
        {
            var indices = new List<int>();
            var vertices = new List<VertexNormalColor>();

            var slopeLength = Math.Sqrt(radius * radius + height * height);
            var slopeCos = radius / slopeLength;
            var slopeSin = height / slopeLength;

            // Cone
            for (int i = 0; i < segments; ++i)
            {
                var angle = (double)i / (double)segments * 2.0 * Math.PI;
                var angleTop = (double)(i + 0.5) / (double)segments * 2.0 * Math.PI;

                var position = new Vector3(0.0f, (float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
                var normal = new Vector3((float)slopeSin, (float)(Math.Cos(angle) * slopeCos), (float)(Math.Sin(angle) * slopeSin));
                var normalTop = new Vector3((float)slopeSin, (float)(Math.Cos(angleTop) * slopeCos), (float)(Math.Sin(angleTop) * slopeSin));

                vertices.Add(new VertexNormalColor { Position = new Vector3(height, 0.0f, 0.0f), Normal = normalTop, Color = color });
                vertices.Add(new VertexNormalColor { Position = position, Normal = normal, Color = color });

                indices.Add(i * 2);
                indices.Add(i * 2 + 1);
                indices.Add((i * 2 + 3) % (segments * 2));
            }

            // End cap
            vertices.Add(new VertexNormalColor { Position = new Vector3(0.0f, 0.0f, 0.0f), Normal = -Vector3.UnitX, Color = color });
            for (int i = 0; i < segments; ++i)
            {
                var angle = (double)i / (double)segments * 2 * Math.PI;
                var position = new Vector3(0.0f, (float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
                vertices.Add(new VertexNormalColor { Position = position, Normal = -Vector3.UnitX, Color = color });

                indices.Add(segments * 2);
                indices.Add(segments * 2 + 1 + ((i + 1) % segments));
                indices.Add(segments * 2 + 1 + i);
            }

            var vertexArray = vertices.ToArray();
            var verticesData = new BufferData(BufferFlags.VertexBuffer, new byte[vertexArray.Length * Utilities.SizeOf<VertexNormalColor>()]);
            Utilities.Write(verticesData.Content, vertexArray, 0, vertexArray.Length);

            var indexArray = indices.ToArray();
            var indexData = new BufferData(BufferFlags.IndexBuffer, new byte[indexArray.Length * 4]);
            Utilities.Write(indexData.Content, indexArray, 0, indexArray.Length);

            var data = new MeshDrawData { DrawCount = indices.Count, PrimitiveType = PrimitiveType.TriangleList };

            data.IndexBuffer =
                new IndexBufferBindingData
                {
                    Offset = 0,
                    Count = indices.Count,
                    Buffer = indexData,
                    Is32Bit = true
                };

            data.VertexBuffers = new VertexBufferBindingData[] {
                new VertexBufferBindingData
                {
                    Offset = 0,
                    Count = vertices.Count,
                    Buffer = verticesData,
                    Declaration = new VertexDeclaration(
                        VertexElement.Position<Vector3>(),
                        VertexElement.Normal<Vector3>(),
                        VertexElement.Color<Vector4>())
                }};

            data.CompactIndexBuffer();

            return data;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct VertexColor
        {
            public Vector3 Position;
            public Color4 Color;
        }

        public static MeshDrawData CreateCircle(GraphicsDevice graphicsDevice, float radius, int segments, Color4 color)
        {
            var vertices = new VertexColor[segments + 1];
            for (int i = 0; i < segments + 1; ++i)
            {
                vertices[i] = new VertexColor { Position = new Vector3(0.0f, (float)Math.Cos((double)i / (double)segments * Math.PI * 2.0) * radius, (float)Math.Sin((double)i / (double)segments * Math.PI * 2.0) * radius), Color = color };
            }

            var verticesData = new BufferData(BufferFlags.VertexBuffer, new byte[vertices.Length * Utilities.SizeOf<VertexColor>()]);
            Utilities.Write(verticesData.Content, vertices, 0, vertices.Length);

            var data = new MeshDrawData { DrawCount = vertices.Length, PrimitiveType = PrimitiveType.LineStrip };

            data.VertexBuffers = new VertexBufferBindingData[] {
                new VertexBufferBindingData
                {
                    Offset = 0,
                    Count = vertices.Length,
                    Buffer = verticesData,
                    Declaration = new VertexDeclaration(
                        VertexElement.Position<Vector3>(),
                        VertexElement.Color<Vector4>())
                }};

            return data;
        }

        public static MeshDraw CreateCircleArc(GraphicsDevice graphicsDevice, float radius, float angle, int segments, Color4 color)
        {
            var vertices = new VertexColor[segments + 1];
            for (int i = 0; i < segments + 1; ++i)
            {
                vertices[i] = new VertexColor { Position = new Vector3(0.0f,
                                                                       (float)Math.Cos((double)i / (double)segments * angle) * radius,
                                                                       (float)Math.Sin((double)i / (double)segments * angle) * radius), 
                                                Color = color };
            }

            var buffer = Graphics.Buffer.Vertex.New(graphicsDevice, vertices);

            var data = new MeshDraw { DrawCount = vertices.Length, PrimitiveType = PrimitiveType.LineStrip };

            data.VertexBuffers = new[]
                {
                    new VertexBufferBinding(buffer, new VertexDeclaration(
                        VertexElement.Position<Vector3>(),
                        VertexElement.Color<Vector4>()), vertices.Length)
                };


            return data;
        }

        public static MeshDraw CreateLine(GraphicsDevice graphicsDevice, float length, Color4 color)
        {
            var vertices = new VertexColor[2];
            vertices[0] = new VertexColor { Position = Vector3.Zero, Color = color };
            vertices[1] = new VertexColor { Position = Vector3.UnitX * length, Color = color };

            var buffer = Graphics.Buffer.Vertex.New(graphicsDevice, vertices);

            var data = new MeshDraw { DrawCount = vertices.Length, PrimitiveType = PrimitiveType.LineList };

            data.VertexBuffers = new[]
                {
                    new VertexBufferBinding(buffer, new VertexDeclaration(
                        VertexElement.Position<Vector3>(),
                        VertexElement.Color<Vector4>()), vertices.Length)
                };

            return data;
        }
    }
}