// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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