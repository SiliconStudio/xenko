using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    // D3D11 version
    /// <summary>
    /// Defines a list of descriptor layout. This is used to allocate a <see cref="DescriptorSet"/>.
    /// </summary>
    public partial class DescriptorSetLayout
    {
        public static DescriptorSetLayout New(GraphicsDevice device, DescriptorSetLayoutBuilder builder)
        {
            return new DescriptorSetLayout(device, builder);
        }

#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D || SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
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