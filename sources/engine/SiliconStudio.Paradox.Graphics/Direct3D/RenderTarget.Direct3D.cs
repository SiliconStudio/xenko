// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D 
using System;
using SharpDX;
using SharpDX.Direct3D11;
using Utilities = SiliconStudio.Core.Utilities;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A renderable Texture2D.
    /// </summary>
    public partial class RenderTarget
    {
        private readonly GraphicsDevice device;
        private SharpDX.Direct3D11.RenderTargetView renderTargetView;

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
        {
            this.device = device;
            Description = texture.Description;

            Texture = texture;
            NativeRenderTargetView = texture.GetRenderTargetView(viewType, arraySlice, mipSlice);

            Width = Math.Max(1, Description.Width >> mipSlice);
            Height = Math.Max(1, Description.Height >> mipSlice);

            ViewType = viewType;
            ArraySlice = arraySlice;
            MipLevel = mipSlice;
            ViewFormat = viewFormat == PixelFormat.None ? Description.Format : viewFormat;
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
        {
            this.device = device;
            Description = texture.Description;

            Texture = texture;
            NativeRenderTargetView = view;

            Width = Math.Max(1, Description.Width >> mipSlice);
            Height = Math.Max(1, Description.Height >> mipSlice);

            var viewFormat = view.Description.Format;

            ViewType = viewType;
            ArraySlice = arraySlice;
            MipLevel = mipSlice;
            ViewFormat = (PixelFormat)viewFormat;
        }

        internal void Destroy()
        {
            Utilities.Dispose(ref renderTargetView);
        }

        /// <inheritdoc/>
        internal void OnRecreate()
        {
            NativeRenderTargetView = Texture.GetRenderTargetView(ViewType, ArraySlice, MipLevel);
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
                return renderTargetView;
            }
            set
            {
                if (renderTargetView != null) throw new ArgumentException(string.Format(FrameworkResources.GraphicsResourceAlreadySet, "RenderTargetView"), "value");
                renderTargetView = value;

                // Associate PrivateData to this DeviceResource
                GraphicsResourceBase.SetDebugName(device, renderTargetView, "RenderTargetView " + (Texture.Name ?? string.Empty));
            }
        }
    }
}
 
#endif 
