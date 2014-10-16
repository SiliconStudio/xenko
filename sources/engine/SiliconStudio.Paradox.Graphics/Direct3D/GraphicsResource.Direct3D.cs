// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D 
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// GraphicsResource class
    /// </summary>
    public abstract partial class GraphicsResource
    {
        protected ShaderResourceView nativeShaderResourceView;
        private UnorderedAccessView unorderedAccessView;

        protected override void OnNameChanged()
        {
            base.OnNameChanged();
            if (GraphicsDevice != null && GraphicsDevice.IsDebugMode)
            {
                if (this.nativeShaderResourceView != null)
                {
                    nativeShaderResourceView.DebugName = Name == null ? null : String.Format("{0} SRV", Name);
                }

                if (this.unorderedAccessView != null)
                {
                    unorderedAccessView.DebugName = Name == null ? null : String.Format("{0} UAV", Name);
                }
            }
        }

        /// <summary>
        /// Gets or sets the ShaderResourceView attached to this GraphicsResource.
        /// Note that only Texture2D, Texture3D, RenderTarget2D, RenderTarget3D, DepthStencil are using this ShaderResourceView
        /// </summary>
        /// <value>The device child.</value>
        protected internal SharpDX.Direct3D11.ShaderResourceView NativeShaderResourceView
        {
            get
            {
                return nativeShaderResourceView;
            }
            set
            {
                Debug.Assert(nativeShaderResourceView == null);
                nativeShaderResourceView = value;

                if (nativeShaderResourceView != null)
                {
                    // Associate PrivateData to this DeviceResource
                    SetDebugName(GraphicsDevice, nativeShaderResourceView, "SRV " + Name);
                }
            }
        }

        /// <summary>
        /// Gets or sets the UnorderedAccessView attached to this GraphicsResource.
        /// </summary>
        /// <value>The device child.</value>
        protected internal UnorderedAccessView NativeUnorderedAccessView
        {
            get
            {
                return unorderedAccessView;
            }
            set
            {
                Debug.Assert(unorderedAccessView == null);
                unorderedAccessView = value;

                if (unorderedAccessView != null)
                {
                    // Associate PrivateData to this DeviceResource
                    SetDebugName(GraphicsDevice, unorderedAccessView, "UAV " + Name);
                }
            }
        }

        protected override void DestroyImpl()
        {
            if (nativeShaderResourceView != null)
            {
                ((IUnknown)nativeShaderResourceView).Release();
                nativeShaderResourceView = null;
            }

            if (unorderedAccessView != null)
            {
                ((IUnknown)unorderedAccessView).Release();
                unorderedAccessView = null;
            }

            base.DestroyImpl();
        }
    }
}
 
#endif
