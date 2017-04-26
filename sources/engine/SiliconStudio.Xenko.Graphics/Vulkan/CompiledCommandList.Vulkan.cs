// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
