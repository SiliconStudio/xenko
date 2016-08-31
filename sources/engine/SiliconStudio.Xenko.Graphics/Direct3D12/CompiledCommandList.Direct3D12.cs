// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D12
using System.Collections.Generic;
using SharpDX.Direct3D12;

namespace SiliconStudio.Xenko.Graphics
{
    public partial struct CompiledCommandList
    {
        internal CommandList Builder;
        internal GraphicsCommandList NativeCommandList;
        internal CommandAllocator NativeCommandAllocator;
        internal List<DescriptorHeap> SrvHeaps;
        internal List<DescriptorHeap> SamplerHeaps;
        internal List<Texture> StagingResources;
    }
}
#endif