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

using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A Texture 1D frontend to <see cref="SharpDX.Direct3D11.Texture1D"/>.
    /// </summary>
    [DataConverter(AutoGenerate = false, ContentReference = true, DataType = false)]
    public partial class Texture1D : Texture
    {
        /// <summary>
        /// Makes a copy of this texture.
        /// </summary>
        /// <remarks>
        /// This method doesn't copy the content of the texture.
        /// </remarks>
        /// <returns>
        /// A copy of this texture.
        /// </returns>
        public override Texture Clone()
        {
            return new Texture1D(GraphicsDevice, GetCloneableDescription());
        }

        /// <summary>
        /// Return an equivalent staging texture CPU read-writable from this instance.
        /// </summary>
        /// <returns></returns>
        public override Texture ToStaging()
        {
            return new Texture1D(this.GraphicsDevice, this.Description.ToStagingDescription());
        }

        /// <summary>
        /// Creates a new <see cref="Texture1D"/> with a single mipmap.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="width">The width.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="textureFlags">true if the texture needs to support unordered read write.</param>
        /// <param name="arraySize">Size of the texture 2D array, default to 1.</param>
        /// <returns>
        /// A new instance of <see cref="Texture1D"/> class.
        /// </returns>
        public static Texture1D New(GraphicsDevice device, int width, PixelFormat format, TextureFlags textureFlags = TextureFlags.ShaderResource, int arraySize = 1, GraphicsResourceUsage usage = GraphicsResourceUsage.Default)
        {
            return New(device, width, false, format, textureFlags, arraySize, usage);
        }

        /// <summary>
        /// Creates a new <see cref="Texture1D"/>.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="width">The width.</param>
        /// <param name="mipCount">Number of mipmaps, set to true to have all mipmaps, set to an int >=1 for a particular mipmap count.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="textureFlags">true if the texture needs to support unordered read write.</param>
        /// <param name="arraySize">Size of the texture 2D array, default to 1.</param>
        /// <returns>
        /// A new instance of <see cref="Texture1D"/> class.
        /// </returns>
        public static Texture1D New(GraphicsDevice device, int width, MipMapCount mipCount, PixelFormat format, TextureFlags textureFlags = TextureFlags.ShaderResource, int arraySize = 1, GraphicsResourceUsage usage = GraphicsResourceUsage.Default)
        {
            return new Texture1D(device, NewDescription(width, format, textureFlags, mipCount, arraySize, usage));
        }

        /// <summary>
        /// Creates a new <see cref="Texture1D" /> with a single level of mipmap.
        /// </summary>
        /// <typeparam name="T">Type of the initial data to upload to the texture</typeparam>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="width">The width.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="textureData">Texture data. Size of must be equal to sizeof(Format) * width </param>
        /// <param name="textureFlags">true if the texture needs to support unordered read write.</param>
        /// <returns>A new instance of <see cref="Texture1D" /> class.</returns>
        /// <remarks>
        /// The first dimension of mipMapTextures describes the number of array (Texture1D Array), second dimension is the mipmap, the third is the texture data for a particular mipmap.
        /// </remarks>
        public unsafe static Texture1D New<T>(GraphicsDevice device, int width, PixelFormat format, T[] textureData, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable) where T : struct
        {
            return new Texture1D(device, NewDescription(width, format, textureFlags, 1, 1, usage), new[] {GetDataBox(format, width, 1, 1, textureData, (IntPtr)Interop.Fixed(textureData))});
        }

        /// <summary>
        /// Creates a new <see cref="Texture1D" /> with a single level of mipmap.
        /// </summary>
        /// <typeparam name="T">Type of the initial data to upload to the texture</typeparam>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="width">The width.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="textureData">Texture data. Size of must be equal to sizeof(Format) * width </param>
        /// <param name="dataPtr">Data ptr</param>
        /// <param name="textureFlags">true if the texture needs to support unordered read write.</param>
        /// <returns>A new instance of <see cref="Texture1D" /> class.</returns>
        /// <remarks>
        /// The first dimension of mipMapTextures describes the number of array (Texture1D Array), second dimension is the mipmap, the third is the texture data for a particular mipmap.
        /// </remarks>
        public unsafe static Texture1D New(GraphicsDevice device, int width, PixelFormat format, IntPtr dataPtr, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable)
        {
            return new Texture1D(device, NewDescription(width, format, textureFlags, 1, 1, usage), new [] { new DataBox(dataPtr, 0, 0), });
        }
        /// <summary>
        /// Creates a new <see cref="Texture1D" /> directly from an <see cref="Image"/>.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="image">An image in CPU memory.</param>
        /// <param name="textureFlags">true if the texture needs to support unordered read write.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>A new instance of <see cref="Texture1D" /> class.</returns>
        public static new Texture1D New(GraphicsDevice device, Image image, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (image.Description.Dimension != TextureDimension.Texture1D)
                throw new ArgumentException("Invalid image. Must be 1D", "image");

            return new Texture1D(device, CreateTextureDescriptionFromImage(image, textureFlags, usage), image.ToDataBox());
        }

        /// <summary>
        /// Loads a 1D texture from a stream.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="stream">The stream to load the texture from.</param>
        /// <param name="textureFlags">True to load the texture with unordered access enabled. Default is false.</param>
        /// <param name="usage">Usage of the resource. Default is <see cref="GraphicsResourceUsage.Immutable"/> </param>
        /// <exception cref="ArgumentException">If the texture is not of type 1D</exception>
        /// <returns>A texture</returns>
        public static new Texture1D Load(GraphicsDevice device, Stream stream, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable)
        {
            var texture = Texture.Load(device, stream, textureFlags, usage);
            if (!(texture is Texture1D))
                throw new ArgumentException(string.Format("Texture is not type of [Texture1D] but [{0}]", texture.GetType().Name));
            return (Texture1D)texture;
        }

        protected static TextureDescription NewDescription(int width, PixelFormat format, TextureFlags flags, int mipCount, int arraySize, GraphicsResourceUsage usage)
        {
            usage = (flags & TextureFlags.UnorderedAccess) != 0 ? GraphicsResourceUsage.Default : usage;
            var desc = new TextureDescription()
            {
                Dimension = TextureDimension.Texture1D,
                Width = width,
                Height = 1,
                Depth = 1,
                ArraySize = arraySize,
                Flags = flags,
                Format = format,
                MipLevels = CalculateMipMapCount(mipCount, width),
                Usage = GetUsageWithFlags(usage, flags),
            };
            return desc;
        }
    }
}