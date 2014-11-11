// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.ReferenceCounting;
using SiliconStudio.Paradox.Games;
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D 
// Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.IO;

using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.IO;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A Texture 2D frontend to <see cref="SharpDX.Direct3D11.Texture2D"/>.
    /// </summary>
    public partial class Texture2DBase
    {
        private SharpDX.DXGI.Surface dxgiSurface;
        protected internal Texture2DDescription NativeDescription;
        internal Texture2D TextureDepthStencilBuffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2DBase" /> class.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="description2D">The description.</param>
        /// <param name="dataBoxes">A variable-length parameters list containing data rectangles.</param>
        protected internal Texture2DBase(GraphicsDevice device, TextureDescription description2D, DataBox[] dataBoxes = null)
            : base(device, CheckMipLevels(device, ref description2D), ViewType.Full, 0, 0)
        {
            NativeDescription = ConvertToNativeDescription(device, description2D);
            //System.Diagnostics.Debug.WriteLine("Texture2D {0}x{1} {2}", NativeDescription.Width, NativeDescription.Height, NativeDescription.Format);
            NativeDeviceChild = new SharpDX.Direct3D11.Texture2D(device.NativeDevice, NativeDescription, ConvertDataBoxes(dataBoxes));
            NativeShaderResourceView = GetShaderResourceView(ViewType, ArraySlice, MipLevel);
            NativeUnorderedAccessView = GetUnorderedAccessView(ArraySlice, MipLevel);

            // If we have a depthStencilBufferForShaderResource, then we should override the default shader resource view
            if (TextureDepthStencilBuffer != null)
                nativeShaderResourceView = TextureDepthStencilBuffer.nativeShaderResourceView;
        }

        /// <summary>
        /// Specialised constructor for use only by derived classes.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice" />.</param>
        /// <param name="texture">The texture.</param>
        protected internal Texture2DBase(GraphicsDevice device, Texture2DBase texture, ViewType viewType, int viewArraySlice, int viewMipLevel, PixelFormat viewFormat = PixelFormat.None)
            : base(device, texture, viewType, viewArraySlice, viewMipLevel, viewFormat)
        {
            // Copy the device child, but don't use NativeDeviceChild, as it is registering it for disposing.
            _nativeDeviceChild = texture._nativeDeviceChild;
            NativeResource = texture.NativeResource;
            NativeDescription = texture.NativeDescription;
            NativeShaderResourceView = GetShaderResourceView(viewType, viewArraySlice, viewMipLevel);
            NativeUnorderedAccessView = GetUnorderedAccessView(viewArraySlice, viewMipLevel);
            dxgiSurface = texture.dxgiSurface;
        }

        /// <summary>
        /// Specialised constructor for use only by derived classes.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="texture">The texture.</param>
        protected internal Texture2DBase(GraphicsDevice device, SharpDX.Direct3D11.Texture2D texture)
            : base(device, ConvertFromNativeDescription(texture.Description), ViewType.Full, 0, 0)
        {
            // Copy the device child, but don't use NativeDeviceChild, as it is registering it for disposing.
            NativeDescription = texture.Description;
            NativeDeviceChild = texture;
            NativeShaderResourceView = GetShaderResourceView(ViewType, ArraySlice, MipLevel);
            NativeUnorderedAccessView = GetUnorderedAccessView(ArraySlice, MipLevel);
        }

        internal override ShaderResourceView GetShaderResourceView(ViewType viewType, int arrayOrDepthSlice, int mipIndex)
        {
            if ((this.NativeDescription.BindFlags & BindFlags.ShaderResource) == 0)
                return null;

            int arrayCount;
            int mipCount;
            GetViewSliceBounds(viewType, ref arrayOrDepthSlice, ref mipIndex, out arrayCount, out mipCount);

            // Create the view
            var srvDescription = new ShaderResourceViewDescription() { Format = ComputeShaderResourceViewFormat()};

            // Initialize for texture arrays or texture cube
            if (this.Description.ArraySize > 1)
            {
                // If texture cube
                if ((this.NativeDescription.OptionFlags & ResourceOptionFlags.TextureCube) != 0)
                {
                    srvDescription.Dimension = ShaderResourceViewDimension.TextureCube;
                    srvDescription.TextureCube.MipLevels = mipCount;
                    srvDescription.TextureCube.MostDetailedMip = mipIndex;
                }
                else
                {
                    // Else regular Texture2D
                    srvDescription.Dimension = this.NativeDescription.SampleDescription.Count > 1 ? ShaderResourceViewDimension.Texture2DMultisampledArray : ShaderResourceViewDimension.Texture2DArray;

                    // Multisample?
                    if (this.NativeDescription.SampleDescription.Count > 1)
                    {
                        srvDescription.Texture2DMSArray.ArraySize = arrayCount;
                        srvDescription.Texture2DMSArray.FirstArraySlice = arrayOrDepthSlice;
                    }
                    else
                    {
                        srvDescription.Texture2DArray.ArraySize = arrayCount;
                        srvDescription.Texture2DArray.FirstArraySlice = arrayOrDepthSlice;
                        srvDescription.Texture2DArray.MipLevels = mipCount;
                        srvDescription.Texture2DArray.MostDetailedMip = mipIndex;
                    }
                }
            }
            else
            {
                srvDescription.Dimension = this.NativeDescription.SampleDescription.Count > 1 ? ShaderResourceViewDimension.Texture2DMultisampled : ShaderResourceViewDimension.Texture2D;
                if (this.NativeDescription.SampleDescription.Count <= 1)
                {
                    srvDescription.Texture2D.MipLevels = mipCount;
                    srvDescription.Texture2D.MostDetailedMip = mipIndex;
                }
            }

            // Default ShaderResourceView
            return new ShaderResourceView(this.GraphicsDevice.NativeDevice, NativeResource, srvDescription);
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
            // Dependency: wait for underlying texture to be recreated
            if (ParentTexture != null && ParentTexture.LifetimeState != GraphicsResourceLifetimeState.Active)
                return false;

            base.OnRecreate();

            // Only a view?
            if (ParentTexture != null)
            {
                // Copy the device child, but don't use NativeDeviceChild, as it is registering it for disposing.
                _nativeDeviceChild = ParentTexture._nativeDeviceChild;
                NativeResource = ParentTexture.NativeResource;
                NativeDescription = ((Texture2D)ParentTexture).NativeDescription;
                dxgiSurface = ((Texture2D)ParentTexture).dxgiSurface;

                return true;
            }
            else
            {
                // Render Target / Depth Stencil are considered as "dynamic"
                if ((Description.Usage == GraphicsResourceUsage.Immutable
                     || Description.Usage == GraphicsResourceUsage.Default)
                    && (NativeDescription.BindFlags & (BindFlags.RenderTarget | BindFlags.DepthStencil)) == 0)
                    return false;

                NativeDeviceChild = new SharpDX.Direct3D11.Texture2D(GraphicsDevice.NativeDevice, NativeDescription);
            }

            // Recreate SRV/UAV
            NativeShaderResourceView = GetShaderResourceView(ViewType, ArraySlice, MipLevel);
            NativeUnorderedAccessView = GetUnorderedAccessView(ArraySlice, MipLevel);

            // If we have a depthStencilBufferForShaderResource, then we should override the default shader resource view
            if (TextureDepthStencilBuffer != null)
                nativeShaderResourceView = TextureDepthStencilBuffer.nativeShaderResourceView;

            return true;
        }

        public override Texture Clone()
        {
            throw new NotImplementedException();
        }

        internal void Recreate(Texture2DDescription description)
        {
            NativeDescription = description;
            NativeDeviceChild = new SharpDX.Direct3D11.Texture2D(GraphicsDevice.NativeDevice, NativeDescription);
            NativeShaderResourceView = GetShaderResourceView(ViewType, ArraySlice, MipLevel);
            NativeUnorderedAccessView = GetUnorderedAccessView(ArraySlice, MipLevel);

            // If we have a depthStencilBufferForShaderResource, then we should override the default shader resource view
            if (TextureDepthStencilBuffer != null)
                nativeShaderResourceView = TextureDepthStencilBuffer.nativeShaderResourceView;
        }

        internal void Recreate(SharpDX.Direct3D11.Texture2D texture2D)
        {
            NativeDescription = texture2D.Description;
            NativeDeviceChild = texture2D;
            NativeShaderResourceView = GetShaderResourceView(ViewType, ArraySlice, MipLevel);
            NativeUnorderedAccessView = GetUnorderedAccessView(ArraySlice, MipLevel);

            // If we have a depthStencilBufferForShaderResource, then we should override the default shader resource view
            if (TextureDepthStencilBuffer != null)
                nativeShaderResourceView = TextureDepthStencilBuffer.nativeShaderResourceView;
        }

        public override void Recreate(DataBox[] dataBoxes = null)
        {
            NativeDeviceChild = new SharpDX.Direct3D11.Texture2D(GraphicsDevice.NativeDevice, NativeDescription, ConvertDataBoxes(dataBoxes));
            NativeShaderResourceView = GetShaderResourceView(ViewType, ArraySlice, MipLevel);
            NativeUnorderedAccessView = GetUnorderedAccessView(ArraySlice, MipLevel);

            // If we have a depthStencilBufferForShaderResource, then we should override the default shader resource view
            if (TextureDepthStencilBuffer != null)
                nativeShaderResourceView = TextureDepthStencilBuffer.nativeShaderResourceView;
        }

        private Format ComputeShaderResourceViewFormat()
        {
            var format = NativeDescription.Format;
            Format viewFormat = format;

            // Special case for DepthStencil ShaderResourceView that are bound as Float
            if ((Description.Flags & TextureFlags.DepthStencil) != 0)
            {
                // Determine TypeLess Format and ShaderResourceView Format
                switch (format)
                {
                    case SharpDX.DXGI.Format.R16_Typeless:
                        viewFormat = SharpDX.DXGI.Format.R16_Float;
                        break;
                    case SharpDX.DXGI.Format.R32_Typeless:
                        viewFormat = SharpDX.DXGI.Format.R32_Float;
                        break;
                    case SharpDX.DXGI.Format.R24G8_Typeless:
                        viewFormat = SharpDX.DXGI.Format.R24_UNorm_X8_Typeless;
                        break;
                    case SharpDX.DXGI.Format.R32G8X24_Typeless:
                        viewFormat = SharpDX.DXGI.Format.R32_Float_X8X24_Typeless;
                        break;
                }
            }

            return viewFormat;
        }

        internal override UnorderedAccessView GetUnorderedAccessView(int arrayOrDepthSlice, int mipIndex)
        {
            if ((this.NativeDescription.BindFlags & BindFlags.UnorderedAccess) == 0)
                return null;

            int arrayCount;
            int mipCount;
            GetViewSliceBounds(ViewType.Single, ref arrayOrDepthSlice, ref mipIndex, out arrayCount, out mipCount);

            var uavDescription = new UnorderedAccessViewDescription()
                {
                    Format = (Format)this.Description.Format,
                    Dimension = this.Description.ArraySize > 1 ? UnorderedAccessViewDimension.Texture2DArray : UnorderedAccessViewDimension.Texture2D
                };

            if (this.Description.ArraySize > 1)
            {
                uavDescription.Texture2DArray.ArraySize = arrayCount;
                uavDescription.Texture2DArray.FirstArraySlice = arrayOrDepthSlice;
                uavDescription.Texture2DArray.MipSlice = mipIndex;
            }
            else
            {
                uavDescription.Texture2D.MipSlice = mipIndex;
            }

            return new UnorderedAccessView(GraphicsDevice.NativeDevice, NativeResource, uavDescription);
        }

        internal override RenderTargetView GetRenderTargetView(ViewType viewType, int arrayOrDepthSlice, int mipIndex)
        {
            if ((this.NativeDescription.BindFlags & BindFlags.RenderTarget) == 0)
                return null;

            if (viewType == ViewType.MipBand)
                throw new NotSupportedException("ViewSlice.MipBand is not supported for render targets");

            int arrayCount;
            int mipCount;
            GetViewSliceBounds(viewType, ref arrayOrDepthSlice, ref mipIndex, out arrayCount, out mipCount);

            // Create the render target view
            var rtvDescription = new RenderTargetViewDescription() { Format = this.NativeDescription.Format };

            if (this.Description.ArraySize > 1)
            {
                rtvDescription.Dimension = this.NativeDescription.SampleDescription.Count > 1 ? RenderTargetViewDimension.Texture2DMultisampledArray : RenderTargetViewDimension.Texture2DArray;
                if (this.NativeDescription.SampleDescription.Count > 1)
                {
                    rtvDescription.Texture2DMSArray.ArraySize = arrayCount;
                    rtvDescription.Texture2DMSArray.FirstArraySlice = arrayOrDepthSlice;
                }
                else
                {
                    rtvDescription.Texture2DArray.ArraySize = arrayCount;
                    rtvDescription.Texture2DArray.FirstArraySlice = arrayOrDepthSlice;
                    rtvDescription.Texture2DArray.MipSlice = mipIndex;
                }
            }
            else
            {
                rtvDescription.Dimension = this.NativeDescription.SampleDescription.Count > 1 ? RenderTargetViewDimension.Texture2DMultisampled : RenderTargetViewDimension.Texture2D;
                if (this.NativeDescription.SampleDescription.Count <= 1)
                    rtvDescription.Texture2D.MipSlice = mipIndex;
            }

            return new RenderTargetView(GraphicsDevice.NativeDevice, NativeResource, rtvDescription);
        }

        protected static TextureDescription ConvertFromNativeDescription(Texture2DDescription  description)
        {
            var desc = new TextureDescription()
            {
                Dimension = TextureDimension.Texture2D,
                Width = description.Width,
                Height = description.Height,
                Depth =  1,
                Level =  (MSAALevel)description.SampleDescription.Count,
                Format = (PixelFormat)description.Format,
                MipLevels = description.MipLevels,
                Usage = (GraphicsResourceUsage)description.Usage,
                ArraySize = description.ArraySize,
                Flags = TextureFlags.None
            };

            if ((description.BindFlags & BindFlags.RenderTarget) !=0)
                desc.Flags |= TextureFlags.RenderTarget;
            if ((description.BindFlags & BindFlags.UnorderedAccess) != 0)
                desc.Flags |= TextureFlags.UnorderedAccess;
            if ((description.BindFlags & BindFlags.DepthStencil) != 0)
                desc.Flags |= TextureFlags.DepthStencil;
            if ((description.BindFlags & BindFlags.ShaderResource) != 0)
                desc.Flags |= TextureFlags.ShaderResource;

            return desc;
        }
 
        protected static Texture2DDescription ConvertToNativeDescription(GraphicsDevice device, TextureDescription description)
        {
            var format = (Format) description.Format;
            var flags = description.Flags;

            // If the texture is going to be bound on the depth stencil, for to use TypeLess format
            if ((flags & TextureFlags.DepthStencil) != 0)
            {
                if (device.Features.Profile < GraphicsProfile.Level_10_0)
                {
                    flags &= ~TextureFlags.ShaderResource;
                }
                else
                {
                    // Determine TypeLess Format and ShaderResourceView Format
                    switch (description.Format)
                    {
                        case PixelFormat.D16_UNorm:
                            format = SharpDX.DXGI.Format.R16_Typeless;
                            break;
                        case PixelFormat.D32_Float:
                            format = SharpDX.DXGI.Format.R32_Typeless;
                            break;
                        case PixelFormat.D24_UNorm_S8_UInt:
                            format = SharpDX.DXGI.Format.R24G8_Typeless;
                            break;
                        case PixelFormat.D32_Float_S8X24_UInt:
                            format = SharpDX.DXGI.Format.R32G8X24_Typeless;
                            break;
                        default:
                            throw new InvalidOperationException(string.Format("Unsupported DepthFormat [{0}] for depth buffer", description.Format));
                    }
                }
            }

            var desc = new Texture2DDescription()
            {
                Width = description.Width,
                Height = description.Height,
                ArraySize = description.ArraySize,
                // TODO calculate appropriate MultiSamples
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = GetBindFlagsFromTextureFlags(flags),
                Format = format,
                MipLevels = description.MipLevels,
                Usage = (ResourceUsage)description.Usage,
                CpuAccessFlags = GetCpuAccessFlagsFromUsage(description.Usage),
                OptionFlags = ResourceOptionFlags.None
            };

            if (description.Dimension == TextureDimension.TextureCube)
                desc.OptionFlags = ResourceOptionFlags.TextureCube;

            return desc;
        }

        #region Helper functions

        /// <summary>
        /// Check and modify if necessary the mipmap levels of the image (Troubles with DXT images whose resolution in less than 4x4 in DX9.x).
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="description">The texture description.</param>
        /// <returns>The updated texture description.</returns>
        private static TextureDescription CheckMipLevels(GraphicsDevice device, ref TextureDescription description)
        {
            if (device.Features.Profile < GraphicsProfile.Level_10_0 && (description.Flags & TextureFlags.DepthStencil) == 0 && description.Format.IsCompressed())
            {
                description.MipLevels = Math.Min(CalculateMipCount(description.Width, description.Height), description.MipLevels);
            }
            return description;
        }

        /// <summary>
        /// Calculates the mip level from a specified size.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="minimumSizeLastMip">The minimum size of the last mip.</param>
        /// <returns>The mip level.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Value must be > 0;size</exception>
        private static int CalculateMipCountFromSize(int size, int minimumSizeLastMip = 4)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("Value must be > 0", "size");
            }

            if (minimumSizeLastMip <= 0)
            {
                throw new ArgumentOutOfRangeException("Value must be > 0", "minimumSizeLastMip");
            }

            int level = 1;
            while ((size / 2) >= minimumSizeLastMip)
            {
                size = Math.Max(1, size / 2);
                level++;
            }
            return level;
        }

        /// <summary>
        /// Calculates the mip level from a specified width,height,depth.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="minimumSizeLastMip">The minimum size of the last mip.</param>
        /// <returns>The mip level.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Value must be &gt; 0;size</exception>
        private static int CalculateMipCount(int width, int height, int minimumSizeLastMip = 4)
        {
            return Math.Min(CalculateMipCountFromSize(width, minimumSizeLastMip), CalculateMipCountFromSize(height, minimumSizeLastMip));
        }

        #endregion 
    }
}
#endif