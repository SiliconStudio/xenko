// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_NULL

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Defines a list of descriptor layout. This is used to allocate a <see cref="DescriptorSet"/>.
    /// </summary>
    public partial class DescriptorSetLayout : GraphicsResourceBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="DescriptorSetLayout"/> for <param name="device"/> using the 
        /// <see cref="DescriptorSetLayoutBuilder"/> <param name="builder"/>.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="builder">The descriptor set layout builder.</param>
        private DescriptorSetLayout(GraphicsDevice device, DescriptorSetLayoutBuilder builder)
        {
        }
    }
}

#endif
