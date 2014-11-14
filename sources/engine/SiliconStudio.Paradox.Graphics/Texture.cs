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

using System;
using System.IO;
using SiliconStudio.Core;
using SiliconStudio.Core.ReferenceCounting;
using SiliconStudio.Core.Serialization.Contents;
using Utilities = SiliconStudio.Core.Utilities;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Base class for texture resources.
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="N:SharpDX.Direct3D11"/> texture resource.</typeparam>
    [ContentSerializer]
    public abstract partial class Texture : GraphicsResource
    {
        /// <summary>
        /// Common description for the original texture.
        /// </summary>
        public readonly TextureDescription Description;

        /// <summary>
        /// The width of this texture view.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of this texture view.
        /// </summary>
        public int Height;

        /// <summary>
        /// The depth of this texture view.
        /// </summary>
        public readonly int Depth;

        /// <summary>
        /// The format of this texture view.
        /// </summary>
        public readonly PixelFormat ViewFormat;

        /// <summary>
        /// The format of this texture view.
        /// </summary>
        public readonly ViewType ViewType;

        /// <summary>
        /// The miplevel index of this texture view.
        /// </summary>
        public readonly int MipLevel;

        /// <summary>
        /// The array index of this texture view.
        /// </summary>
        public readonly int ArraySlice;
        
        /// <summary>
        /// Gets a boolean indicating whether this <see cref="Texture"/> is a using a block compress format (BC1, BC2, BC3, BC4, BC5, BC6H, BC7).
        /// </summary>
        public readonly bool IsBlockCompressed;

        /// <summary>
        /// The width stride in bytes (number of bytes per row).
        /// </summary>
        internal readonly int RowStride;

        /// <summary>
        /// The depth stride in bytes (number of bytes per depth slice).
        /// </summary>
        internal readonly int DepthStride;

        /// <summary>
        /// The underlying parent texture (if this is a view).
        /// </summary>
        internal readonly Texture ParentTexture;

        private MipMapDescription[] mipmapDescriptions;

        protected Texture()
        {
        }

        protected Texture(GraphicsDevice device, Texture parentTexture, ViewType viewType, int viewArraySlice, int viewMipLevel, PixelFormat viewFormat = PixelFormat.None)
            : this(device, parentTexture.Description, viewType, viewArraySlice, viewMipLevel, viewFormat)
        {
            ParentTexture = parentTexture;
            ParentTexture.AddReferenceInternal();
        }

        protected Texture(GraphicsDevice device, TextureDescription description, ViewType viewType, int viewArraySlice, int viewMipLevel, PixelFormat viewFormat = PixelFormat.None) : base(device)
        {
            Description = description;
            IsBlockCompressed =  description.Format.IsCompressed();
            RowStride = this.Description.Width * description.Format.SizeInBytes();
            DepthStride = RowStride * this.Description.Height;
            mipmapDescriptions = Image.CalculateMipMapDescription(description);

            Width = Math.Max(1, Description.Width >> viewMipLevel);
            Height = Math.Max(1, Description.Height >> viewMipLevel);
            Depth = Math.Max(1, Description.Depth >> viewMipLevel);
            MipLevel = viewMipLevel;
            ViewFormat = viewFormat == PixelFormat.None ? description.Format : viewFormat;
            ArraySlice = viewArraySlice;
            ViewType = viewType;
        }

        protected virtual TextureDescription GetCloneableDescription()
        {
            var description = this.Description;
            if (description.Usage == GraphicsResourceUsage.Immutable)
                description.Usage = GraphicsResourceUsage.Default;
            return description;
        }

        protected override void Destroy()
        {
            base.Destroy();
            if (ParentTexture != null)
            {
                ParentTexture.ReleaseInternal();
            }
        }

        /// <summary>
        /// Gets a view on this texture for a particular <see cref="ViewType"/>, array index (or zIndex for Texture3D), and mipmap index.
        /// </summary>
        /// <param name="viewType">The type of the view to create.</param>
        /// <param name="arrayOrDepthSlice"></param>
        /// <param name="mipMapSlice"></param>
        /// <returns>A new texture object that is bouded to the requested view.</returns>
        public T ToTexture<T>(ViewType viewType, int arrayOrDepthSlice, int mipMapSlice) where T : Texture
        {
            return (T)ToTexture(viewType, arrayOrDepthSlice, mipMapSlice);
        }

        /// <summary>
        /// Gets a view on this texture for a particular <see cref="ViewType"/>, array index (or zIndex for Texture3D), and mipmap index.
        /// </summary>
        /// <param name="viewType">The type of the view to create.</param>
        /// <param name="arraySlice"></param>
        /// <param name="mipMapSlice"></param>
        /// <returns>A new texture object that is bouded to the requested view.</returns>
        public abstract Texture ToTexture(ViewType viewType, int arraySlice, int mipMapSlice);

        public RenderTarget ToRenderTarget()
        {
            return ToRenderTarget(ViewType.Single, 0, 0);
        }

        /// <summary>
        /// Gets the mipmap description of this instance for the specified mipmap level.
        /// </summary>
        /// <param name="mipmap">The mipmap.</param>
        /// <returns>A description of a particular mipmap for this texture.</returns>
        public MipMapDescription GetMipMapDescription(int mipmap)
        {
            return mipmapDescriptions[mipmap];
        }

        public static int CalculateMipSize(int width, int mipLevel)
        {
            mipLevel = Math.Min(mipLevel, Image.CountMips(width));
            width = width >> mipLevel;
            return width > 0 ? width : 1;
        }

        /// <summary>
        /// Gets the absolute sub-resource index from the array and mip slice.
        /// </summary>
        /// <param name="arraySlice">The array slice index.</param>
        /// <param name="mipSlice">The mip slice index.</param>
        /// <returns>A value equals to arraySlice * Description.MipLevels + mipSlice.</returns>
        public int GetSubResourceIndex(int arraySlice, int mipSlice)
        {
            return arraySlice * Description.MipLevels + mipSlice;
        }

        /// <summary>
        /// Calculates the expected width of a texture using a specified type.
        /// </summary>
        /// <typeparam name="TData">The type of the T pixel data.</typeparam>
        /// <returns>The expected width</returns>
        /// <exception cref="System.ArgumentException">If the size is invalid</exception>
        public int CalculateWidth<TData>(int mipLevel = 0) where TData : struct
        {
            var widthOnMip = CalculateMipSize((int)Description.Width, mipLevel);
            var rowStride = widthOnMip * Description.Format.SizeInBytes();

            var dataStrideInBytes = Utilities.SizeOf<TData>() * widthOnMip;
            var width = ((double)rowStride / dataStrideInBytes) * widthOnMip;
            if (Math.Abs(width - (int)width) > Double.Epsilon)
                throw new ArgumentException("sizeof(TData) / sizeof(Format) * Width is not an integer");

            return (int)width;
        }

        /// <summary>
        /// Calculates the number of pixel data this texture is requiring for a particular mip level.
        /// </summary>
        /// <typeparam name="TData">The type of the T pixel data.</typeparam>
        /// <param name="mipLevel">The mip level.</param>
        /// <returns>The number of pixel data.</returns>
        /// <remarks>This method is used to allocated a texture data buffer to hold pixel datas: var textureData = new T[ texture.CalculatePixelCount&lt;T&gt;() ] ;.</remarks>
        public int CalculatePixelDataCount<TData>(int mipLevel = 0) where TData : struct
        {
            return CalculateWidth<TData>(mipLevel) * CalculateMipSize(Description.Height, mipLevel) * CalculateMipSize(Description.Depth, mipLevel);
        }

        /// <summary>
        /// Makes a copy of this texture.
        /// </summary>
        /// <remarks>
        /// This method doesn't copy the content of the texture.
        /// </remarks>
        /// <returns>
        /// A copy of this texture.
        /// </returns>
        public abstract Texture Clone();

        /// <summary>
        /// Makes a copy of this texture with type casting.
        /// </summary>
        /// <remarks>
        /// This method doesn't copy the content of the texture.
        /// </remarks>
        /// <returns>
        /// A copy of this texture.
        /// </returns>
        public T Clone<T>() where T : Texture
        {
            return (T)this.Clone();
        }

        /// <summary>
        /// Gets the content of this texture to an array of data.
        /// </summary>
        /// <typeparam name="TData">The type of the T data.</typeparam>
        /// <param name="arraySlice">The array slice index. This value must be set to 0 for Texture 3D.</param>
        /// <param name="mipSlice">The mip slice index.</param>
        /// <returns>The texture data.</returns>
        /// <remarks>
        /// This method is only working when called from the main thread that is accessing the main <see cref="GraphicsDevice"/>.
        /// This method creates internally a stagging resource, copies to it and map it to memory. Use method with explicit staging resource
        /// for optimal performances.</remarks>
        public TData[] GetData<TData>(int arraySlice = 0, int mipSlice = 0) where TData : struct
        {
            var toData = new TData[this.CalculatePixelDataCount<TData>(mipSlice)];
            GetData(toData, arraySlice, mipSlice);
            return toData;
        }

        /// <summary>
        /// Copies the content of this texture to an array of data.
        /// </summary>
        /// <typeparam name="TData">The type of the T data.</typeparam>
        /// <param name="toData">The destination buffer to receive a copy of the texture datas.</param>
        /// <param name="arraySlice">The array slice index. This value must be set to 0 for Texture 3D.</param>
        /// <param name="mipSlice">The mip slice index.</param>
        /// <param name="doNotWait">if set to <c>true</c> this method will return immediately if the resource is still being used by the GPU for writing. Default is false</param>
        /// <returns><c>true</c> if data was correctly retrieved, <c>false</c> if <see cref="doNotWait"/> flag was true and the resource is still being used by the GPU for writing.</returns>
        /// <remarks>
        /// This method is only working when called from the main thread that is accessing the main <see cref="GraphicsDevice"/>.
        /// This method creates internally a stagging resource if this texture is not already a stagging resouce, copies to it and map it to memory. Use method with explicit staging resource
        /// for optimal performances.</remarks>
        public bool GetData<TData>(TData[] toData, int arraySlice = 0, int mipSlice = 0, bool doNotWait = false) where TData : struct
        {
            // Get data from this resource
            if (Description.Usage == GraphicsResourceUsage.Staging)
            {
                // Directly if this is a staging resource
                return GetData(this, toData, arraySlice, mipSlice, doNotWait);
            }
            else
            {
                // Unefficient way to use the Copy method using dynamic staging texture
                using (var throughStaging = this.ToStaging())
                    return GetData(throughStaging, toData, arraySlice, mipSlice, doNotWait);
            }
        }

        /// <summary>
        /// Copies the content of this texture from GPU memory to an array of data on CPU memory using a specific staging resource.
        /// </summary>
        /// <typeparam name="TData">The type of the T data.</typeparam>
        /// <param name="stagingTexture">The staging texture used to transfer the texture to.</param>
        /// <param name="toData">To data.</param>
        /// <param name="arraySlice">The array slice index. This value must be set to 0 for Texture 3D.</param>
        /// <param name="mipSlice">The mip slice index.</param>
        /// <param name="doNotWait">if set to <c>true</c> this method will return immediately if the resource is still being used by the GPU for writing. Default is false</param>
        /// <returns><c>true</c> if data was correctly retrieved, <c>false</c> if <see cref="doNotWait"/> flag was true and the resource is still being used by the GPU for writing.</returns>
        /// <exception cref="System.ArgumentException">When strides is different from optimal strides, and TData is not the same size as the pixel format, or Width * Height != toData.Length</exception>
        /// <remarks>
        /// This method is only working when called from the main thread that is accessing the main <see cref="GraphicsDevice"/>.
        /// </remarks>
        public unsafe bool GetData<TData>(Texture stagingTexture, TData[] toData, int arraySlice = 0, int mipSlice = 0, bool doNotWait = false) where TData : struct
        {
            return GetData(stagingTexture, new DataPointer((IntPtr)Interop.Fixed(toData), toData.Length * Utilities.SizeOf<TData>()), arraySlice, mipSlice, doNotWait);
        }

        /// <summary>
        /// Copies the content an array of data on CPU memory to this texture into GPU memory.
        /// </summary>
        /// <typeparam name="TData">The type of the T data.</typeparam>
        /// <param name="fromData">The data to copy from.</param>
        /// <param name="arraySlice">The array slice index. This value must be set to 0 for Texture 3D.</param>
        /// <param name="mipSlice">The mip slice index.</param>
        /// <param name="region">Destination region</param>
        /// <exception cref="System.ArgumentException">When strides is different from optimal strides, and TData is not the same size as the pixel format, or Width * Height != toData.Length</exception>
        /// <remarks>
        /// This method is only working on the main graphics device. Use method with explicit graphics device to set data on a deferred context.
        /// See also unmanaged documentation about Map/UnMap for usage and restrictions.
        /// </remarks>
        public void SetData<TData>(TData[] fromData, int arraySlice = 0, int mipSlice = 0, ResourceRegion? region = null) where TData : struct
        {
            SetData(GraphicsDevice, fromData, arraySlice, mipSlice, region);
        }

        /// <summary>
        /// Copies the content an data on CPU memory to this texture into GPU memory using the specified <see cref="GraphicsDevice"/> (The graphics device could be deffered).
        /// </summary>
        /// <param name="fromData">The data to copy from.</param>
        /// <param name="arraySlice">The array slice index. This value must be set to 0 for Texture 3D.</param>
        /// <param name="mipSlice">The mip slice index.</param>
        /// <param name="region">Destination region</param>
        /// <exception cref="System.ArgumentException">When strides is different from optimal strides, and TData is not the same size as the pixel format, or Width * Height != toData.Length</exception>
        /// <remarks>
        /// This method is only working on the main graphics device. Use method with explicit graphics device to set data on a deferred context.
        /// See also unmanaged documentation about Map/UnMap for usage and restrictions.
        /// </remarks>
        public void SetData(DataPointer fromData, int arraySlice = 0, int mipSlice = 0, ResourceRegion? region = null)
        {
            SetData(GraphicsDevice, fromData, arraySlice, mipSlice, region);
        }

        /// <summary>
        /// Copies the content an array of data on CPU memory to this texture into GPU memory using the specified <see cref="GraphicsDevice"/> (The graphics device could be deffered).
        /// </summary>
        /// <typeparam name="TData">The type of the T data.</typeparam>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="fromData">The data to copy from.</param>
        /// <param name="arraySlice">The array slice index. This value must be set to 0 for Texture 3D.</param>
        /// <param name="mipSlice">The mip slice index.</param>
        /// <param name="region">Destination region</param>
        /// <exception cref="System.ArgumentException">When strides is different from optimal strides, and TData is not the same size as the pixel format, or Width * Height != toData.Length</exception>
        /// <remarks>
        /// See unmanaged documentation for usage and restrictions.
        /// </remarks>
        public unsafe void SetData<TData>(GraphicsDevice device, TData[] fromData, int arraySlice = 0, int mipSlice = 0, ResourceRegion? region = null) where TData : struct
        {
            SetData(device, new DataPointer((IntPtr)Interop.Fixed(fromData), fromData.Length * Utilities.SizeOf<TData>()), arraySlice, mipSlice, region);
        }

        /// <summary>
        /// Copies the content of this texture from GPU memory to a pointer on CPU memory using a specific staging resource.
        /// </summary>
        /// <param name="stagingTexture">The staging texture used to transfer the texture to.</param>
        /// <param name="toData">The pointer to data in CPU memory.</param>
        /// <param name="arraySlice">The array slice index. This value must be set to 0 for Texture 3D.</param>
        /// <param name="mipSlice">The mip slice index.</param>
        /// <param name="doNotWait">if set to <c>true</c> this method will return immediately if the resource is still being used by the GPU for writing. Default is false</param>
        /// <returns><c>true</c> if data was correctly retrieved, <c>false</c> if <see cref="doNotWait"/> flag was true and the resource is still being used by the GPU for writing.</returns>
        /// <exception cref="System.ArgumentException">When strides is different from optimal strides, and TData is not the same size as the pixel format, or Width * Height != toData.Length</exception>
        /// <remarks>
        /// This method is only working when called from the main thread that is accessing the main <see cref="GraphicsDevice"/>.
        /// </remarks>
        public unsafe bool GetData(Texture stagingTexture, DataPointer toData, int arraySlice = 0, int mipSlice = 0, bool doNotWait = false)
        {
            if (stagingTexture == null) throw new ArgumentNullException("stagingTexture");
            var device = GraphicsDevice;
            //var deviceContext = device.NativeDeviceContext;

            // Get mipmap description for the specified mipSlice
            var mipmap = this.GetMipMapDescription(mipSlice);

            // Copy height, depth
            int height = mipmap.HeightPacked;
            int depth = mipmap.Depth;

            // Calculate depth stride based on mipmap level
            int rowStride = mipmap.RowStride;

            // Depth Stride
            int textureDepthStride = mipmap.DepthStride;

            // MipMap Stride
            int mipMapSize = mipmap.MipmapSize;

            // Check size validity of data to copy to
            if (toData.Size > mipMapSize)
                throw new ArgumentException(string.Format("Size of toData ({0} bytes) is not compatible expected size ({1} bytes) : Width * Height * Depth * sizeof(PixelFormat) size in bytes", toData.Size, mipMapSize));

            // Copy the actual content of the texture to the staging resource
            if (!ReferenceEquals(this, stagingTexture))
                device.Copy(this, stagingTexture);

            // Calculate the subResourceIndex for a Texture2D
            int subResourceIndex = this.GetSubResourceIndex(arraySlice, mipSlice);

            // Map the staging resource to a CPU accessible memory
            var mappedResource = device.MapSubresource(stagingTexture, subResourceIndex, MapMode.Read, doNotWait);

            // Box can be empty if DoNotWait is set to true, return false if empty
            var box = mappedResource.DataBox;
            if (box.IsEmpty)
            {
                return false;
            }

            // If depth == 1 (Texture1D, Texture2D or TextureCube), then depthStride is not used
            var boxDepthStride = this.Description.Depth == 1 ? box.SlicePitch : textureDepthStride;

            var isFlippedTexture = IsFlippedTexture();

            // The fast way: If same stride, we can directly copy the whole texture in one shot
            if (box.RowPitch == rowStride && boxDepthStride == textureDepthStride && !isFlippedTexture)
            {
                Utilities.CopyMemory(toData.Pointer, box.DataPointer, mipMapSize);
            }
            else
            {
                // Otherwise, the long way by copying each scanline
                var sourcePerDepthPtr = (byte*)box.DataPointer;
                var destPtr = (byte*)toData.Pointer;

                // Iterate on all depths
                for (int j = 0; j < depth; j++)
                {
                    var sourcePtr = sourcePerDepthPtr;
                    // Iterate on each line

                    if (isFlippedTexture)
                    {
                        sourcePtr = sourcePtr + box.RowPitch * (height - 1);
                        for (int i = height - 1; i >= 0; i--)
                        {
                            // Copy a single row
                            Utilities.CopyMemory(new IntPtr(destPtr), new IntPtr(sourcePtr), rowStride);
                            sourcePtr -= box.RowPitch;
                            destPtr += rowStride;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < height; i++)
                        {
                            // Copy a single row
                            Utilities.CopyMemory(new IntPtr(destPtr), new IntPtr(sourcePtr), rowStride);
                            sourcePtr += box.RowPitch;
                            destPtr += rowStride;
                        }
                    }
                    sourcePerDepthPtr += box.SlicePitch;
                }
            }

            // Make sure that we unmap the resource in case of an exception
            device.UnmapSubresource(mappedResource);

            return true;
        }

        /// <summary>
        /// Copies the content an data on CPU memory to this texture into GPU memory.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="fromData">The data to copy from.</param>
        /// <param name="arraySlice">The array slice index. This value must be set to 0 for Texture 3D.</param>
        /// <param name="mipSlice">The mip slice index.</param>
        /// <param name="region">Destination region</param>
        /// <exception cref="System.ArgumentException">When strides is different from optimal strides, and TData is not the same size as the pixel format, or Width * Height != toData.Length</exception>
        /// <remarks>
        /// See unmanaged documentation for usage and restrictions.
        /// </remarks>
        public unsafe void SetData(GraphicsDevice device, DataPointer fromData, int arraySlice = 0, int mipSlice = 0, ResourceRegion? region = null)
        {
            if (device == null) throw new ArgumentNullException("device");
            if (region.HasValue && this.Description.Usage != GraphicsResourceUsage.Default)
                throw new ArgumentException("Region is only supported for textures with ResourceUsage.Default");

            // Get mipmap description for the specified mipSlice
            var mipMapDesc = this.GetMipMapDescription(mipSlice);

            int width = mipMapDesc.Width;
            int height = mipMapDesc.Height;
            int depth = mipMapDesc.Depth;

            // If we are using a region, then check that parameters are fine
            if (region.HasValue)
            {
                int newWidth = region.Value.Right - region.Value.Left;
                int newHeight = region.Value.Bottom - region.Value.Top;
                int newDepth = region.Value.Back - region.Value.Front;
                if (newWidth > width)
                    throw new ArgumentException(string.Format("Region width [{0}] cannot be greater than mipmap width [{1}]", newWidth, width), "region");
                if (newHeight > height)
                    throw new ArgumentException(string.Format("Region height [{0}] cannot be greater than mipmap height [{1}]", newHeight, height), "region");
                if (newDepth > depth)
                    throw new ArgumentException(string.Format("Region depth [{0}] cannot be greater than mipmap depth [{1}]", newDepth, depth), "region");

                width = newWidth;
                height = newHeight;
                depth = newDepth;
            }

            // Size per pixel
            var sizePerElement = Description.Format.SizeInBytes();

            // Calculate depth stride based on mipmap level
            int rowStride;

            // Depth Stride
            int textureDepthStride;

            // Compute Actual pitch
            Image.ComputePitch(this.Description.Format, width, height, out rowStride, out textureDepthStride, out width, out height);

            // Size Of actual texture data
            int sizeOfTextureData = textureDepthStride * depth;

            // Check size validity of data to copy to
            if (fromData.Size != sizeOfTextureData)
                throw new ArgumentException(string.Format("Size of toData ({0} bytes) is not compatible expected size ({1} bytes) : Width * Height * Depth * sizeof(PixelFormat) size in bytes", fromData.Size, sizeOfTextureData));

            // Calculate the subResourceIndex for a Texture
            int subResourceIndex = this.GetSubResourceIndex(arraySlice, mipSlice);

            // If this texture is declared as default usage, we use UpdateSubresource that supports sub resource region.
            if (this.Description.Usage == GraphicsResourceUsage.Default)
            {
                // If using a specific region, we need to handle this case
                if (region.HasValue)
                {
                    var regionValue = region.Value;
                    var sourceDataPtr = fromData.Pointer;

                    // Workaround when using region with a deferred context and a device that does not support CommandList natively
                    // see http://blogs.msdn.com/b/chuckw/archive/2010/07/28/known-issue-direct3d-11-updatesubresource-and-deferred-contexts.aspx
                    if (device.NeedWorkAroundForUpdateSubResource)
                    {
                        if (IsBlockCompressed)
                        {
                            regionValue.Left /= 4;
                            regionValue.Right /= 4;
                            regionValue.Top /= 4;
                            regionValue.Bottom /= 4;
                        }
                        sourceDataPtr = new IntPtr((byte*)sourceDataPtr - (regionValue.Front * textureDepthStride) - (regionValue.Top * rowStride) - (regionValue.Left * sizePerElement));
                    }
                    device.UpdateSubresource(this, subResourceIndex, new DataBox(sourceDataPtr, rowStride, textureDepthStride), regionValue);
                }
                else
                {
                    device.UpdateSubresource(this, subResourceIndex, new DataBox(fromData.Pointer, rowStride, textureDepthStride));
                }
            }
            else
            {
                var mappedResource = device.MapSubresource(this, subResourceIndex, this.Description.Usage == GraphicsResourceUsage.Dynamic ? MapMode.WriteDiscard : MapMode.Write);
                var box = mappedResource.DataBox;

                // If depth == 1 (Texture1D, Texture2D or TextureCube), then depthStride is not used
                var boxDepthStride = this.Description.Depth == 1 ? box.SlicePitch : textureDepthStride;

                // The fast way: If same stride, we can directly copy the whole texture in one shot
                if (box.RowPitch == rowStride && boxDepthStride == textureDepthStride)
                {
                    Utilities.CopyMemory(box.DataPointer, fromData.Pointer, sizeOfTextureData);
                }
                else
                {
                    // Otherwise, the long way by copying each scanline
                    var destPerDepthPtr = (byte*)box.DataPointer;
                    var sourcePtr = (byte*)fromData.Pointer;

                    // Iterate on all depths
                    for (int j = 0; j < depth; j++)
                    {
                        var destPtr = destPerDepthPtr;
                        // Iterate on each line
                        for (int i = 0; i < height; i++)
                        {
                            Utilities.CopyMemory((IntPtr)destPtr, (IntPtr)sourcePtr, rowStride);
                            destPtr += box.RowPitch;
                            sourcePtr += rowStride;
                        }
                        destPerDepthPtr += box.SlicePitch;
                    }

                }
                device.UnmapSubresource(mappedResource);
            }
        }

        /// <summary>
        /// Return an equivalent staging texture CPU read-writable from this instance.
        /// </summary>
        /// <returns></returns>
        public abstract Texture ToStaging();

        /// <summary>
        /// Loads a texture from a stream.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="stream">The stream to load the texture from.</param>
        /// <param name="textureFlags">True to load the texture with unordered access enabled. Default is false.</param>
        /// <param name="usage">Usage of the resource. Default is <see cref="GraphicsResourceUsage.Immutable"/> </param>
        /// <returns>A texture</returns>
        public static Texture Load(GraphicsDevice device, Stream stream, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable)
        {
            using (var image = Image.Load(stream))
                return New(device, image, textureFlags, usage);
        }

        /// <summary>
        /// Loads a texture from a stream.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice" />.</param>
        /// <param name="image">The image.</param>
        /// <param name="textureFlags">True to load the texture with unordered access enabled. Default is false.</param>
        /// <param name="usage">Usage of the resource. Default is <see cref="GraphicsResourceUsage.Immutable" /></param>
        /// <returns>A texture</returns>
        /// <exception cref="System.InvalidOperationException">Dimension not supported</exception>
        public static Texture New(GraphicsDevice device, Image image, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable)
        {
            if (device == null) throw new ArgumentNullException("device");
            if (image == null) throw new ArgumentNullException("image");
            switch (image.Description.Dimension)
            {
                case TextureDimension.Texture1D:
                    return Texture1D.New(device, image, textureFlags, usage);
                case TextureDimension.Texture2D:
                    return Texture2D.New(device, image, textureFlags, usage);
                case TextureDimension.Texture3D:
                    return Texture3D.New(device, image, textureFlags, usage);
                case TextureDimension.TextureCube:
                    return TextureCube.New(device, image, textureFlags, usage);
            }
            
            throw new InvalidOperationException("Dimension not supported");
        }

        /// <summary>
        /// Creates a new texture with the specified generic texture description.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="description">The description.</param>
        /// <returns>A Texture instance, either a RenderTarget or DepthStencilBuffer or Texture, depending on Binding flags.</returns>
        public static Texture New(GraphicsDevice graphicsDevice, TextureDescription description)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            switch (description.Dimension)
            {
                case TextureDimension.Texture1D:
                    return new Texture1D(graphicsDevice, description);
                case TextureDimension.Texture2D:
                    return new Texture2D(graphicsDevice, description);
                case TextureDimension.Texture3D:
                    return new Texture3D(graphicsDevice, description);
                case TextureDimension.TextureCube:
                    return new TextureCube(graphicsDevice, description);
            }
            return null;
        }

        /// <summary>
        /// Saves this texture to a stream with a specified format.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="fileType">Type of the image file.</param>
        public void Save(Stream stream, ImageFileType fileType)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            using (var staging = ToStaging())
                Save(stream, staging, fileType);
        }

        /// <summary>
        /// Gets the GPU content of this texture as an <see cref="Image"/> on the CPU.
        /// </summary>
        public Image GetDataAsImage()
        {
            using (var stagingTexture = ToStaging())
                return GetDataAsImage(stagingTexture);
        }

        /// <summary>
        /// Gets the GPU content of this texture to an <see cref="Image"/> on the CPU.
        /// </summary>
        /// <param name="stagingTexture">The staging texture used to temporary transfer the image from the GPU to CPU.</param>
        /// <exception cref="ArgumentException">If stagingTexture is not a staging texture.</exception>
        public Image GetDataAsImage(Texture stagingTexture)
        {
            if (stagingTexture == null) throw new ArgumentNullException("stagingTexture");
            if (stagingTexture.Description.Usage != GraphicsResourceUsage.Staging)
                throw new ArgumentException("Invalid texture used as staging. Must have Usage = GraphicsResourceUsage.Staging", "stagingTexture");

            var image = Image.New(stagingTexture.Description);
            try {
                for (int arrayIndex = 0; arrayIndex < image.Description.ArraySize; arrayIndex++)
                {
                    for (int mipLevel = 0; mipLevel < image.Description.MipLevels; mipLevel++)
                    {
                        var pixelBuffer = image.PixelBuffer[arrayIndex, mipLevel];
                        GetData(stagingTexture, new DataPointer(pixelBuffer.DataPointer, pixelBuffer.BufferStride), arrayIndex, mipLevel);
                    }
                }

            } catch (Exception)
            {
                // If there was an exception, free the allocated image to avoid any memory leak.
                image.Dispose();
                throw;
            }
            return image;
        }

        /// <summary>
        /// Saves this texture to a stream with a specified format.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="stagingTexture">The staging texture used to temporary transfer the image from the GPU to CPU.</param>
        /// <param name="fileType">Type of the image file.</param>
        /// <exception cref="ArgumentException">If stagingTexture is not a staging texture.</exception>
        public void Save(Stream stream, Texture stagingTexture, ImageFileType fileType)
        {
            using (var image = GetDataAsImage(stagingTexture))
                image.Save(stream, fileType);
        }

        /// <summary>
        /// Calculates the mip map count from a requested level.
        /// </summary>
        /// <param name="requestedLevel">The requested level.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="depth">The depth.</param>
        /// <returns>The resulting mipmap count (clamp to [1, maxMipMapCount] for this texture)</returns>
        internal static int CalculateMipMapCount(MipMapCount requestedLevel, int width, int height = 0, int depth = 0)
        {
            int size = Math.Max(Math.Max(width, height), depth);
            int maxMipMap = 1 + (int)Math.Ceiling(Math.Log(size) / Math.Log(2.0));

            return requestedLevel  == 0 ? maxMipMap : Math.Min(requestedLevel, maxMipMap);
        }

        protected static DataBox GetDataBox<T>(PixelFormat format, int width, int height, int depth, T[] textureData, IntPtr fixedPointer) where T : struct
        {
            // Check that the textureData size is correct
            if (textureData == null) throw new ArgumentNullException("textureData");
            int rowPitch;
            int slicePitch;
            int widthCount;
            int heightCount;
            Image.ComputePitch(format, width, height, out rowPitch, out slicePitch, out widthCount, out heightCount);
            if (Utilities.SizeOf(textureData) != (slicePitch * depth)) throw new ArgumentException("Invalid size for Image");

            return new DataBox(fixedPointer, rowPitch, slicePitch);
        }

        internal static TextureDescription CreateTextureDescriptionFromImage(Image image, TextureFlags textureFlags, GraphicsResourceUsage usage)
        {
            var desc = (TextureDescription)image.Description;
            desc.Flags = textureFlags;
            desc.Usage = usage;
            if ((textureFlags & TextureFlags.UnorderedAccess) != 0)
                desc.Usage = GraphicsResourceUsage.Default;
            return desc;
        }

        internal void GetViewSliceBounds(ViewType viewType, ref int arrayOrDepthIndex, ref int mipIndex, out int arrayOrDepthCount, out int mipCount)
        {
            int arrayOrDepthSize = this.Description.Depth > 1 ? this.Description.Depth : this.Description.ArraySize;

            switch (viewType)
            {
                case ViewType.Full:
                    arrayOrDepthIndex = 0;
                    mipIndex = 0;
                    arrayOrDepthCount = arrayOrDepthSize;
                    mipCount = this.Description.MipLevels;
                    break;
                case ViewType.Single:
                    arrayOrDepthCount = 1;
                    mipCount = 1;
                    break;
                case ViewType.MipBand:
                    arrayOrDepthCount = arrayOrDepthSize - arrayOrDepthIndex;
                    mipCount = 1;
                    break;
                case ViewType.ArrayBand:
                    arrayOrDepthCount = 1;
                    mipCount = Description.MipLevels - mipIndex;
                    break;
                default:
                    arrayOrDepthCount = 0;
                    mipCount = 0;
                    break;
            }
        }

        internal int GetViewCount()
        {
            int arrayOrDepthSize = this.Description.Depth > 1 ? this.Description.Depth : this.Description.ArraySize;
            return GetViewIndex((ViewType)4, arrayOrDepthSize, this.Description.MipLevels);
        }

        internal int GetViewIndex(ViewType viewType, int arrayOrDepthIndex, int mipIndex)
        {
            int arrayOrDepthSize = this.Description.Depth > 1 ? this.Description.Depth : this.Description.ArraySize;
            return (((int)viewType) * arrayOrDepthSize + arrayOrDepthIndex) * this.Description.MipLevels + mipIndex;
        }

        internal static GraphicsResourceUsage GetUsageWithFlags(GraphicsResourceUsage usage, TextureFlags flags)
        {
            // If we have a texture supporting render target or unordered access, force to UsageDefault
            if ((flags & TextureFlags.RenderTarget) != 0 || (flags & TextureFlags.UnorderedAccess) != 0)
                return GraphicsResourceUsage.Default;
            return usage;
        }
    }
}