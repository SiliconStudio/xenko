// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D 
// Copyright (c) 2011-2012 Silicon Studio

using System;

using SharpDX.Direct3D11;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class FakeTexture
    {
        internal override ShaderResourceView GetShaderResourceView(ViewType viewType, int arrayOrDepthSlice, int mipIndex)
        {
            throw new NotImplementedException();
        }

        internal override RenderTargetView GetRenderTargetView(ViewType viewType, int arrayOrDepthSlice, int mipMapSlice)
        {
            throw new NotImplementedException();
        }

        internal override UnorderedAccessView GetUnorderedAccessView(int arrayOrDepthSlice, int mipMapSlice)
        {
            throw new NotImplementedException();
        }         
    }
}
#endif