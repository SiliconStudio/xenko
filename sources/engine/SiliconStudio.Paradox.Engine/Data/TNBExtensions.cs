// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics.Data;

namespace SiliconStudio.Paradox.Extensions
{
    public static class TNBExtensions
    {
        /// <summary>
        /// Generates the tangents and binormals for this mesh data.
        /// Tangents and bitangents will be encoded as float4:
        /// float3 for tangent and an additional float for handedness (1 or -1),
        /// so that bitangent can be reconstructed.
        /// More info at http://www.terathon.com/code/tangent.html
        /// </summary>
        /// <param name="meshData">The mesh data.</param>
        public static unsafe void GenerateTangentBinormal(this MeshDraw meshData)
        {
            if (!meshData.IsSimple())
                throw new ArgumentException("meshData is not simple.");

            if (meshData.PrimitiveType != PrimitiveType.TriangleList
                && meshData.PrimitiveType != PrimitiveType.TriangleListWithAdjacency)
                throw new NotImplementedException();

            var oldVertexBufferBinding = meshData.VertexBuffers[0];
            var indexBufferBinding = meshData.IndexBuffer;
            var indexData = indexBufferBinding != null ? indexBufferBinding.Buffer.GetSerializationData().Content : null;

            var oldVertexStride = oldVertexBufferBinding.Declaration.VertexStride;
            var bufferData = oldVertexBufferBinding.Buffer.GetSerializationData().Content;

            fixed (byte* indexBufferStart = indexData)
            fixed (byte* oldBuffer = bufferData)
            {
                var result = GenerateTangentBinormal(oldVertexBufferBinding.Declaration, (IntPtr)oldBuffer, oldVertexBufferBinding.Count, oldVertexBufferBinding.Offset, oldVertexBufferBinding.Stride, (IntPtr)indexBufferStart, indexBufferBinding != null && indexBufferBinding.Is32Bit, indexBufferBinding != null ? indexBufferBinding.Count : 0);

                // Replace new vertex buffer binding
                meshData.VertexBuffers[0] = new VertexBufferBinding(new BufferData(BufferFlags.VertexBuffer, result.Value).ToSerializableVersion(), result.Key, oldVertexBufferBinding.Count);
            }
        }

