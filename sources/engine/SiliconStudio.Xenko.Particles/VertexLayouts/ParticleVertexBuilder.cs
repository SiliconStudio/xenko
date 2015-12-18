// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    public class ParticleVertexBuilder
    {
        public VertexDeclaration VertexDeclaration { get; private set; }

        private int verticesPerParticle = 4;
        private int verticesPerQuad = 4;
        private int indicesPerQuad = 6;

        public delegate void TransformAttributeDelegate<T>(ref T value);

        private readonly int vertexStructSize;
        private readonly int indexStructSize;

        private DeviceResourceContext ResourceContext;

        private readonly Dictionary<AttributeDescription, AttributeAccessor> availableAttributes;

        private readonly List<VertexElement> vertexElementList;

        public ParticleVertexBuilder()
        {
            vertexElementList = new List<VertexElement>();

            ResetVertexElementList();

            indexStructSize = sizeof(short);

            availableAttributes = new Dictionary<AttributeDescription, AttributeAccessor>();

            UpdateVertexLayout();
        }

        internal void ResetVertexElementList()
        {
            vertexElementList.Clear();

            // Mandatory
            AddVertexElement(ParticleVertexElements.Position);
            AddVertexElement(ParticleVertexElements.TexCoord);
        }

        internal void AddVertexElement(VertexElement element)
        {
            if (vertexElementList.Contains(element))
                return;

            vertexElementList.Add(element);
        }

        internal void UpdateVertexLayout()
        {
            VertexDeclaration = new VertexDeclaration(vertexElementList.ToArray());

            availableAttributes.Clear();

            var totalOffset = 0;
            foreach (var vertexElement in VertexDeclaration.VertexElements)
            {
                var attrDesc = new AttributeDescription(vertexElement.SemanticName);
                var stride = vertexElement.Format.SizeInBytes();
                var attrAccs = new AttributeAccessor { Offset = totalOffset, Size = stride };
                totalOffset += stride;

                availableAttributes.Add(attrDesc, attrAccs);
            }

            bufferIsDirty = true;
        }

        private bool bufferIsDirty = true;
        private int requiredQuads = 0;
        private int livingQuads = 0;

        public void SetRequiredQuads(int quadsPerParticle, int livingParticles, int totalParticles)
        {
            verticesPerParticle = quadsPerParticle * verticesPerQuad;
            var minQuads = quadsPerParticle * livingParticles;
            var maxQuads = quadsPerParticle * totalParticles;

            livingQuads = minQuads;

            if (minQuads > requiredQuads || maxQuads <= requiredQuads / 2)
            {
                requiredQuads = maxQuads;
                bufferIsDirty = true;
            }
        }

        private unsafe void InitBuffer(GraphicsDevice device, Effect effect, int vertexCount, int indexCount)
        {
            //    ResourceContext = device.GetOrCreateSharedData(GraphicsDeviceSharedDataType.PerContext, "ResourceKey",
            //        d => new DeviceResourceContext(device, effect, VertexDeclaration, vertexCount, indexStructSize, indexCount));

            ResourceContext = new DeviceResourceContext(device, effect, VertexDeclaration, vertexCount, indexStructSize, indexCount);

            var mappedIndices = device.MapSubresource(ResourceContext.IndexBuffer, 0, MapMode.WriteDiscard, false, 0, indexCount * indexStructSize);
            var indexPointer = mappedIndices.DataBox.DataPointer;

            var k = 0;
            for (var i = 0; i < indexCount; k += verticesPerQuad)
            {
                *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 0);
                *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 1);
                *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 2);
                *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 0);
                *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 2);
                *(short*)(indexPointer + indexStructSize * i++) = (short)(k + 3);
            }

            device.UnmapSubresource(mappedIndices);
        }

        private MappedResource mappedVertices;
        private IntPtr vertexBuffer = IntPtr.Zero;
        private IntPtr vertexBufferOrigin = IntPtr.Zero;

        internal IntPtr StartBuffer(GraphicsDevice device, Effect effect)
        {
            if (bufferIsDirty && requiredQuads > 0)
            {
                InitBuffer(device, effect, requiredQuads * verticesPerQuad, requiredQuads * indicesPerQuad);
                bufferIsDirty = false;
            }

            vertexBuffer = IntPtr.Zero;
            vertexBufferOrigin = IntPtr.Zero;

            if (bufferIsDirty)
                return IntPtr.Zero;

            mappedVertices = device.MapSubresource(ResourceContext.VertexBuffer, 0, MapMode.WriteDiscard, false, 0, ResourceContext.VertexCount * vertexStructSize);

            vertexBuffer        = mappedVertices.DataBox.DataPointer;
            vertexBufferOrigin  = mappedVertices.DataBox.DataPointer;

            return mappedVertices.DataBox.DataPointer;
        }

        internal void RestartBuffer()
        {
            vertexBuffer = vertexBufferOrigin;
        }

        internal void FlushBuffer(GraphicsDevice device)
        {
            if (bufferIsDirty)
                return;

            vertexBuffer = IntPtr.Zero;
            vertexBufferOrigin = IntPtr.Zero;

            device.UnmapSubresource(mappedVertices);

            device.SetVertexArrayObject(ResourceContext.VertexArrayObject);

            device.DrawIndexed(PrimitiveType.TriangleList, livingQuads * indicesPerQuad, ResourceContext.IndexBufferPosition);
        }

        internal void NextVertex()
        {
            vertexBuffer += VertexDeclaration.VertexStride;
        }

        internal void NextParticle()
        {
            vertexBuffer += VertexDeclaration.VertexStride * verticesPerParticle;
        }

        internal AttributeAccessor GetAccessor(AttributeDescription desc) 
        {            
            AttributeAccessor accessor;
            if (!availableAttributes.TryGetValue(desc, out accessor))
            {
                return new AttributeAccessor { Offset = 0, Size = 0 };
            }
            
            return accessor;
        }

        internal void SetAttribute(AttributeAccessor accessor, IntPtr ptrRef) 
        {
            Utilities.CopyMemory(vertexBuffer + accessor.Offset, ptrRef, accessor.Size);
        }

        internal void SetAttributePerParticle(AttributeAccessor accessor, IntPtr ptrRef)
        {
            for (var i = 0; i < verticesPerParticle; i++)
            {
                Utilities.CopyMemory(vertexBuffer + accessor.Offset + i * VertexDeclaration.VertexStride, ptrRef, accessor.Size);
            }
        }

        internal void TransformAttributePerParticle<T>(AttributeAccessor accessor, TransformAttributeDelegate<T> transformMethod) where T : struct
        {
            for (var i = 0; i < verticesPerParticle; i++)
            {
                var temp = Utilities.Read<T>(vertexBuffer + accessor.Offset + i * VertexDeclaration.VertexStride);

                transformMethod(ref temp);

                Utilities.Write(vertexBuffer + accessor.Offset + i * VertexDeclaration.VertexStride, ref temp);
            }
        }

        // TransformAttributeDelegate

        /// <summary>
        /// Use a ResourceContext per GraphicsDevice (DeviceContext)
        /// </summary>
        protected class DeviceResourceContext : ComponentBase
        {
            /// <summary>
            /// Gets the number of vertices.
            /// </summary>
            public readonly int VertexCount;

            /// <summary>
            /// The vertex buffer of the batch.
            /// </summary>
            public readonly Buffer VertexBuffer;

            /// <summary>
            /// Gets the number of indices.
            /// </summary>
            public readonly int IndexCount;

            /// <summary>
            /// The index buffer of the batch.
            /// </summary>
            public readonly Buffer IndexBuffer;

            /// <summary>
            /// The VertexArrayObject of the batch.
            /// </summary>
            public readonly VertexArrayObject VertexArrayObject;

            /// <summary>
            /// The current position in vertex into the vertex array buffer.
            /// </summary>
            public int VertexBufferPosition;

            /// <summary>
            /// The current position in index into the index array buffer.
            /// </summary>
            public int IndexBufferPosition;

            public DeviceResourceContext(GraphicsDevice device, Effect effect, VertexDeclaration declaration, int vertexCount, int indexStructSize, int indexCount)
            {
                var vertexSize = declaration.CalculateSize();

                VertexCount = vertexCount;
                IndexCount  = indexCount;

                VertexBuffer = Buffer.Vertex.New(device, VertexCount * vertexSize, GraphicsResourceUsage.Dynamic).DisposeBy(this);

                IndexBuffer = Buffer.Index.New(device, IndexCount * indexStructSize, GraphicsResourceUsage.Dynamic).DisposeBy(this);

                var indexBufferBinding = new IndexBufferBinding(IndexBuffer, indexStructSize == sizeof(int), IndexBuffer.Description.SizeInBytes / indexStructSize);
                var vertexBufferBinding = new VertexBufferBinding(VertexBuffer, declaration, VertexCount, vertexSize);

                // Creates a VAO
                VertexArrayObject = VertexArrayObject.New(device, effect.InputSignature, indexBufferBinding, vertexBufferBinding).DisposeBy(this);
            }
        }

    }
}
