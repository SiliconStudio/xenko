// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D 
using System;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Describes rasterizer state, that determines how to convert vector data (shapes) into raster data (pixels).
    /// </summary>
    public partial class RasterizerState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RasterizerState"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="name">The name.</param>
        /// <param name="rasterizerStateDescription">The rasterizer state description.</param>
        private RasterizerState(GraphicsDevice device, RasterizerStateDescription rasterizerStateDescription) : base(device)
        {
            Description = rasterizerStateDescription;

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

        /// <summary>
        /// Applies this instance to the pipeline stage.
        /// </summary>
        /// <param name="device">The device.</param>
        internal void Apply(GraphicsDevice device)
        {
            device.NativeDeviceContext.Rasterizer.State = (SharpDX.Direct3D11.RasterizerState)NativeDeviceChild;
        }

        private void CreateNativeDeviceChild()
        {
            SharpDX.Direct3D11.RasterizerStateDescription nativeDescription;

            nativeDescription.CullMode = (SharpDX.Direct3D11.CullMode)Description.CullMode;
            nativeDescription.FillMode = (SharpDX.Direct3D11.FillMode)Description.FillMode;
            nativeDescription.IsFrontCounterClockwise = Description.FrontFaceCounterClockwise;
            nativeDescription.DepthBias = Description.DepthBias;
            nativeDescription.SlopeScaledDepthBias = Description.SlopeScaleDepthBias;
            nativeDescription.DepthBiasClamp = Description.DepthBiasClamp;
            nativeDescription.IsDepthClipEnabled = Description.DepthClipEnable;
            nativeDescription.IsScissorEnabled = Description.ScissorTestEnable;
            nativeDescription.IsMultisampleEnabled = Description.MultiSampleAntiAlias;
            nativeDescription.IsAntialiasedLineEnabled = Description.MultiSampleAntiAliasLine;

            NativeDeviceChild = new SharpDX.Direct3D11.RasterizerState(NativeDevice, nativeDescription);
        }
    }
} 
#endif 
