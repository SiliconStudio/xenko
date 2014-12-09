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
using SharpDX.Direct3D11;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
{
    public abstract partial class Texture
    {
        internal RenderTarget ToRenderTarget(ViewType viewType, int arraySlize, int mipSlice)
        {
            return new RenderTarget(GraphicsDevice, this, viewType, arraySlize, mipSlice);            
        }

        /// <summary>
        /// Gets a specific <see cref="ShaderResourceView" /> from this texture.
        /// </summary>
        /// <param name="viewType">Type of the view slice.</param>
        /// <param name="arrayOrDepthSlice">The texture array slice index.</param>
        /// <param name="mipIndex">The mip map slice index.</param>
        /// <returns>An <see cref="ShaderResourceView" /></returns>
        internal abstract ShaderResourceView GetShaderResourceView(ViewType viewType, int arrayOrDepthSlice, int mipIndex);

        /// <summary>
        /// Gets a specific <see cref="RenderTargetView" /> from this texture.
        /// </summary>
        /// <param name="viewType">Type of the view slice.</param>
        /// <param name="arrayOrDepthSlice">The texture array slice index.</param>
        /// <param name="mipMapSlice">The mip map slice index.</param>
        /// <returns>An <see cref="RenderTargetView" /></returns>
        internal abstract RenderTargetView GetRenderTargetView(ViewType viewType, int arrayOrDepthSlice, int mipMapSlice);

        /// <summary>
        /// Gets a specific <see cref="UnorderedAccessView"/> from this texture.
        /// </summary>
        /// <param name="arrayOrDepthSlice">The texture array slice index.</param>
        /// <param name="mipMapSlice">The mip map slice index.</param>
        /// <returns>An <see cref="UnorderedAccessView"/></returns>
        internal abstract UnorderedAccessView GetUnorderedAccessView(int arrayOrDepthSlice, int mipMapSlice);

        public virtual void Recreate(DataBox[] dataBoxes = null)
        {
            throw new NotImplementedException();
        }

        protected override void DestroyImpl()
        {
            // If it was a View, do not release reference
            if (ParentTexture != null)
            {
                _nativeDeviceChild = null;
                NativeResource = null;
            }

            base.DestroyImpl();
        }
        
        internal static SharpDX.Direct3D11.BindFlags GetBindFlagsFromTextureFlags(TextureFlags flags)
        {
            var result = BindFlags.None;
            if ((flags & TextureFlags.ShaderResource) != 0)
                result |= BindFlags.ShaderResource;
            if ((flags & TextureFlags.RenderTarget) != 0)
                result |= BindFlags.RenderTarget;
            if ((flags & TextureFlags.UnorderedAccess) != 0)
                result |= BindFlags.UnorderedAccess;
            if ((flags & TextureFlags.DepthStencil) != 0)
                result |= BindFlags.DepthStencil;

            return result;
        }

        internal unsafe static SharpDX.DataBox[] ConvertDataBoxes(DataBox[] dataBoxes)
        {
            if (dataBoxes == null)
                return null;

            var sharpDXDataBoxes = new SharpDX.DataBox[dataBoxes.Length];
            fixed (void* pDataBoxes = sharpDXDataBoxes)
                Utilities.Write((IntPtr)pDataBoxes, dataBoxes, 0, dataBoxes.Length);

            return sharpDXDataBoxes;
        }

        private bool IsFlippedTexture()
        {
            return false;
        }
    }
}
#endif