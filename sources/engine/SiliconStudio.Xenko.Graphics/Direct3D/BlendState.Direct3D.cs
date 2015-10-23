// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D 
using System;
using SiliconStudio.Core.Mathematics;
using SharpDX;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Describes a blend state.
    /// </summary>
    public partial class BlendState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlendState"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="name">The name.</param>
        /// <param name="blendStateDescription">The blend state description.</param>
        internal BlendState(GraphicsDevice device, BlendStateDescription blendStateDescription)
            : base(device)
        {
            BlendFactor = SiliconStudio.Core.Mathematics.Color4.White;
            MultiSampleMask = -1;

            Description = blendStateDescription;

            CreateNativeDeviceChild();
        }

        /// <inheritdoc/>
        protected internal override void OnDestroyed()
        {
            base.OnDestroyed();
            DestroyImpl();
        }

        /// <inheritdoc/>
        protected internal override bool OnRecreate()
        {
            base.OnRecreate();
            CreateNativeDeviceChild();
            return true;
        }

        private void CreateNativeDeviceChild()
        {
            var nativeDescription = new SharpDX.Direct3D11.BlendStateDescription();

            nativeDescription.AlphaToCoverageEnable = Description.AlphaToCoverageEnable;
            nativeDescription.IndependentBlendEnable = Description.IndependentBlendEnable;
            for (int i = 0; i < Description.RenderTargets.Length; ++i)
            {
                nativeDescription.RenderTarget[i].IsBlendEnabled = Description.RenderTargets[i].BlendEnable;
                nativeDescription.RenderTarget[i].SourceBlend = (SharpDX.Direct3D11.BlendOption)Description.RenderTargets[i].ColorSourceBlend;
                nativeDescription.RenderTarget[i].DestinationBlend = (SharpDX.Direct3D11.BlendOption)Description.RenderTargets[i].ColorDestinationBlend;
                nativeDescription.RenderTarget[i].BlendOperation = (SharpDX.Direct3D11.BlendOperation)Description.RenderTargets[i].ColorBlendFunction;
                nativeDescription.RenderTarget[i].SourceAlphaBlend = (SharpDX.Direct3D11.BlendOption)Description.RenderTargets[i].AlphaSourceBlend;
                nativeDescription.RenderTarget[i].DestinationAlphaBlend = (SharpDX.Direct3D11.BlendOption)Description.RenderTargets[i].AlphaDestinationBlend;
                nativeDescription.RenderTarget[i].AlphaBlendOperation = (SharpDX.Direct3D11.BlendOperation)Description.RenderTargets[i].AlphaBlendFunction;
                nativeDescription.RenderTarget[i].RenderTargetWriteMask = (SharpDX.Direct3D11.ColorWriteMaskFlags)Description.RenderTargets[i].ColorWriteChannels;
            }

            NativeDeviceChild = new SharpDX.Direct3D11.BlendState(NativeDevice, nativeDescription);
        }
    }
}
 
#endif 
