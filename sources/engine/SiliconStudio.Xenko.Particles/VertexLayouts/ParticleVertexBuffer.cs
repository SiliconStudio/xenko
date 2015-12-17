using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    public class ParticleVertexBuffer
    {
        public VertexDeclaration VertexDeclaration { get; private set; }

        protected const int OffsetPosition  = 0;
        protected const int OffsetUv        = 12 + OffsetPosition;
        protected const int OffsetColor     = 8  + OffsetUv;
        protected const int OffsetLifetime  = 4  + OffsetColor;
        protected const int OffsetRandom    = 4  + OffsetLifetime;
        protected const int OffsetMax       = 4  + OffsetRandom;

        private int verticesPerParticle = 4;
        private int verticesPerQuad = 4;
        private int indicesPerQuad = 6;

        private readonly int vertexStructSize;
        private readonly int indexStructSize;

        private DeviceResourceContext ResourceContext;

        private readonly Dictionary<AttributeDescription, AttributeAccessor> availableAttributes;

        public ParticleVertexBuffer()
        {
            // TODO This should be dynamic
            VertexDeclaration
                = new VertexDeclaration(
                    VertexElement.Position<Vector3>(),
                    VertexElement.TextureCoordinate<Vector2>(),
                    VertexElement.Color<Color>(),
                    new VertexElement("BATCH_LIFETIME", PixelFormat.R32_Float),
                    new VertexElement("BATCH_RANDOMSEED", PixelFormat.R32_Float)
                    );

            indexStructSize = sizeof(short);

            availableAttributes = new Dictionary<AttributeDescription, AttributeAccessor>();
            var totalOffset = 0;
            foreach (var vertexElement in VertexDeclaration.VertexElements)
            {
                var attrDesc = new AttributeDescription(vertexElement.SemanticName);
                var stride = vertexElement.Format.SizeInBytes();
                var attrAccs = new AttributeAccessor { Offset = totalOffset, Stride = stride };
                totalOffset += stride;

                availableAttributes.Add(attrDesc, attrAccs);
            }
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
                return new AttributeAccessor { Offset = 0, Stride = 0 };
            }
            
            return accessor;
        }

        internal void SetAttribute(AttributeAccessor accessor, IntPtr ptrRef) 
        {
            Utilities.CopyMemory(vertexBuffer + accessor.Offset, ptrRef, accessor.Stride);
        }

        internal void SetAttributePerParticle(AttributeAccessor accessor, IntPtr ptrRef)
        {
            for (var i = 0; i < verticesPerParticle; i++)
            {
                Utilities.CopyMemory(vertexBuffer + accessor.Offset + i * VertexDeclaration.VertexStride, ptrRef, accessor.Stride);
            }
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

    public struct AttributeAccessor
    {
        public int Offset;
        public int Stride;
    }

    public struct AttributeDescription
    {
        private readonly int hashCode;
        public override int GetHashCode() => hashCode;

        public AttributeDescription(string name)
        {
            hashCode = name.GetHashCode();
        }

    }
}
