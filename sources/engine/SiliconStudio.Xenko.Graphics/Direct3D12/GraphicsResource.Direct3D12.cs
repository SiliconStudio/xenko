// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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

        internal long? StagingFenceValue;
        internal CommandList StagingBuilder;
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
