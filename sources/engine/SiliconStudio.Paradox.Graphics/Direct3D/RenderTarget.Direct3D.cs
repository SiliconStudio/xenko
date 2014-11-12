// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D 
using System;
using SharpDX;
using SharpDX.Direct3D11;
using SiliconStudio.Core.ReferenceCounting;
using Utilities = SiliconStudio.Core.Utilities;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A renderable Texture2D.
    /// </summary>
    public partial class RenderTarget
    {
        private SharpDX.Direct3D11.RenderTargetView _renderTargetView;

        /// <summary>
        /// Create a RenderTarget from a texture.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="texture">The texture.</param>
        /// <param name="viewType">The view type.</param>
        /// <param name="arraySlice">The index of the array slice.</param>
        /// <param name="mipSlice">The index of the mip slice.</param>
        /// <param name="viewFormat">The pixel format.</param>
        internal RenderTarget(GraphicsDevice device, Texture texture, ViewType viewType, int arraySlice, int mipSlice, PixelFormat viewFormat = PixelFormat.None)
            : base(device)
        {
            _nativeDeviceChild = texture.NativeDeviceChild;
            Description = texture.Description;

            NativeRenderTargetView = texture.GetRenderTargetView(viewType, arraySlice, mipSlice);

            Width = Math.Max(1, Description.Width >> mipSlice);
            Height = Math.Max(1, Description.Height >> mipSlice);

            ViewType = viewType;
            ArraySlice = arraySlice;
            MipLevel = mipSlice;
            ViewFormat = viewFormat == PixelFormat.None ? Description.Format : viewFormat;

            Texture = texture;
            Texture.AddReferenceInternal();
        }

        /// <summary>
        /// Create a RenderTarget from a texture.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="texture">The texture.</param>
        /// <param name="viewType">The view type.</param>
        /// <param name="arraySlice">The index of the array slice.</param>
        /// <param name="mipSlice">The index of the mip slice.</param>
        /// <param name="view">The render target view.</param>
        internal RenderTarget(GraphicsDevice device, Texture texture, ViewType viewType, int arraySlice, int mipSlice, RenderTargetView view)
            : base(device)
        {
            _nativeDeviceChild = texture.NativeDeviceChild; //._nativeDeviceChild;
            Description = texture.Description;

            NativeRenderTargetView = view;

            Width = Math.Max(1, Description.Width >> mipSlice);
            Height = Math.Max(1, Description.Height >> mipSlice);

            var viewFormat = view.Description.Format;

            ViewType = viewType;
            ArraySlice = arraySlice;
            MipLevel = mipSlice;
            ViewFormat = (PixelFormat)viewFormat;

            Texture = texture;
        }

        protected override void DestroyImpl()
        {
            // Do not release _nativeDeviceChild, as it is owned by the texture.
            // _renderTargetView should keep it alive anyway.
            //base.DestroyImpl();
            _nativeDeviceChild = null;
            Utilities.Dispose(ref _renderTargetView);
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
            // Dependency: wait for underlying texture to be recreated first
            if (Texture.LifetimeState != GraphicsResourceLifetimeState.Active)
                return false;

            base.OnRecreate();
            _nativeDeviceChild = Texture.NativeDeviceChild;
            NativeRenderTargetView = Texture.GetRenderTargetView(ViewType, ArraySlice, MipLevel);

            return true;
        }

        /// <summary>
        /// Gets or sets the RenderTargetView attached to this GraphicsResource.
        /// Note that only Texture2D, Texture3D, RenderTarget2D, RenderTarget3D, DepthStencil are using this ShaderResourceView
        /// </summary>
        /// <value>The device child.</value>
        internal SharpDX.Direct3D11.RenderTargetView NativeRenderTargetView
        {
            get
            {
                return _renderTargetView;
            }
            set
            {
                if (_renderTargetView != null) throw new ArgumentException(string.Format(FrameworkResources.GraphicsResourceAlreadySet, "RenderTargetView"), "value");
                _renderTargetView = value;

                // Associate PrivateData to this DeviceResource
                SetDebugName(GraphicsDevice, _renderTargetView, "View " + Name);
            }
        }
    }
}
 
#endif 
