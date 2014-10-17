// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Data;

namespace SiliconStudio.Paradox.Extensions
{
    public static class PolySortExtensions
    {
        public unsafe static void SortMeshPolygons(this MeshDrawData meshData, Vector3 viewDirectionForSorting)
        {
            // need to have alreade an vertex buffer
            if (meshData.VertexBuffers == null)
                throw new ArgumentException();
            // For now, require a MeshData with an index buffer
            if (meshData.IndexBuffer == null)
                throw new NotImplementedException("The mesh Data needs to have index buffer");
            if(meshData.VertexBuffers.Length != 1)
                throw new NotImplementedException("Sorting not implemented for multiple vertex buffers by submeshdata");

            if (viewDirectionForSorting == Vector3.Zero)
            {
                // By default to -Z if sorting is set to null
                viewDirectionForSorting = -Vector3.UnitZ;
            }

            const int PolySize = 3; // currently only triangle list are supported
            var polyIndicesSize = PolySize * Utilities.SizeOf<int>();
            var vertexBuffer = meshData.VertexBuffers[0];
            var oldIndexBuffer = meshData.IndexBuffer;
            var vertexStride = vertexBuffer.Declaration.VertexStride;

            // Generate the sort list
            var sortList = new List<KeyValuePair<int, Vector3>>();
            var pointList = new List<Vector3>();

            fixed (byte* vertexBufferPointerStart = &vertexBuffer.Buffer.Value.Content[vertexBuffer.Offset])
            fixed (byte* indexBufferPointerStart = &oldIndexBuffer.Buffer.Value.Content[oldIndexBuffer.Offset])
            {
                for (var i = 0; i < oldIndexBuffer.Count / PolySize; ++i) 
                {
                    // fill the point list of the polygon vertices
                    pointList.Clear();
                    for (var u = 0; u < PolySize; ++u)
                    {
                        var curIndex = *(int*)(indexBufferPointerStart + Utilities.SizeOf<int>() * (i * PolySize + u));
                        var pVertexPos = (Vector3*)(vertexBufferPointerStart + vertexStride * curIndex);
                        pointList.Add(*pVertexPos);
                    }

                    // compute the bary-center
                    var accu = Vector3.Zero;
                    foreach (var pt in pointList) //linq do not seems to work on Vector3 type, so compute the mean by hand ...
                        accu += pt;
                    var center = accu / pointList.Count;

                    // add to the list to sort
                    sortList.Add(new KeyValuePair<int, Vector3>(i,center));
                }
            }

            // sort the list
            var sortedIndices = sortList.OrderBy(x => Vector3.Dot(x.Value, viewDirectionForSorting)).Select(x=>x.Key).ToList();   // TODO have a generic delegate for sorting
            
            // re-write the index buffer
            var newIndexBufferData = new byte[oldIndexBuffer.Count * Utilities.SizeOf<int>()];
            fixed (byte* newIndexDataStart = &newIndexBufferData[0])
            fixed (byte* oldIndexDataStart = &oldIndexBuffer.Buffer.Value.Content[0])
            {
                var newIndexBufferPointer = newIndexDataStart;

                foreach (var index in sortedIndices)
                {
                    Utilities.CopyMemory((IntPtr)(newIndexBufferPointer), (IntPtr)(oldIndexDataStart + index * polyIndicesSize), polyIndicesSize);

                    newIndexBufferPointer += polyIndicesSize;
                }
            }
            meshData.IndexBuffer = new IndexBufferBindingData(new BufferData(BufferFlags.IndexBuffer, newIndexBufferData), oldIndexBuffer.Is32Bit, oldIndexBuffer.Count);
        }
    }
}
