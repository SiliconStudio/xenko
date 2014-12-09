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
using System.IO;

using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.IO;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A Texture 3D frontend to <see cref="SharpDX.Direct3D11.Texture3D"/>.
    /// </summary>
    public partial class Texture3D
    {
        protected readonly SharpDX.Direct3D11.Texture3D Resource;
        private SharpDX.DXGI.Surface dxgiSurface;
        protected internal readonly Texture3DDescription NativeDescription;

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture3DBase" /> class.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="description3D">The description.</param>
        /// <param name="dataRectangles">A variable-length parameters list containing data rectangles.</param>
        protected internal Texture3D(GraphicsDevice device, TextureDescription description3D, DataBox[] dataBoxes = null) : base(device, description3D, ViewType.Full, 0, 0)
        {
            NativeDescription = ConvertToNativeDescription(description3D);
            Resource = new SharpDX.Direct3D11.Texture3D(device.NativeDevice, NativeDescription, ConvertDataBoxes(dataBoxes));
            NativeDeviceChild = Resource;
            NativeShaderResourceView = GetShaderResourceView(ViewType, ArraySlice, MipLevel);
            NativeUnorderedAccessView = GetUnorderedAccessView(ArraySlice, MipLevel);
        }

        /// <summary>
        /// Specialised constructor for use only by derived classes.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="texture">The texture.</param>
        protected internal Texture3D(GraphicsDevice device, Texture3D texture, ViewType viewType, int arraySlice, int mipMapSlice, PixelFormat viewFormat = PixelFormat.None) : base(device, texture, viewType, arraySlice, mipMapSlice, viewFormat)
        {
            // Copy the device child, but don't use NativeDeviceChild, as it is registering it for disposing.
            _nativeDeviceChild = texture._nativeDeviceChild;
            Resource = texture.Resource;
            NativeDescription = texture.NativeDescription;
            dxgiSurface = texture.dxgiSurface;
            NativeShaderResourceView = GetShaderResourceView(ViewType, ArraySlice, MipLevel);
            NativeUnorderedAccessView = GetUnorderedAccessView(ArraySlice, MipLevel);
        }

        public override Texture CreateTextureView(ViewType viewType, int arraySlice, int mipMapSlice)
        {
            return new Texture3D(GraphicsDevice, this, viewType, arraySlice, mipMapSlice);
        }

        internal override ShaderResourceView GetShaderResourceView(ViewType viewType, int arrayOrDepthSlice, int mipIndex)
        {
            if ((this.NativeDescription.BindFlags & BindFlags.ShaderResource) == 0)
                return null;

            int arrayCount;
            int mipCount;
            GetViewSliceBounds(viewType, ref arrayOrDepthSlice, ref mipIndex, out arrayCount, out mipCount);

            // Create the view
            var srvDescription = new ShaderResourceViewDescription {
                Format = (Format)this.Description.Format,
                Dimension = ShaderResourceViewDimension.Texture3D,
                Texture3D = {
                    MipLevels = mipCount,
                    MostDetailedMip = mipIndex
                }
            };

            return new ShaderResourceView(this.GraphicsDevice.NativeDevice, this.Resource, srvDescription);
        }

        internal override UnorderedAccessView GetUnorderedAccessView(int zSlice, int mipIndex)
        {
            if ((this.NativeDescription.BindFlags & BindFlags.UnorderedAccess) == 0)
                return null;

            int sliceCount;
            int mipCount;
            GetViewSliceBounds(ViewType.Single, ref zSlice, ref mipIndex, out sliceCount, out mipCount);

            var uavIndex = GetViewIndex(ViewType.Single, zSlice, mipIndex);

            var uavDescription = new UnorderedAccessViewDescription() {
                Format = (Format)this.Description.Format,
                Dimension = UnorderedAccessViewDimension.Texture3D,
                Texture3D = {
                    FirstWSlice = zSlice,
                    MipSlice = mipIndex,
                    WSize = sliceCount
                }
            };

            return new UnorderedAccessView(GraphicsDevice.NativeDevice, Resource, uavDescription);
        }

        protected static Texture3DDescription ConvertToNativeDescription(TextureDescription description)
        {
            var desc = new Texture3DDescription()
                           {
                               Width = description.Width,
                               Height = description.Height,
                               Depth = description.Depth,
                               BindFlags = GetBindFlagsFromTextureFlags(description.Flags),
                               Format = (Format)description.Format,
                               MipLevels = description.MipLevels,
                               Usage = (ResourceUsage)description.Usage,
                               CpuAccessFlags = GetCpuAccessFlagsFromUsage(description.Usage),
                               OptionFlags = ResourceOptionFlags.None
                           };
            return desc;
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
            var rtvDescription = new RenderTargetViewDescription()
            {
                Format = this.NativeDescription.Format,
                Dimension = RenderTargetViewDimension.Texture3D,
                Texture3D =
                {
                    DepthSliceCount = arrayCount,
                    FirstDepthSlice = arrayOrDepthSlice,
                    MipSlice = mipIndex,
                }
            };

            return new RenderTargetView(GraphicsDevice.NativeDevice, Resource, rtvDescription);
        }
    }
}
#endif