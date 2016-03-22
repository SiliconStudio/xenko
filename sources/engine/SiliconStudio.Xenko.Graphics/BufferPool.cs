using System;
using System.Runtime.InteropServices;

namespace SiliconStudio.Xenko.Graphics
{
    public class BufferPool : IDisposable
    {
        public int Size;
        public IntPtr Data;

        private int bufferAllocationOffset;

        internal BufferPool(int size)
        {
            Size = size;
            Data = Marshal.AllocHGlobal(size);
        }

        public static BufferPool New(GraphicsDevice graphicsDevice, int size)
        {
            return new BufferPool(size);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(Data);
            Data = IntPtr.Zero;
        }

        public void Reset()
        {
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

            if (bufferAllocationOffset > Size)
                throw new InvalidOperationException();

            // TODO: We only implemented the D3D11/ES 2.0 compatibility mode
            // Need to write code to take advantage of cbuffer offsets later
            bufferPoolAllocationResult.Data = Data + result;
            bufferPoolAllocationResult.Size = size;
            bufferPoolAllocationResult.Uploaded = false;
            if (type == BufferPoolAllocationType.UsedMultipleTime)
            {
                if (bufferPoolAllocationResult.Buffer == null || bufferPoolAllocationResult.Buffer.SizeInBytes != size)
                {
                    // Release old buffer in case size changed
                    if (bufferPoolAllocationResult.Buffer != null)
                        bufferPoolAllocationResult.Buffer.Dispose();

                    bufferPoolAllocationResult.Buffer = Buffer.Constant.New(graphicsDevice, size);
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