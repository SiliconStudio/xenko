using System;

namespace SiliconStudio.Xenko.Graphics
{
    public class ResourceGroup
    {
        public DescriptorSet DescriptorSet;

        public BufferPoolAllocationResult ConstantBuffer;
    }

    public struct BufferPoolAllocationResult
    {
        public IntPtr Data;
        public int Size;

        public bool Uploaded;
        public Buffer Buffer;
    }
}