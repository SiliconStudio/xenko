// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Paradox.Rendering.Data;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics.Data;

namespace SiliconStudio.Paradox.Extensions
{
    public static class TransformExtensions
    {
        /// <summary>
        /// Transform a vertex buffer positions, normals, tangents and bitangents using the given matrix.
        /// </summary>
        /// <param name="meshData">The mesh data.</param>
        public unsafe static void TransformBuffer(this VertexBufferBinding vertexBufferBinding, ref Matrix matrix)
        {
            // List of items that need to be transformed by the matrix
            var vertexElementsToTransform1 = vertexBufferBinding.Declaration
                .EnumerateWithOffsets()
                .Where(x => x.VertexElement.SemanticName == VertexElementUsage.Position
                         && (x.VertexElement.Format == PixelFormat.R32G32B32A32_Float
                          || x.VertexElement.Format == PixelFormat.R32G32B32_Float))
                .ToArray();

            // List of items that need to be transformed by the inverse transpose matrix
            var vertexElementsToTransform2 = vertexBufferBinding.Declaration
                .EnumerateWithOffsets()
                .Where(x => (x.VertexElement.SemanticName == VertexElementUsage.Normal
                          || x.VertexElement.SemanticName == VertexElementUsage.Tangent
                          || x.VertexElement.SemanticName == VertexElementUsage.BiTangent)
                         && x.VertexElement.Format == PixelFormat.R32G32B32_Float)
                .ToArray();

            // If needed, compute matrix inverse transpose
            Matrix inverseTransposeMatrix;
            if (vertexElementsToTransform2.Length > 0)
            {
                Matrix.Invert(ref matrix, out inverseTransposeMatrix);
                Matrix.Transpose(ref inverseTransposeMatrix, out inverseTransposeMatrix);
            }
            else
            {
                inverseTransposeMatrix = Matrix.Identity;
            }

            // Transform buffer data
            var bufferData = vertexBufferBinding.Buffer.GetSerializationData().Content;
            var vertexStride = vertexBufferBinding.Declaration.VertexStride;
            var vertexCount = vertexBufferBinding.Count;
            fixed (byte* bufferPointerStart = &bufferData[vertexBufferBinding.Offset])
            {
                var bufferPointer = bufferPointerStart;

                for (int i = 0; i < vertexCount; ++i)
                {
                    // Transform positions
                    foreach (var vertexElement in vertexElementsToTransform1)
                    {
                        var elementPointer = bufferPointer + vertexElement.Offset;
                        if (vertexElement.VertexElement.Format == PixelFormat.R32G32B32A32_Float)
                        {
                            Vector4.Transform(ref *(Vector4*)elementPointer, ref matrix, out *(Vector4*)elementPointer);
                        }
                        else
                        {
                            Vector3.TransformCoordinate(ref *(Vector3*)elementPointer, ref matrix, out *(Vector3*)elementPointer);
                        }
                    }

                    // Transform normals
                    foreach (var vertexElement in vertexElementsToTransform2)
                    {
                        var elementPointer = bufferPointer + vertexElement.Offset;
                        Vector3.TransformNormal(ref *(Vector3*)elementPointer, ref inverseTransposeMatrix, out *(Vector3*)elementPointer);
                    }

                    bufferPointer += vertexStride;
                }
            }
        }
    }
}