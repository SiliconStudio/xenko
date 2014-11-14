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

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A Texture 2D frontend to <see cref="SharpDX.Direct3D11.Texture2D"/>.
    /// </summary>
    public partial class Texture2D
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2DBase" /> class.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="description2D">The description.</param>
        /// <param name="dataBoxes">A variable-length parameters list containing data rectangles.</param>
        protected internal Texture2D(GraphicsDevice device, TextureDescription description2D, DataBox[] dataBoxes = null) : base(device, description2D, dataBoxes)
        {
        }

        /// <summary>
        /// Specialised constructor for use only by derived classes.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice" />.</param>
        /// <param name="texture">The texture.</param>
        /// <param name="viewType">Type of the view.</param>
        /// <param name="arraySlice">The array slice.</param>
        /// <param name="mipMapSlice">The mip map slice.</param>
        /// <param name="viewFormat">The view format.</param>
        protected internal Texture2D(GraphicsDevice device, Texture2D texture, ViewType viewType = ViewType.Full, int arraySlice = 0, int mipMapSlice = 0, PixelFormat viewFormat = PixelFormat.None)
            : base(device, texture, viewType, arraySlice, mipMapSlice, viewFormat)
        {
        }

        /// <summary>
        /// Specialised constructor for use only by derived classes.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="texture">The texture.</param>
        protected internal Texture2D(GraphicsDevice device, SharpDX.Direct3D11.Texture2D texture)
            : base(device, texture)
        {
        }

        public override Texture ToTexture(ViewType viewType, int arraySlice, int mipMapSlice)
        {
            return new Texture2D(GraphicsDevice, this, viewType, arraySlice, mipMapSlice);
        }

        /// <summary>
        /// Gets a new instance of a depth stencil buffer linked to this texture.
        /// </summary>
        /// <param name="isReadOnly">if set to <c>true</c> the returned depth stencil buffer view is a read-only view, false otherwise..</param>
        /// <returns>A depth stencil buffer view.</returns>
        public DepthStencilBuffer ToDepthStencilBuffer(bool isReadOnly)
        {
            return new DepthStencilBuffer(GraphicsDevice, this, isReadOnly);
        }

        /// <summary>
        /// Creates a texture that can be used as a ShaderResource from an existing depth texture.
        /// </summary>
        /// <returns></returns>
        public Texture2D ToDepthTextureCompatible()
        {
            if ((Description.Flags & TextureFlags.DepthStencil) == 0)
                throw new NotSupportedException("This texture is not a valid depth stencil texture");

            var description = Description;
            description.Format = (PixelFormat)DepthStencilBuffer.ComputeShaderResourceFormat((Format)Description.Format);
            if (description.Format == PixelFormat.None)
                throw new NotSupportedException("This depth stencil format is not supported");

            description.Flags = TextureFlags.ShaderResource;
            return New(GraphicsDevice, description);
        }
    }
}
#endif