// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
namespace SiliconStudio.Xenko.Graphics
{
    public partial struct MappedResource
    {
        internal SharpVulkan.Buffer UploadResource;
        internal int UploadOffset;
    }
}
#endif
