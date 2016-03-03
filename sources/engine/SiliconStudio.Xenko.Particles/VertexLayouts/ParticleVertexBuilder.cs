// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Graphics;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    /// <summary>
    /// Manager class for the vertex buffer stream which can dynamically change the required vertex layout and rebuild the buffer based on the particle fields
    /// </summary>
    public class ParticleVertexBuilder
    {
        private int verticesPerParticle = 4;
        private int verticesPerQuad = 4;
        private int indicesPerQuad = 6;

        public delegate void TransformAttributeDelegate<T>(ref T value);

        private int vertexStructSize;
        private readonly int indexStructSize;

        private DeviceResourceContext resourceContext;

        private readonly Dictionary<AttributeDescription, AttributeAccessor> availableAttributes;

        private readonly List<VertexElement> vertexElementList;

        private bool bufferIsDirty = true;
        private int requiredQuads;
        private int livingQuads;

        private MappedResource mappedVertices;
        private IntPtr vertexBuffer = IntPtr.Zero;
        private IntPtr vertexBufferOrigin = IntPtr.Zero;

        private int currentVertex;
        private int maxVertices;

        private int currentParticleIndex;
        private int maxParticles;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ParticleVertexBuilder()
        {
            vertexElementList = new List<VertexElement>();

            ResetVertexElementList();

            indexStructSize = sizeof(short);

            availableAttributes = new Dictionary<AttributeDescription, AttributeAccessor>();

            UpdateVertexLayout();
        }

        protected int VerticesPerSegFirst { get; private set; }
        protected int VerticesPerSegMiddle { get; private set; }
        protected int VerticesPerSegLast { get; private set; }

        protected int VerticesPerSegCurrent { get; private set; }



        /// <summary>
        /// The current <see cref="Graphics.VertexDeclaration"/> of the contained vertex buffer
        /// </summary>
        public VertexDeclaration VertexDeclaration { get; private set; }

        /// <summary>
        /// The default texture coordinates will default to the first texture coordinates element added to the list in case there are two or more sets
        /// </summary>
        public AttributeDescription DefaultTexCoords { get; private set; } = new AttributeDescription(null);

        /// <summary>
        /// Resets the list of required vertex elements, setting it to the minimum mandatory length
        /// </summary>
        public void ResetVertexElementList()
        {
            vertexElementList.Clear();

            // Mandatory
            AddVertexElement(ParticleVertexElements.Position);
            //AddVertexElement(ParticleVertexElements.TexCoord0);
        }

        /// <summary>
        /// Adds a new required element to the list of vertex elements, if it's not in the list already
        /// </summary>
        /// <param name="element">New element to add</param>
        public void AddVertexElement(VertexElement element)
        {
            if (vertexElementList.Contains(element))
                return;

            vertexElementList.Add(element);
        }

        /// <summary>
        /// Updates the vertex layout with the new list. Should be called only when there have been changes to the list.
        /// </summary>
        public void UpdateVertexLayout()
        {
            VertexDeclaration = new VertexDeclaration(vertexElementList.ToArray());

            availableAttributes.Clear();
            DefaultTexCoords = new AttributeDescription(null);

            var totalOffset = 0;
            foreach (var vertexElement in VertexDeclaration.VertexElements)
            {
                var attrDesc = new AttributeDescription(vertexElement.SemanticAsText);
                if (DefaultTexCoords.GetHashCode() == 0 && vertexElement.SemanticAsText.Contains("TEXCOORD"))
                {
                    DefaultTexCoords = attrDesc;
                }

                var stride = vertexElement.Format.SizeInBytes();
                var attrAccs = new AttributeAccessor { Offset = totalOffset, Size = stride };
                totalOffset += stride;

                availableAttributes.Add(attrDesc, attrAccs);
            }

            bufferIsDirty = true;
        }

        /// <summary>
        /// Sets the required quads per particle and number of particles so that a sufficiently big buffer can be allocated
        /// </summary>
        /// <param name="quadsPerParticle">Required quads per particle, assuming 1 quad = 4 vertices = 6 indices</param>
        /// <param name="livingParticles">Number of living particles this frame</param>
        /// <param name="totalParticles">Number of total number of possible particles for the parent emitter</param>
        public void SetRequiredQuads(int quadsPerParticle, int livingParticles, int totalParticles)
        {
            verticesPerParticle = quadsPerParticle * verticesPerQuad;
            var minQuads = quadsPerParticle * livingParticles;
            var maxQuads = quadsPerParticle * totalParticles;

            livingQuads = minQuads;
            maxParticles = livingParticles;

            currentVertex = 0;
            maxVertices = livingParticles * verticesPerParticle;

            if (requiredQuads == 0 || minQuads > requiredQuads || maxQuads <= requiredQuads / 2)
            {
                requiredQuads = maxQuads;
                bufferIsDirty = true;
            }

            // The default assumption is that every particle defines a separate segment and no segments are shared
            SetVerticesPerSegment(verticesPerParticle, verticesPerParticle, verticesPerParticle);
        }

        /// <summary>
        /// Sets how many vertices are associated with the first, middle and last quad segments of the buffer. In case of billboards 1 segment = 1 quad but other shapes might be laid out differently
        /// </summary>
        /// <param name="verticesForFirstSegment">Number of vertices for the first segment</param>
        /// <param name="verticesForMiddleSegment">Number of vertices for the middle segments</param>
        /// <param name="verticesForLastSegment">Number of vertices for the last segment</param>
        public void SetVerticesPerSegment(int verticesForFirstSegment, int verticesForMiddleSegment, int verticesForLastSegment)
        {
            VerticesPerSegFirst = verticesForFirstSegment;
            VerticesPerSegMiddle = verticesForMiddleSegment;
            VerticesPerSegLast = verticesForLastSegment;

            VerticesPerSegCurrent = VerticesPerSegFirst;
        }

        /// <summary>
        /// Initiates a new vertex and index buffers
        /// </summary>
        /// <param name="device"><see cref="GraphicsDevice"/> to use</param>
        /// <param name="vertexCount">Required vertices count. Stride is estimated from the vertex declaration</param>
        /// <param name="indexCount">Required indices count. Stride is automatically estimated</param>
        private unsafe void InitBuffer(GraphicsDevice device, int vertexCount, int indexCount)
        {
            resourceContext = new DeviceResourceContext(device, VertexDeclaration, vertexCount, indexStructSize, indexCount);

            vertexStructSize = VertexDeclaration.VertexStride;

            var mappedIndices = device.MapSubresource(resourceContext.IndexBuffer, 0, MapMode.WriteDiscard, false, 0, indexCount * indexStructSize);
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

        /// <summary>
        /// Maps a subresource so that particle data can be written to the vertex buffer
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public IntPtr MapBuffer(GraphicsDevice device)
        {
            if (bufferIsDirty && requiredQuads > 0)
            {
                InitBuffer(device, requiredQuads * verticesPerQuad, requiredQuads * indicesPerQuad);
                bufferIsDirty = false;
            }

            vertexBuffer = IntPtr.Zero;
            vertexBufferOrigin = IntPtr.Zero;

            if (bufferIsDirty)
                return IntPtr.Zero;

            mappedVertices = device.MapSubresource(resourceContext.VertexBuffer, 0, MapMode.WriteDiscard, false, 0, resourceContext.VertexCount * vertexStructSize);

            vertexBuffer        = mappedVertices.DataBox.DataPointer;
            vertexBufferOrigin  = mappedVertices.DataBox.DataPointer;

            return mappedVertices.DataBox.DataPointer;
        }

        /// <summary>
        /// Creates a <see cref="VertexArrayObject"/> for the current buffer and vertex layout
        /// </summary>
        /// <param name="device"><see cref="GraphicsDevice"/> to use</param>
        /// <param name="effect"><see cref="Effect"/> which will render the buffer</param>
        // ReSharper disable once InconsistentNaming
        public void CreateVAO(GraphicsDevice device, Effect effect)
        {
            resourceContext.CreateVAO(device, effect, VertexDeclaration, indexStructSize);
        }

        /// <summary>
        /// Moves the index to the beginning of the buffer so that the data can be filled from the first particle again
        /// </summary>
        public void RestartBuffer()
        {
            vertexBuffer = vertexBufferOrigin;
            currentParticleIndex = 0;
            currentVertex = 0;
            VerticesPerSegCurrent = VerticesPerSegFirst;
        }

        /// <summary>
        /// Unmaps the subresource after all the particle data has been updated
        /// </summary>
        /// <param name="device"></param>
        public void UnmapBuffer(GraphicsDevice device)
        {
            if (bufferIsDirty)
                return;

            vertexBuffer = IntPtr.Zero;
            vertexBufferOrigin = IntPtr.Zero;
            currentParticleIndex = 0;
            currentVertex = 0;

            device.UnmapSubresource(mappedVertices);
        }

        /// <summary>
        /// Draws the generated vertex buffer with the particle data from this frame
        /// </summary>
        /// <param name="device"></param>
        public void Draw(GraphicsDevice device)
        {
            if (bufferIsDirty)
                return;

            device.SetVertexArrayObject(resourceContext.VertexArrayObject);

            device.DrawIndexed(PrimitiveType.TriangleList, livingQuads * indicesPerQuad, resourceContext.IndexBufferPosition);
        }

        /// <summary>
        /// Advances the pointer to the next vertex in the buffer, so that it can be written
        /// </summary>
        public void NextVertex()
        {
            if (++currentVertex >= maxVertices)
                currentVertex = maxVertices - 1;

            vertexBuffer = vertexBufferOrigin + VertexDeclaration.VertexStride * currentVertex;
        }

        /// <summary>
        /// Advances the pointer to the next particle in the buffer, so that its first vertex can be written
        /// </summary>
        public void NextParticle()
        {
            if (++currentParticleIndex >= maxParticles)
                currentParticleIndex = maxParticles - 1;

            vertexBuffer = vertexBufferOrigin + (VertexDeclaration.VertexStride * currentParticleIndex * verticesPerParticle);
        }

        /// <summary>
        /// Advances the pointer to the next segment in the buffer, so that its first vertex can be written
        /// </summary>
        public void NextSegment()
        {
            // The number of segments is tied to the number of particles
            if (++currentParticleIndex >= maxParticles)
            {
                // Already at the last particle
                currentParticleIndex = maxParticles - 1;
                return;
            }

            vertexBuffer += VertexDeclaration.VertexStride * VerticesPerSegCurrent;
            VerticesPerSegCurrent = (currentParticleIndex < maxParticles - 1) ? VerticesPerSegMiddle : VerticesPerSegLast;
        }

        public AttributeAccessor GetAccessor(AttributeDescription desc) 
        {            
            AttributeAccessor accessor;
            if (!availableAttributes.TryGetValue(desc, out accessor))
            {
                return new AttributeAccessor { Offset = 0, Size = 0 };
            }
            
            return accessor;
        }

        /// <summary>
        /// Sets the data for the current vertex using the provided <see cref="AttributeAccessor"/>
        /// </summary>
        /// <param name="accessor">Accessor to the vertex data</param>
        /// <param name="ptrRef">Pointer to the source data</param>
        public void SetAttribute(AttributeAccessor accessor, IntPtr ptrRef) 
        {
            Utilities.CopyMemory(vertexBuffer + accessor.Offset, ptrRef, accessor.Size);
        }

        /// <summary>
        /// Sets the same data for the all vertices in the current particle using the provided <see cref="AttributeAccessor"/>
        /// </summary>
        /// <param name="accessor">Accessor to the vertex data</param>
        /// <param name="ptrRef">Pointer to the source data</param>
        public void SetAttributePerParticle(AttributeAccessor accessor, IntPtr ptrRef)
        {
            for (var i = 0; i < verticesPerParticle; i++)
            {
                Utilities.CopyMemory(vertexBuffer + accessor.Offset + i * VertexDeclaration.VertexStride, ptrRef, accessor.Size);
            }
        }

        /// <summary>
        /// Sets the same data for the all vertices in the current particle using the provided <see cref="AttributeAccessor"/>
        /// </summary>
        /// <param name="accessor">Accessor to the vertex data</param>
        /// <param name="ptrRef">Pointer to the source data</param>
        public void SetAttributePerSegment(AttributeAccessor accessor, IntPtr ptrRef)
        {
            for (var i = 0; i < VerticesPerSegCurrent; i++)
            {
                Utilities.CopyMemory(vertexBuffer + accessor.Offset + i * VertexDeclaration.VertexStride, ptrRef, accessor.Size);
            }
        }

        /// <summary>
        /// Transforms attribute data using already written data from another attribute
        /// </summary>
        /// <typeparam name="T">Type data</typeparam>
        /// <param name="accessorTo">Vertex attribute accessor to the destination attribute</param>
        /// <param name="accessorFrom">Vertex attribute accessor to the source attribute</param>
        /// <param name="transformMethod">Transform method for the type data</param>
        public void TransformAttributePerSegment<T>(AttributeAccessor accessorFrom, AttributeAccessor accessorTo, TransformAttributeDelegate<T> transformMethod) where T : struct
        {
            for (var i = 0; i < VerticesPerSegCurrent; i++)
            {
                var temp = Utilities.Read<T>(vertexBuffer + accessorFrom.Offset + i * VertexDeclaration.VertexStride);

                transformMethod(ref temp);

                Utilities.Write(vertexBuffer + accessorTo.Offset + i * VertexDeclaration.VertexStride, ref temp);
            }
        }

        /// <summary>
        /// Transforms already written attribute data using the provided transform method
        /// </summary>
        /// <typeparam name="T">Type data</typeparam>
        /// <param name="accessor">Vertex attribute accessor</param>
        /// <param name="transformMethod">Transform method for the type data</param>
        public void TransformAttributePerParticle<T>(AttributeAccessor accessor, TransformAttributeDelegate<T> transformMethod) where T : struct
        {
            for (var i = 0; i < verticesPerParticle; i++)
            {
                var temp = Utilities.Read<T>(vertexBuffer + accessor.Offset + i * VertexDeclaration.VertexStride);

                transformMethod(ref temp);

                Utilities.Write(vertexBuffer + accessor.Offset + i * VertexDeclaration.VertexStride, ref temp);
            }
        }

        /// <summary>
        /// Transforms attribute data using already written data from another attribute
        /// </summary>
        /// <typeparam name="T">Type data</typeparam>
        /// <param name="accessorTo">Vertex attribute accessor to the destination attribute</param>
        /// <param name="accessorFrom">Vertex attribute accessor to the source attribute</param>
        /// <param name="transformMethod">Transform method for the type data</param>
        public void TransformAttributePerParticle<T>(AttributeAccessor accessorFrom, AttributeAccessor accessorTo, TransformAttributeDelegate<T> transformMethod) where T : struct
        {
            for (var i = 0; i < verticesPerParticle; i++)
            {
                var temp = Utilities.Read<T>(vertexBuffer + accessorFrom.Offset + i * VertexDeclaration.VertexStride);

                transformMethod(ref temp);

                Utilities.Write(vertexBuffer + accessorTo.Offset + i * VertexDeclaration.VertexStride, ref temp);
            }
        }

        /// <summary>
        /// Gets the current <see cref="EffectInputSignature"/> for the vertex buffer
        /// </summary>
        /// <returns></returns>
        public EffectInputSignature GetInputSignature()
        {
            return resourceContext.EffectInputSignature;
        }


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
            public VertexArrayObject VertexArrayObject;

            /// <summary>
            /// The current position in vertex into the vertex array buffer.
            /// </summary>
            public int VertexBufferPosition;

            /// <summary>
            /// The current position in index into the index array buffer.
            /// </summary>
            public int IndexBufferPosition;

            public EffectInputSignature EffectInputSignature;

            private bool dirty;

            public DeviceResourceContext(GraphicsDevice device, VertexDeclaration declaration, int vertexCount, int indexStructSize, int indexCount)
            {
                var vertexSize = declaration.CalculateSize();

                VertexCount = vertexCount;
                IndexCount  = indexCount;

                VertexBuffer = Buffer.Vertex.New(device, VertexCount * vertexSize, GraphicsResourceUsage.Dynamic).DisposeBy(this);

                IndexBuffer = Buffer.Index.New(device, IndexCount * indexStructSize, GraphicsResourceUsage.Dynamic).DisposeBy(this);

                dirty = true;
            }

            public void CreateVAO(GraphicsDevice device, Effect effect, VertexDeclaration declaration, int indexStructSize)
            {
                if (!dirty)
                    return;
                dirty = false;

                var vertexSize = declaration.CalculateSize();
                EffectInputSignature = effect.InputSignature;

                var indexBufferBinding = new IndexBufferBinding(IndexBuffer, indexStructSize == sizeof(int), IndexBuffer.Description.SizeInBytes / indexStructSize);
                var vertexBufferBinding = new VertexBufferBinding(VertexBuffer, declaration, VertexCount, vertexSize);

                // Creates a VAO
                VertexArrayObject = VertexArrayObject.New(device, effect.InputSignature, indexBufferBinding, vertexBufferBinding).DisposeBy(this);
            }
        }

    }
}
