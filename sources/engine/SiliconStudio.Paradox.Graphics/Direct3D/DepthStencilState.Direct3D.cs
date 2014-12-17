// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D 
using System;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Contains depth-stencil state for the device.
    /// </summary>
    public partial class DepthStencilState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencilState"/> class.
        /// </summary>
        /// <param name="depthEnable">if set to <c>true</c> [depth enable].</param>
        /// <param name="depthWriteEnable">if set to <c>true</c> [depth write enable].</param>
        /// <param name="name">The name.</param>
        private DepthStencilState(GraphicsDevice device, DepthStencilStateDescription depthStencilStateDescription)
            : base(device)
        {
            Description = depthStencilStateDescription;

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
            SharpDX.Direct3D11.DepthStencilStateDescription nativeDescription;

            nativeDescription.IsDepthEnabled = Description.DepthBufferEnable;
            nativeDescription.DepthComparison = (SharpDX.Direct3D11.Comparison)Description.DepthBufferFunction;
            nativeDescription.DepthWriteMask = Description.DepthBufferWriteEnable ? SharpDX.Direct3D11.DepthWriteMask.All : SharpDX.Direct3D11.DepthWriteMask.Zero;

            nativeDescription.IsStencilEnabled = Description.StencilEnable;
            nativeDescription.StencilReadMask = Description.StencilMask;
            nativeDescription.StencilWriteMask = Description.StencilWriteMask;

            nativeDescription.FrontFace.FailOperation = (SharpDX.Direct3D11.StencilOperation)Description.FrontFace.StencilFail;
            nativeDescription.FrontFace.PassOperation = (SharpDX.Direct3D11.StencilOperation)Description.FrontFace.StencilPass;
            nativeDescription.FrontFace.DepthFailOperation = (SharpDX.Direct3D11.StencilOperation)Description.FrontFace.StencilDepthBufferFail;
            nativeDescription.FrontFace.Comparison = (SharpDX.Direct3D11.Comparison)Description.FrontFace.StencilFunction;

            nativeDescription.BackFace.FailOperation = (SharpDX.Direct3D11.StencilOperation)Description.BackFace.StencilFail;
            nativeDescription.BackFace.PassOperation = (SharpDX.Direct3D11.StencilOperation)Description.BackFace.StencilPass;
            nativeDescription.BackFace.DepthFailOperation = (SharpDX.Direct3D11.StencilOperation)Description.BackFace.StencilDepthBufferFail;
            nativeDescription.BackFace.Comparison = (SharpDX.Direct3D11.Comparison)Description.BackFace.StencilFunction;

            NativeDeviceChild = new SharpDX.Direct3D11.DepthStencilState(NativeDevice, nativeDescription);
        }
    }
} 
#endif 
