// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D12
using System;
using SharpDX;
using SharpDX.Direct3D12;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Describes a sampler state used for texture sampling.
    /// </summary>
    public partial class SamplerState
    {
        internal CpuDescriptorHandle NativeSampler;

        /// <summary>
        /// Initializes a new instance of the <see cref="SamplerState"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="name">The name.</param>
        /// <param name="samplerStateDescription">The sampler state description.</param>
        private SamplerState(GraphicsDevice device, SamplerStateDescription samplerStateDescription) : base(device)
        {
            Description = samplerStateDescription;

            CreateNativeDeviceChild();
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
            SharpDX.Direct3D12.SamplerStateDescription nativeDescription;

            nativeDescription.AddressU = (SharpDX.Direct3D12.TextureAddressMode)Description.AddressU;
            nativeDescription.AddressV = (SharpDX.Direct3D12.TextureAddressMode)Description.AddressV;
            nativeDescription.AddressW = (SharpDX.Direct3D12.TextureAddressMode)Description.AddressW;
            nativeDescription.BorderColor = ColorHelper.Convert(Description.BorderColor);
            nativeDescription.ComparisonFunction = (SharpDX.Direct3D12.Comparison)Description.CompareFunction;
            nativeDescription.Filter = (SharpDX.Direct3D12.Filter)Description.Filter;
            nativeDescription.MaximumAnisotropy = Description.MaxAnisotropy;
            nativeDescription.MaximumLod = Description.MaxMipLevel;
            nativeDescription.MinimumLod = Description.MinMipLevel;
            nativeDescription.MipLodBias = Description.MipMapLevelOfDetailBias;

            NativeSampler = GraphicsDevice.SamplerAllocator.Allocate(1);
            GraphicsDevice.NativeDevice.CreateSampler(nativeDescription, NativeSampler);
        }
    }
} 
#endif
