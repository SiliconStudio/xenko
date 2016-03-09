using System;

namespace SiliconStudio.Xenko.Graphics
{
    public struct BufferPoolAllocationResult
    {
        public IntPtr Data;
        public int Size;

        public bool Uploaded;
        public Buffer Buffer;
    }
}