using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    public struct ResourceGroup
    {
        public DescriptorSet DescriptorSet;
        public int ConstantBufferOffset;
        public int ConstantBufferSize;
    }
}