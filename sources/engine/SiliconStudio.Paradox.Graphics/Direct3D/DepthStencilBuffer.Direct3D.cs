// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D
using System;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using SiliconStudio.Core;
using SiliconStudio.Core.ReferenceCounting;
using Utilities = SiliconStudio.Core.Utilities;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Depth stencil buffer
    /// </summary>
    public partial class DepthStencilBuffer
    {
        internal DepthStencilView NativeDepthStencilView;

        internal bool HasStencil;

        private readonly bool isReadOnly;

        internal DepthStencilBuffer(GraphicsDevice device, Texture2D depthTexture, bool isReadOnly) : base(device)
        {
            DescriptionInternal = depthTexture.Description;
            Texture = depthTexture;
            Texture.AddReferenceInternal();
            this.isReadOnly = isReadOnly;
            InitializeViews(out HasStencil);
        }

        public static bool IsReadOnlySupported(GraphicsDevice device)
        {
            return device.Features.Profile >= GraphicsProfile.Level_11_0;
        }

        protected override void DestroyImpl()
        {
            // Do not release NativeDepthStencilView, as it is owned by the texture.
            // _renderTargetView should keep it alive anyway.
            //base.DestroyImpl();
            _nativeDeviceChild = null;
            Utilities.Dispose(ref NativeDepthStencilView);
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

            _nativeDeviceChild = Texture.NativeDeviceChild;
            InitializeViews(out HasStencil);

            return true;
        }

        private void InitializeViews(out bool hasStencil)
        {
            var nativeDescription = ((Texture2D)Texture).NativeDescription;

            if ((nativeDescription.BindFlags & BindFlags.DepthStencil) == 0)
                throw new InvalidOperationException();

            // Check that the format is supported
            if (ComputeShaderResourceFormat((Format)Texture.Description.Format) == Format.Unknown) 
                throw new NotSupportedException("Depth stencil format not supported");

            // Setup the HasStencil flag
            hasStencil = IsStencilFormat(nativeDescription.Format);

            // Create a Depth stencil view on this texture2D
            var depthStencilViewDescription = new SharpDX.Direct3D11.DepthStencilViewDescription
            {
                Format = ComputeDepthViewFormatFromTextureFormat(nativeDescription.Format),
                Flags = SharpDX.Direct3D11.DepthStencilViewFlags.None,
            };

            if (nativeDescription.ArraySize > 1)
            {
                depthStencilViewDescription.Dimension = SharpDX.Direct3D11.DepthStencilViewDimension.Texture2DArray;
                depthStencilViewDescription.Texture2DArray.ArraySize = nativeDescription.ArraySize;
                depthStencilViewDescription.Texture2DArray.FirstArraySlice = 0;
                depthStencilViewDescription.Texture2DArray.MipSlice = 0;
            }
            else
            {
                depthStencilViewDescription.Dimension = SharpDX.Direct3D11.DepthStencilViewDimension.Texture2D;
                depthStencilViewDescription.Texture2D.MipSlice = 0;
            }

            if (nativeDescription.SampleDescription.Count > 1)
                depthStencilViewDescription.Dimension = DepthStencilViewDimension.Texture2DMultisampled;

            if (isReadOnly)
            {
                if (!IsReadOnlySupported(GraphicsDevice))
                    throw new NotSupportedException("Cannot instantiate ReadOnly DepthStencilBuffer. Not supported on this device.");

                // Create a Depth stencil view on this texture2D
                depthStencilViewDescription.Flags = DepthStencilViewFlags.ReadOnlyDepth;
                if (HasStencil)
                    depthStencilViewDescription.Flags |= DepthStencilViewFlags.ReadOnlyStencil;

                // Create the Depth Stencil View
                NativeDepthStencilView = new SharpDX.Direct3D11.DepthStencilView(GraphicsDevice.NativeDevice, Texture.NativeResource, depthStencilViewDescription);
            }
            else
            {
                // Create the Depth Stencil View
                NativeDepthStencilView = new SharpDX.Direct3D11.DepthStencilView(GraphicsDevice.NativeDevice, Texture.NativeResource, depthStencilViewDescription);
            }
        }

        internal static bool IsStencilFormat(Format format)
        {
            switch (format)
            {
                case Format.R24G8_Typeless:
                case Format.D24_UNorm_S8_UInt:
                case Format.R32G8X24_Typeless:
                case Format.D32_Float_S8X24_UInt:
                    return true;
            }

            return false;
        }

        internal static Format ComputeShaderResourceFormat(Format format)
        {
            Format viewFormat;

            // Determine TypeLess Format and ShaderResourceView Format
            switch (format)
            {
                case Format.D16_UNorm:
                    viewFormat = SharpDX.DXGI.Format.R16_Float;
                    break;
                case Format.D32_Float:
                    viewFormat = SharpDX.DXGI.Format.R32_Float;
                    break;
                case Format.D24_UNorm_S8_UInt:
                    viewFormat = SharpDX.DXGI.Format.R24_UNorm_X8_Typeless;
                    break;
                case Format.D32_Float_S8X24_UInt:
                    viewFormat = SharpDX.DXGI.Format.R32_Float_X8X24_Typeless;
                    break;
                default:
                    viewFormat = Format.Unknown;
                    break;
            }

            return viewFormat;
        }

        internal static Format ComputeDepthViewFormatFromTextureFormat(Format format)
        {
            Format viewFormat;

            switch (format)
            {
                case Format.R16_Typeless:
                case Format.D16_UNorm:
                    viewFormat = Format.D16_UNorm;
                    break;
                case Format.R32_Typeless:
                case Format.D32_Float:
                    viewFormat = Format.D32_Float;
                    break;
                case Format.R24G8_Typeless:
                case Format.D24_UNorm_S8_UInt:
                    viewFormat = Format.D24_UNorm_S8_UInt;
                    break;
                case Format.R32G8X24_Typeless:
                case Format.D32_Float_S8X24_UInt:
                    viewFormat = Format.D32_Float_S8X24_UInt;
                    break;
                default:
                    throw new NotSupportedException(string.Format("Unsupported depth format [{0}]", format));
            }

            return viewFormat;
        }
    }
}
 
#endif 
