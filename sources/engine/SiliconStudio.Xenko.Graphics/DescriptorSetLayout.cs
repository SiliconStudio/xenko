using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    // D3D11 version
    /// <summary>
    /// Defines a list of descriptor layout. This is used to allocate a <see cref="DescriptorSet"/>.
    /// </summary>
    public partial class DescriptorSetLayout : GraphicsResourceBase
    {
        public static DescriptorSetLayout New(GraphicsDevice device, DescriptorSetLayoutBuilder builder)
        {
            return new DescriptorSetLayout(device, builder);
        }

#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11 || SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL || (SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN && SILICONSTUDIO_XENKO_GRAPHICS_NO_DESCRIPTOR_COPIES)
        internal readonly int ElementCount;
        internal readonly DescriptorSetLayoutBuilder.Entry[] Entries;

        private DescriptorSetLayout(GraphicsDevice device, DescriptorSetLayoutBuilder builder)
        {
            ElementCount = builder.ElementCount;
            Entries = builder.Entries.ToArray();
        }
#endif
    }
}