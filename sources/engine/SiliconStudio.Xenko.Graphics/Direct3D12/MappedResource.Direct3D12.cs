// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
