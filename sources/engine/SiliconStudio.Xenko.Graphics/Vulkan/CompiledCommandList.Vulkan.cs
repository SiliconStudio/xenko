// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using System.Collections.Generic;
using SharpVulkan;

namespace SiliconStudio.Xenko.Graphics
{
    public partial struct CompiledCommandList
    {
        internal CommandList Builder;
        internal CommandBuffer NativeCommandBuffer;
        internal List<SharpVulkan.DescriptorPool> DescriptorPools;
        internal List<Texture> StagingResources;
    }
}
#endif
