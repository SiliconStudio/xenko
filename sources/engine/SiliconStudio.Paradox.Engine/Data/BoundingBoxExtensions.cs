// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Data;

namespace SiliconStudio.Paradox.Extensions
{
    public static class BoundingBoxExtensions
    {
        public unsafe static BoundingBox ComputeBoundingBox(this VertexBufferBinding vertexBufferBinding, ref Matrix matrix)
        {
            var positionOffset = vertexBufferBinding.Declaration
                .EnumerateWithOffsets()
                .First(x => x.VertexElement.SemanticAsText == "POSITION")
                .Offset;

            var boundingBox = BoundingBox.Empty;

            var vertexStride = vertexBufferBinding.Declaration.VertexStride;
            fixed (byte* bufferStart = &vertexBufferBinding.Buffer.GetSerializationData().Content[vertexBufferBinding.Offset])
            {
                byte* buffer = bufferStart;
                for (int i = 0; i < vertexBufferBinding.Count; ++i)
                {
                    var position = (Vector3*)(buffer + positionOffset);
                    Vector3 transformedPosition;

                    Vector3.TransformCoordinate(ref *position, ref matrix, out transformedPosition);
                    BoundingBox.Merge(ref boundingBox, ref transformedPosition, out boundingBox);
                    
                    buffer += vertexStride;
                }
            }

            return boundingBox;
        }
    }
}