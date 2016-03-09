// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D12
namespace SiliconStudio.Xenko.Graphics
{
    public partial struct MappedResource
    {
        internal SharpDX.Direct3D12.Resource UploadResource;
        internal int UploadOffset;
    }
}
#endif