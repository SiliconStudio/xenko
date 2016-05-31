// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D12
using System;
using SharpDX.Direct3D12;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// GraphicsResource class
    /// </summary>
    public abstract partial class GraphicsResource
    {
        internal GraphicsResource ParentResource;

        internal long StagingFenceValue;
        internal CpuDescriptorHandle NativeShaderResourceView;
        internal ResourceStates NativeResourceState;

        protected bool IsDebugMode
        {
            get
            {
                return GraphicsDevice != null && GraphicsDevice.IsDebugMode;
            }
        }
    }
}
 
#endif
