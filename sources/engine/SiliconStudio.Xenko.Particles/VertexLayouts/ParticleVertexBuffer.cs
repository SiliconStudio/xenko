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

        private readonly int vertexStructSize;
        private readonly int indexStructSize;

        private DeviceResourceContext ResourceContext;

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
        }

        private unsafe void InitBuffer(GraphicsDevice device, Effect effect, int vertexCount, int indexCount)
        {
            //    ResourceContext = device.GetOrCreateSharedData(GraphicsDeviceSharedDataType.PerContext, "ResourceKey",
            //        d => new DeviceResourceContext(device, effect, VertexDeclaration, vertexCount, indexStructSize, indexCount));

            indexCount = (indexCount / 6) * 6;

            ResourceContext = new DeviceResourceContext(device, effect, VertexDeclaration, vertexCount, indexStructSize, indexCount);

            var mappedIndices = device.MapSubresource(ResourceContext.IndexBuffer, 0, MapMode.WriteDiscard, false, 0, indexCount * indexStructSize);
            var indexPointer = mappedIndices.DataBox.DataPointer;

            var k = 0;
            for (var i = 0; i < indexCount; k += 4)
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
        private bool hasStarted = false;

        public IntPtr StartBuffer(GraphicsDevice device, Effect effect)
        {
            if (!hasStarted)
            {
                InitBuffer(device, effect, 1024 * 4, 1024 * 6);
                hasStarted = true;
            }

            mappedVertices = device.MapSubresource(ResourceContext.VertexBuffer, 0, MapMode.WriteDiscard, false, 0, ResourceContext.VertexCount * vertexStructSize);

            return mappedVertices.DataBox.DataPointer;
        }

        public void FlushBuffer(GraphicsDevice device, int indexCount)
        {
            device.UnmapSubresource(mappedVertices);

            device.SetVertexArrayObject(ResourceContext.VertexArrayObject);

            device.DrawIndexed(PrimitiveType.TriangleList, indexCount, ResourceContext.IndexBufferPosition);
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

}
