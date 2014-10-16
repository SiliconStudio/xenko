// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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

using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A Texture 1D frontend to <see cref="SharpDX.Direct3D11.Texture1D"/>.
    /// </summary>
    public partial class Texture1D 
    {
        protected internal readonly SharpDX.Direct3D11.Texture1D Resource;
        protected internal SharpDX.DXGI.Surface dxgiSurface;
        protected internal readonly Texture1DDescription NativeDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1DBase" /> class.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="description1D">The description.</param>
        protected internal Texture1D(GraphicsDevice device, TextureDescription description1D, DataBox[] dataBox = null) : base(device, description1D, ViewType.Full, 0, 0)
        {
            NativeDescription = ConvertToNativeDescription(description1D);
            Resource = new SharpDX.Direct3D11.Texture1D(device.NativeDevice, NativeDescription, ConvertDataBoxes(dataBox));
            NativeDeviceChild = Resource;
            NativeShaderResourceView = GetShaderResourceView(ViewType, ArraySlice, MipLevel);
            NativeUnorderedAccessView = GetUnorderedAccessView(ArraySlice, MipLevel);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture1DBase" /> class.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="description1D">The description.</param>
        protected internal Texture1D(GraphicsDevice device, Texture1D texture, ViewType viewType, int arraySlice, int mipMapSlice, PixelFormat viewFormat = PixelFormat.None) : base(device, texture, viewType, arraySlice, mipMapSlice, viewFormat)
        {
            // Copy the device child, but don't use NativeDeviceChild, as it is registering it for disposing.
            _nativeDeviceChild = texture._nativeDeviceChild;
            Resource = texture.Resource;
            NativeDescription = texture.NativeDescription;
            dxgiSurface = texture.dxgiSurface;
            NativeShaderResourceView = GetShaderResourceView(ViewType, ArraySlice, MipLevel);
            NativeUnorderedAccessView = GetUnorderedAccessView(ArraySlice, MipLevel);
        }

        public override Texture ToTexture(ViewType viewType, int arraySlice, int mipMapSlice)
        {
            return new Texture1D(GraphicsDevice, this, viewType, arraySlice, mipMapSlice);
        }

        internal override ShaderResourceView GetShaderResourceView(ViewType viewType, int arrayOrDepthSlice, int mipIndex)
        {
            if ((this.NativeDescription.BindFlags & BindFlags.ShaderResource) == 0)
                return null;

            int arrayCount;
            int mipCount;
            GetViewSliceBounds(viewType, ref arrayOrDepthSlice, ref mipIndex, out arrayCount, out mipCount);

            // Create the view
            var srvDescription = new ShaderResourceViewDescription() { Format = (Format)this.Description.Format };

            // Initialize for texture arrays or texture cube
            if (this.Description.ArraySize > 1)
            {
                // Else regular Texture1D
                srvDescription.Dimension = ShaderResourceViewDimension.Texture1DArray;
                srvDescription.Texture1DArray.ArraySize = arrayCount;
                srvDescription.Texture1DArray.FirstArraySlice = arrayOrDepthSlice;
                srvDescription.Texture1DArray.MipLevels = mipCount;
                srvDescription.Texture1DArray.MostDetailedMip = mipIndex;
            }
            else
            {
                srvDescription.Dimension = ShaderResourceViewDimension.Texture1D;
                srvDescription.Texture1D.MipLevels = mipCount;
                srvDescription.Texture1D.MostDetailedMip = mipIndex;
            }

            return new ShaderResourceView(this.GraphicsDevice.NativeDevice, this.Resource, srvDescription);
        }

        internal override UnorderedAccessView GetUnorderedAccessView(int arrayOrDepthSlice, int mipIndex)
        {
            if ((this.NativeDescription.BindFlags & BindFlags.UnorderedAccess) == 0)
                return null;

            int arrayCount;
            int mipCount;
            GetViewSliceBounds(ViewType.Single, ref arrayOrDepthSlice, ref mipIndex, out arrayCount, out mipCount);

            var uavDescription = new UnorderedAccessViewDescription() {
                Format = (Format)this.Description.Format,
                Dimension = this.Description.ArraySize > 1 ? UnorderedAccessViewDimension.Texture1DArray : UnorderedAccessViewDimension.Texture1D
            };

            if (this.Description.ArraySize > 1)
            {
                uavDescription.Texture1DArray.ArraySize = arrayCount;
                uavDescription.Texture1DArray.FirstArraySlice = arrayOrDepthSlice;
                uavDescription.Texture1DArray.MipSlice = mipIndex;
            }
            else
            {
                uavDescription.Texture1D.MipSlice = mipIndex;
            }

            return new UnorderedAccessView(GraphicsDevice.NativeDevice, Resource, uavDescription);
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
                rtvDescription.Dimension = RenderTargetViewDimension.Texture1DArray;
                rtvDescription.Texture1DArray.ArraySize = arrayCount;
                rtvDescription.Texture1DArray.FirstArraySlice = arrayOrDepthSlice;
                rtvDescription.Texture1DArray.MipSlice = mipIndex;
            }
            else
            {
                rtvDescription.Dimension = RenderTargetViewDimension.Texture1D;
                rtvDescription.Texture1D.MipSlice = mipIndex;
            }

            return new RenderTargetView(GraphicsDevice.NativeDevice, Resource, rtvDescription);
        }

        protected static Texture1DDescription ConvertToNativeDescription(TextureDescription description)
        {
            var desc = new Texture1DDescription()
            {
                Width = description.Width,
                ArraySize = 1,
                BindFlags = GetBindFlagsFromTextureFlags(description.Flags),
                Format = (Format)description.Format,
                MipLevels = description.MipLevels,
                Usage = (ResourceUsage)description.Usage,
                CpuAccessFlags = GetCpuAccessFlagsFromUsage(description.Usage),
                OptionFlags = ResourceOptionFlags.None
            };
            return desc;
        }
    }
}
#endif