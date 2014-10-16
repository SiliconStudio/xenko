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
    /// A Texture 2D frontend to <see cref="SharpDX.Direct3D11.Texture2D"/>.
    /// </summary>
    [DataConverter(AutoGenerate = false, ContentReference = true, DataType = false)]
    public partial class Texture2D : Texture2DBase
    {
        protected Texture2D()
        {
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
        public override Texture Clone()
        {
            return new Texture2D(GraphicsDevice, GetCloneableDescription());
        }

        /// <summary>
        /// Return an equivalent staging texture CPU read-writable from this instance.
        /// </summary>
        /// <returns></returns>
        public override Texture ToStaging()
        {
            return new Texture2D(this.GraphicsDevice, this.Description.ToStagingDescription());
        }
        
        /// <summary>
        /// Creates a new texture from a <see cref="Direct3D11.Texture2D"/>.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="texture">The native texture <see cref="Direct3D11.Texture2D"/>.</param>
        /// <returns>
        /// A new instance of <see cref="Texture2D"/> class.
        /// </returns>
        public static Texture2D New(GraphicsDevice device, Texture2D texture)
        {
            return new Texture2D(device, texture);
        }

        /// <summary>
        /// Creates a new texture from a <see cref="Direct3D11.Texture2D"/>.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="texture">The native texture <see cref="Direct3D11.Texture2D"/>.</param>
        /// <returns>
        /// A new instance of <see cref="Texture2D"/> class.
        /// </returns>
        public static Texture2D New(GraphicsDevice device, TextureDescription textureDescription)
        {
            return new Texture2D(device, textureDescription);
        }

        /// <summary>
        /// Creates a new <see cref="Texture2D" /> with a single mipmap.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="textureFlags">true if the texture needs to support unordered read write.</param>
        /// <param name="arraySize">Size of the texture 2D array, default to 1.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>A new instance of <see cref="Texture2D" /> class.</returns>
        public static Texture2D New(GraphicsDevice device, int width, int height, PixelFormat format, TextureFlags textureFlags = TextureFlags.ShaderResource, int arraySize = 1, GraphicsResourceUsage usage = GraphicsResourceUsage.Default)
        {
            return New(device, width, height, false, format, textureFlags, arraySize, usage);
        }

        /// <summary>
        /// Creates a new <see cref="Texture2D" />.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="mipCount">Number of mipmaps, set to true to have all mipmaps, set to an int >=1 for a particular mipmap count.</param>
        /// <param name="textureFlags">true if the texture needs to support unordered read write.</param>
        /// <param name="arraySize">Size of the texture 2D array, default to 1.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>A new instance of <see cref="Texture2D" /> class.</returns>
        public static Texture2D New(GraphicsDevice device, int width, int height, MipMapCount mipCount, PixelFormat format, TextureFlags textureFlags = TextureFlags.ShaderResource, int arraySize = 1, GraphicsResourceUsage usage = GraphicsResourceUsage.Default)
        {
            return new Texture2D(device, NewDescription(width, height, format, textureFlags, mipCount, arraySize, usage));
        }

        /// <summary>
        /// Creates a new <see cref="Texture2D" /> with a single level of mipmap.
        /// </summary>
        /// <typeparam name="T">Type of the pixel data to upload to the texture.</typeparam>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="textureFlags">true if the texture needs to support unordered read write.</param>
        /// <param name="textureData">The texture data for a single mipmap and a single array slice. See remarks</param>
        /// <returns>A new instance of <see cref="Texture2D" /> class.</returns>
        /// <remarks>
        /// Each value in textureData is a pixel in the destination texture.
        /// </remarks>
        public unsafe static Texture2D New<T>(GraphicsDevice device, int width, int height, PixelFormat format, T[] textureData, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable) where T : struct
        {
            return New(device, width, height, 1, format, new []{ GetDataBox(format, width, height, 1, textureData, (IntPtr)Interop.Fixed(textureData)) }, textureFlags, 1, usage);
        }

        /// <summary>
        /// Creates a new <see cref="Texture2D" />.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="mipCount">Number of mipmaps, set to true to have all mipmaps, set to an int >=1 for a particular mipmap count.</param>
        /// <param name="textureData">Texture datas through an array of <see cref="DataBox"/> </param>
        /// <param name="textureFlags">true if the texture needs to support unordered read write.</param>
        /// <param name="arraySize">Size of the texture 2D array, default to 1.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>A new instance of <see cref="Texture2D" /> class.</returns>
        public static Texture2D New(GraphicsDevice device, int width, int height, MipMapCount mipCount, PixelFormat format, DataBox[] textureData, TextureFlags textureFlags = TextureFlags.ShaderResource, int arraySize = 1, GraphicsResourceUsage usage = GraphicsResourceUsage.Default)
        {
            return new Texture2D(device, NewDescription(width, height, format, textureFlags, mipCount, arraySize, usage), textureData);
        }

        /// <summary>
        /// Creates a new <see cref="Texture2D" /> directly from an <see cref="Image"/>.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="image">An image in CPU memory.</param>
        /// <param name="textureFlags">true if the texture needs to support unordered read write.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>A new instance of <see cref="Texture2D" /> class.</returns>
        public static new Texture2D New(GraphicsDevice device, Image image, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (image.Description.Dimension != TextureDimension.Texture2D)
                throw new ArgumentException("Invalid image. Must be 2D", "image");

            return new Texture2D(device, CreateTextureDescriptionFromImage(image, textureFlags, usage), image.ToDataBox());
        }

        /// <summary>
        /// Loads a 2D texture from a stream.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="stream">The stream to load the texture from.</param>
        /// <param name="textureFlags">True to load the texture with unordered access enabled. Default is false.</param>
        /// <param name="usage">Usage of the resource. Default is <see cref="GraphicsResourceUsage.Immutable"/> </param>
        /// <exception cref="ArgumentException">If the texture is not of type 2D</exception>
        /// <returns>A texture</returns>
        public static new Texture2D Load(GraphicsDevice device, Stream stream, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable)
        {
            var texture = Texture.Load(device, stream, textureFlags, usage);
            if (!(texture is Texture2D))
                throw new ArgumentException(string.Format("Texture is not type of [Texture2D] but [{0}]", texture.GetType().Name));
            return (Texture2D)texture;
        }
    }
}