        /// <summary>
        /// Generate Tangent BiNormal. TODO: Move this to Graphics. Make it more friendly to use.
        /// </summary>
        /// <param name="oldVertexDeclaration"></param>
        /// <param name="vertexData"></param>
        /// <param name="vertexCount"></param>
        /// <param name="vertexOffset"></param>
        /// <param name="vertexStride"></param>
        /// <param name="indexData"></param>
        /// <param name="is32BitIndex"></param>
        /// <param name="indexCountArg"></param>
        /// <returns></returns>
        public static unsafe KeyValuePair<VertexDeclaration, byte[]> GenerateTangentBinormal(VertexDeclaration oldVertexDeclaration, IntPtr vertexData, int vertexCount, int vertexOffset,  int vertexStride, IntPtr indexData, bool is32BitIndex, int indexCountArg)
        {
            var indexBufferBinding = indexData;

            var oldVertexStride = vertexStride;
            var bufferData = vertexData;

            // TODO: Usage index in key
            var offsetMapping = oldVertexDeclaration
                .EnumerateWithOffsets()
                .ToDictionary(x => x.VertexElement.SemanticAsText, x => x.Offset);

            var positionOffset = offsetMapping["POSITION"];
            var uvOffset = offsetMapping[VertexElementUsage.TextureCoordinate];
            var normalOffset = offsetMapping[VertexElementUsage.Normal];

            // Add tangent to vertex declaration
            var vertexElements = oldVertexDeclaration.VertexElements.ToList();
            if (!offsetMapping.ContainsKey(VertexElementUsage.Tangent))
                vertexElements.Add(VertexElement.Tangent<Vector4>());
            var vertexDeclaration = new VertexDeclaration(vertexElements.ToArray());
            var newVertexStride = vertexDeclaration.VertexStride;

            // Update mapping
            offsetMapping = vertexDeclaration
                .EnumerateWithOffsets()
                .ToDictionary(x => x.VertexElement.SemanticAsText, x => x.Offset);

            var tangentOffset = offsetMapping[VertexElementUsage.Tangent];

            var newBufferData = new byte[vertexCount * newVertexStride];

            var tangents = new Vector3[vertexCount];
            var bitangents = new Vector3[vertexCount];

            byte* indexBufferStart = (byte*)indexData;
            byte* oldBuffer = (byte*)bufferData + vertexOffset;
            fixed(byte* newBuffer = newBufferData)
            {
                var indexBuffer32 = indexBufferBinding != IntPtr.Zero && is32BitIndex ? (int*)indexBufferStart : null;
                var indexBuffer16 = indexBufferBinding != IntPtr.Zero && !is32BitIndex ? (short*)indexBufferStart : null;

                var indexCount = indexBufferBinding != IntPtr.Zero ? indexCountArg : vertexCount;

                for (int i = 0; i < indexCount; i += 3)
                {
                    // Get indices
                    int index1 = i + 0;
                    int index2 = i + 1;
                    int index3 = i + 2;

                    if (indexBuffer32 != null)
                    {
                        index1 = indexBuffer32[index1];
                        index2 = indexBuffer32[index2];
                        index3 = indexBuffer32[index3];
                    }
                    else if (indexBuffer16 != null)
                    {
                        index1 = indexBuffer16[index1];
                        index2 = indexBuffer16[index2];
                        index3 = indexBuffer16[index3];
                    }

                    int vertexOffset1 = index1 * oldVertexStride;
                    int vertexOffset2 = index2 * oldVertexStride;
                    int vertexOffset3 = index3 * oldVertexStride;

                    // Get positions
                    var position1 = (Vector3*)&oldBuffer[vertexOffset1 + positionOffset];
                    var position2 = (Vector3*)&oldBuffer[vertexOffset2 + positionOffset];
                    var position3 = (Vector3*)&oldBuffer[vertexOffset3 + positionOffset];

                    // Get texture coordinates
                    var uv1 = (Vector3*)&oldBuffer[vertexOffset1 + uvOffset];
                    var uv2 = (Vector3*)&oldBuffer[vertexOffset2 + uvOffset];
                    var uv3 = (Vector3*)&oldBuffer[vertexOffset3 + uvOffset];

                    // Calculate position and UV vectors from vertex 1 to vertex 2 and 3
                    var edge1 = *position2 - *position1;
                    var edge2 = *position3 - *position1;
                    var uvEdge1 = *uv2 - *uv1;
                    var uvEdge2 = *uv3 - *uv1;

                    var t = Vector3.Normalize(uvEdge2.Y * edge1 - uvEdge1.Y * edge2);
                    var b = Vector3.Normalize(uvEdge1.X * edge2 - uvEdge2.X * edge1);

                    // Contribute to every vertex
                    tangents[index1] += t;
                    tangents[index2] += t;
                    tangents[index3] += t;

                    bitangents[index1] += b;
                    bitangents[index2] += b;
                    bitangents[index3] += b;
                }

                var oldVertexOffset = 0;
                var newVertexOffset = 0;
                for (int i = 0; i < vertexCount; ++i)
                {
                    Utilities.CopyMemory(new IntPtr(&newBuffer[newVertexOffset]), new IntPtr(&oldBuffer[oldVertexOffset]), oldVertexStride);

                    var normal = *(Vector3*)&oldBuffer[oldVertexOffset + normalOffset];
                    var target = ((float*)(&newBuffer[newVertexOffset + tangentOffset]));

                    var tangent = -tangents[i];
                    var bitangent = bitangents[i];

                    // Gram-Schmidt orthogonalize
                    *((Vector3*)target) = Vector3.Normalize(tangent - normal * Vector3.Dot(normal, tangent));

                    // Calculate handedness
                    target[3] = Vector3.Dot(Vector3.Cross(normal, tangent), bitangent) < 0.0f ? -1.0f : 1.0f;

                    oldVertexOffset += oldVertexStride;
                    newVertexOffset += newVertexStride;
                }

                return new KeyValuePair<VertexDeclaration, byte[]>(vertexDeclaration, newBufferData);
            }
        }
    }
}