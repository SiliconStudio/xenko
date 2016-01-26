using System;
using SiliconStudio.Xenko.Graphics;

namespace RenderArchitecture
{
    public class BufferPool
    {
        public ConstantBuffer2 Buffer { get; }

        private int bufferAllocationOffset;

        internal BufferPool(int size)
        {
            Buffer = new ConstantBuffer2(size);
        }

        public static BufferPool New(GraphicsDevice graphicsDevice, int size)
        {
            return new BufferPool(size);
        }

        public void Reset()
        {
            bufferAllocationOffset = 0;
        }

        public int Allocate(int size)
        {
            var result = bufferAllocationOffset;
            bufferAllocationOffset += size;

            if (bufferAllocationOffset > Buffer.Size)
                throw new InvalidOperationException();

            return result;
        }
    }
}