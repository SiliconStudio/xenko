// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
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
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_DIRECT3D 
using System;
using System.IO;

using SharpDX.Direct3D11;
using SharpDX.IO;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A TextureCube frontend to <see cref="SharpDX.Direct3D11.Texture2D"/>.
    /// </summary>
    public partial class TextureCube
    {
        internal TextureCube(GraphicsDevice device, TextureDescription description2D, params DataBox[] dataBoxes) : base(device, description2D, dataBoxes)
        {
        }

        internal TextureCube(GraphicsDevice device, TextureDescription description2D) : base(device, description2D, null)
        {
        }

        internal TextureCube(GraphicsDevice device, TextureCube texture, ViewType viewType = ViewType.Full, int arraySlice = 0, int mipMapSlice = 0, PixelFormat viewFormat = PixelFormat.None)
            : base(device, texture, viewType, arraySlice, mipMapSlice, viewFormat)
        {
        }

        public override Texture CreateTextureView(ViewType viewType, int arraySlice, int mipMapSlice)
        {
            return new TextureCube(GraphicsDevice, this, viewType, arraySlice, mipMapSlice);
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
            var rtvDescription = new RenderTargetViewDescription
            {
                Format = this.NativeDescription.Format,
                Dimension = RenderTargetViewDimension.Texture2DArray,
                Texture2DArray =
                {
                    ArraySize = arrayCount,
                    FirstArraySlice = arrayOrDepthSlice,
                    MipSlice = mipIndex
                }
            };

            return new RenderTargetView(GraphicsDevice.NativeDevice, NativeResource, rtvDescription);
        }
    }
}
#endif