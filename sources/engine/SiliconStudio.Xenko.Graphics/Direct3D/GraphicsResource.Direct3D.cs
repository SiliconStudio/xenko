// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D11
using System;

using SharpDX.Direct3D11;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// GraphicsResource class
    /// </summary>
    public abstract partial class GraphicsResource
    {
        private ShaderResourceView shaderResourceView;
        private UnorderedAccessView unorderedAccessView;

        protected bool IsDebugMode
        {
            get
            {
                return GraphicsDevice != null && GraphicsDevice.IsDebugMode;
            }
        }

        protected override void OnNameChanged()
        {
            base.OnNameChanged();
            if (IsDebugMode)
            {
                if (this.shaderResourceView != null)
                {
                    shaderResourceView.DebugName = Name == null ? null : String.Format("{0} SRV", Name);
                }

                if (this.unorderedAccessView != null)
                {
                    unorderedAccessView.DebugName = Name == null ? null : String.Format("{0} UAV", Name);
                }
            }
        }

        /// <summary>
        /// Gets or sets the ShaderResourceView attached to this GraphicsResource.
        /// Note that only Texture, Texture3D, RenderTarget2D, RenderTarget3D, DepthStencil are using this ShaderResourceView
        /// </summary>
        /// <value>The device child.</value>
        protected internal SharpDX.Direct3D11.ShaderResourceView NativeShaderResourceView
        {
            get
            {
                return shaderResourceView;
            }
            set
            {
                shaderResourceView = value;

                if (IsDebugMode && shaderResourceView != null)
                {
                    shaderResourceView.DebugName = Name == null ? null : String.Format("{0} SRV", Name);
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
                unorderedAccessView = value;

                if (IsDebugMode && unorderedAccessView != null)
                {
                    unorderedAccessView.DebugName = Name == null ? null : String.Format("{0} UAV", Name);
                }
            }
        }

        protected internal override void OnDestroyed()
        {
            ReleaseComObject(ref shaderResourceView);
            ReleaseComObject(ref unorderedAccessView);

            base.OnDestroyed();
        }
    }
}
 
#endif
