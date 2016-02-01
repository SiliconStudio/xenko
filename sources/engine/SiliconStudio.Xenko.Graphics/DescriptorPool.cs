using System;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Storage area for <see cref="DescriptorSet"/>.
    /// </summary>
    public class DescriptorPool : GraphicsResourceBase
    {
        internal readonly DescriptorSetEntry[] Entries;
        private int descriptorAllocationOffset;

        public DescriptorPool(int totalCount)
        {
            Entries = new DescriptorSetEntry[totalCount];
        }

        public static DescriptorPool New(GraphicsDevice device, DescriptorTypeCount[] counts)
        {
            // For now, we put everything together so let's compute total count
            var totalCount = 0;
            foreach (var count in counts)
            {
                totalCount = count.Count;
            }

            return new DescriptorPool(totalCount);
        }

        public void Reset()
        {
            descriptorAllocationOffset = 0;

            Array.Clear(Entries, 0, Entries.Length);
        }

        internal int Allocate(int size)
        {
            var result = descriptorAllocationOffset;
            descriptorAllocationOffset += size;
            return result;
        }
    }
}