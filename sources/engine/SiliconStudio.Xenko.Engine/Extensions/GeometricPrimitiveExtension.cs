// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.GeometricPrimitives;

namespace SiliconStudio.Xenko.Extensions
{
    /// <summary>
    /// An extension class for the <see cref="GeometricPrimitive"/>
    /// </summary>
    public static class GeometricPrimitiveExtensions
    {
        public static MeshDraw ToMeshDraw<T>(this GeometricPrimitive<T> primitive) where T : struct, IVertex
        {
            var vertexBufferBinding = new VertexBufferBinding(primitive.VertexBuffer, new T().GetLayout(), primitive.VertexBuffer.ElementCount);
            var indexBufferBinding = new IndexBufferBinding(primitive.IndexBuffer, primitive.IsIndex32Bits, primitive.IndexBuffer.ElementCount);
            var data = new MeshDraw
            {
                StartLocation = 0, 
                PrimitiveType = PrimitiveType.TriangleList, 
                VertexBuffers = new[] { vertexBufferBinding }, 
                IndexBuffer = indexBufferBinding, 
                DrawCount = primitive.IndexBuffer.ElementCount
            };

            return data;
        }
    }
}
