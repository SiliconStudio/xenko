using System;
using System.Runtime.InteropServices;

namespace SiliconStudio.Xenko.Graphics
{
    public class BufferPool : IDisposable
    {
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D12 || SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
        private const bool useBufferOffsets = true;
        private const int alignment = 256;
#else
        private const bool useBufferOffsets = false;
        private const int alignment = 16;
#endif

        public int Size;
        public IntPtr Data;

        private readonly GraphicsResourceAllocator allocator;
        private Buffer constantBuffer;
        private MappedResource mappedConstantBuffer;
        private CommandList commandList;

        private int bufferAllocationOffset;

        internal BufferPool(GraphicsResourceAllocator allocator, GraphicsDevice graphicsDevice, int size)
        {
            if (size % alignment != 0)
                throw new ArgumentException($"size is not a multiple of alignment ({alignment})", nameof(size));

            this.allocator = allocator;

            Size = size;
            if (!useBufferOffsets)
                Data = Marshal.AllocHGlobal(size);

            Reset();
        }

        public static BufferPool New(GraphicsResourceAllocator allocator, GraphicsDevice graphicsDevice, int size)
        {
            return new BufferPool(allocator, graphicsDevice, size);
        }

        public void Dispose()
        {
            if (useBufferOffsets)
                allocator.ReleaseReference(constantBuffer);
            else
                Marshal.FreeHGlobal(Data);
            Data = IntPtr.Zero;
        }

        public void Map(CommandList commandList)
        {
            if (useBufferOffsets)
            {
                this.commandList = commandList;
                mappedConstantBuffer = commandList.MapSubresource(constantBuffer, 0, MapMode.WriteNoOverwrite);
                Data = mappedConstantBuffer.DataBox.DataPointer;
            }
        }

        public void Unmap()
        {
            if (useBufferOffsets && mappedConstantBuffer.Resource != null)
            {
                commandList.UnmapSubresource(mappedConstantBuffer);
                mappedConstantBuffer = new MappedResource();
            }
        }

        public void Reset()
        {
            if (useBufferOffsets)
            {
                // Release previous buffer
                if (constantBuffer != null)
                    allocator.ReleaseReference(constantBuffer);

                constantBuffer = allocator.GetTemporaryBuffer(new BufferDescription(Size, BufferFlags.ConstantBuffer, GraphicsResourceUsage.Dynamic));
            }

            bufferAllocationOffset = 0;
        }

        public bool CanAllocate(int size)
        {
            return bufferAllocationOffset + size <= Size;
        }

        public void Allocate(GraphicsDevice graphicsDevice, int size, BufferPoolAllocationType type, ref BufferPoolAllocationResult bufferPoolAllocationResult)
        {
            var result = bufferAllocationOffset;
            bufferAllocationOffset += size;

            // Align next allocation
            // Note: total Size should be a multiple of alignment, so that CanAllocate() and Allocate() Size check matches
            bufferAllocationOffset = (bufferAllocationOffset + alignment - 1) / alignment * alignment;

            if (bufferAllocationOffset > Size)
                throw new InvalidOperationException();

            // Map (if needed)
            if (useBufferOffsets && mappedConstantBuffer.Resource == null)
                Map(commandList);

            bufferPoolAllocationResult.Data = Data + result;
            bufferPoolAllocationResult.Size = size;

            if (useBufferOffsets)
            {
                bufferPoolAllocationResult.Uploaded = true;
                bufferPoolAllocationResult.Offset = result;
                bufferPoolAllocationResult.Buffer = constantBuffer;
            }
            else
            {
                bufferPoolAllocationResult.Uploaded = false;

                if (type == BufferPoolAllocationType.UsedMultipleTime)
                {
                    if (bufferPoolAllocationResult.Buffer == null || bufferPoolAllocationResult.Buffer.SizeInBytes != size)
                    {
                        // Release old buffer in case size changed
                        if (bufferPoolAllocationResult.Buffer != null)
                            bufferPoolAllocationResult.Buffer.Dispose();

                        bufferPoolAllocationResult.Buffer = Buffer.Constant.New(graphicsDevice, size, graphicsDevice.Features.HasResourceRenaming ? GraphicsResourceUsage.Dynamic : GraphicsResourceUsage.Default);
                        //bufferPoolAllocationResult.Buffer = Buffer.New(graphicsDevice, size, BufferFlags.ConstantBuffer);
                    }
                }
            }
        }
    }

    public enum BufferPoolAllocationType
    {
        /// <summary>
        /// Notify the allocator that this buffer won't be reused for much more than 1 (or few) draw calls.
        /// In practice, on older D3D11 (not 11.1) and OpenGL ES 2.0 hardware, we won't use a dedicated cbuffer.
        /// This has no effect on new API where we can bind cbuffer offsets.
        /// </summary>
        UsedOnce,

        /// <summary>
        /// Notify the allocator that this buffer will be reused for many draw calls.
        /// In practice, on older D3D11 (not 11.1) and OpenGL ES 2.0 hardware, we will use a dedicated cbuffer.
        /// This has no effect on new API where we can bind cbuffer offsets.
        /// </summary>
        UsedMultipleTime,
    }
}