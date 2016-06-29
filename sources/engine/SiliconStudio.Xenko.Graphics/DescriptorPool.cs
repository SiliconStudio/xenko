using System;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Storage area for <see cref="DescriptorSet"/>.
    /// </summary>
    public partial class DescriptorPool : GraphicsResourceBase
    {
        public static DescriptorPool New(GraphicsDevice graphicsDevice, DescriptorTypeCount[] counts)
        {
            return new DescriptorPool(graphicsDevice, counts);
        }

#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11 || SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL || (SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN && SILICONSTUDIO_XENKO_GRAPHICS_NO_DESCRIPTOR_COPIES)
        internal readonly DescriptorSetEntry[] Entries;
        private int descriptorAllocationOffset;

        private DescriptorPool(GraphicsDevice graphicsDevice, DescriptorTypeCount[] counts)
        {
            // For now, we put everything together so let's compute total count
            var totalCount = 0;
            foreach (var count in counts)
            {
                totalCount += count.Count;
            }

            Entries = new DescriptorSetEntry[totalCount];
        }

        public void Reset()
        {
            descriptorAllocationOffset = 0;

            Array.Clear(Entries, 0, Entries.Length);
        }

        internal int Allocate(int size)
        {
            if (descriptorAllocationOffset + size > Entries.Length)
                return -1;

            var result = descriptorAllocationOffset;
            descriptorAllocationOffset += size;
            return result;
        }
#endif
    }
}