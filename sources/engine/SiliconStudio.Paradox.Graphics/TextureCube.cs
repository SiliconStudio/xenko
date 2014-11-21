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
    /// A TextureCube frontend to <see cref="SharpDX.Direct3D11.Texture2D"/>.
    /// </summary>
    [DataConverter(AutoGenerate = false, ContentReference = true, DataType = false)]
    public partial class TextureCube : Texture2DBase
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
            return new TextureCube(GraphicsDevice, GetCloneableDescription());
        }

        /// <summary>
        /// Return an equivalent staging texture CPU read-writable from this instance.
        /// </summary>
        /// <returns></returns>
        public override Texture ToStaging()
        {
            return new TextureCube(this.GraphicsDevice, this.Description.ToStagingDescription());
        }

        /// <summary>
        /// Creates a new texture from a <see cref="Texture2DDescription"/>.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="description">The description.</param>
        /// <returns>
        /// A new instance of <see cref="TextureCube"/> class.
        /// </returns>
        public static TextureCube New(GraphicsDevice device, TextureDescription description)
        {
            return new TextureCube(device, description);
        }

        /// <summary>
        /// Creates a new texture from a <see cref="Direct3D11.Texture2D"/>.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="texture">The native texture <see cref="Direct3D11.Texture2D"/>.</param>
        /// <returns>
        /// A new instance of <see cref="TextureCube"/> class.
        /// </returns>
        public static TextureCube New(GraphicsDevice device, TextureCube texture)
        {
            return new TextureCube(device, texture);
        }

        /// <summary>
        /// Creates a new <see cref="TextureCube"/>.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="size">The size (in pixels) of the top-level faces of the cube texture.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="isUnorderedReadWrite">true if the texture needs to support unordered read write.</param>
        /// <returns>
        /// A new instance of <see cref="Texture2D"/> class.
        /// </returns>
        public static TextureCube New(GraphicsDevice device, int size, PixelFormat format, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Default)
        {
            return New(device, size, false, format, textureFlags, usage);
        }

        /// <summary>
        /// Creates a new <see cref="TextureCube"/>.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="size">The size (in pixels) of the top-level faces of the cube texture.</param>
        /// <param name="mipCount">Number of mipmaps, set to true to have all mipmaps, set to an int >=1 for a particular mipmap count.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="isUnorderedReadWrite">true if the texture needs to support unordered read write.</param>
        /// <returns>
        /// A new instance of <see cref="Texture2D"/> class.
        /// </returns>
        public static TextureCube New(GraphicsDevice device, int size, MipMapCount mipCount, PixelFormat format, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Default)
        {
            return new TextureCube(device, NewTextureCubeDescription(size, format, textureFlags, mipCount, usage));
        }

        /// <summary>
        /// Creates a new <see cref="TextureCube" /> from a initial data..
        /// </summary>
        /// <typeparam name="T">Type of a pixel data</typeparam>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="size">The size (in pixels) of the top-level faces of the cube texture.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="isUnorderedReadWrite">true if the texture needs to support unordered read write.</param>
        /// <param name="textureData">an array of 6 textures. See remarks</param>
        /// <returns>A new instance of <see cref="TextureCube" /> class.</returns>
        /// <remarks>
        /// The first dimension of mipMapTextures describes the number of array (TextureCube Array), the second is the texture data for a particular cube face.
        /// </remarks>
        public unsafe static TextureCube New<T>(GraphicsDevice device, int size, PixelFormat format, T[][] textureData, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable) where T : struct
        {
            if (textureData.Length != 6)
                throw new ArgumentException("Invalid texture datas. First dimension must be equal to 6", "textureData");

            var dataBoxes = new DataBox[6];

            dataBoxes[0] = GetDataBox(format, size, size, 1, textureData[0], (IntPtr)Interop.Fixed(textureData[0]));
            dataBoxes[1] = GetDataBox(format, size, size, 1, textureData[0], (IntPtr)Interop.Fixed(textureData[1]));
            dataBoxes[2] = GetDataBox(format, size, size, 1, textureData[0], (IntPtr)Interop.Fixed(textureData[2]));
            dataBoxes[3] = GetDataBox(format, size, size, 1, textureData[0], (IntPtr)Interop.Fixed(textureData[3]));
            dataBoxes[4] = GetDataBox(format, size, size, 1, textureData[0], (IntPtr)Interop.Fixed(textureData[4]));
            dataBoxes[5] = GetDataBox(format, size, size, 1, textureData[0], (IntPtr)Interop.Fixed(textureData[5]));

            return new TextureCube(device, NewTextureCubeDescription(size, format, textureFlags, 1, usage), dataBoxes);
        }

        /// <summary>
        /// Creates a new <see cref="TextureCube" /> from a initial data..
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="size">The size (in pixels) of the top-level faces of the cube texture.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="isUnorderedReadWrite">true if the texture needs to support unordered read write.</param>
        /// <param name="textureData">an array of 6 textures. See remarks</param>
        /// <returns>A new instance of <see cref="TextureCube" /> class.</returns>
        /// <remarks>
        /// The first dimension of mipMapTextures describes the number of array (TextureCube Array), the second is the texture data for a particular cube face.
        /// </remarks>
        public static TextureCube New(GraphicsDevice device, int size, PixelFormat format, DataBox[] textureData, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable)
        {
            if (textureData.Length != 6)
                throw new ArgumentException("Invalid texture datas. First dimension must be equal to 6", "textureData");

            return new TextureCube(device, NewTextureCubeDescription(size, format, textureFlags, 1, usage), textureData);
        }

        /// <summary>
        /// Creates a new <see cref="TextureCube" /> directly from an <see cref="Image"/>.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="image">An image in CPU memory.</param>
        /// <param name="isUnorderedReadWrite">true if the texture needs to support unordered read write.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>A new instance of <see cref="TextureCube" /> class.</returns>
        public static new TextureCube New(GraphicsDevice device, Image image, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (image.Description.Dimension != TextureDimension.TextureCube)
                throw new ArgumentException("Invalid image. Must be Cube", "image");

            return new TextureCube(device, CreateTextureDescriptionFromImage(image, textureFlags, usage), image.ToDataBox());
        }

        /// <summary>
        /// Loads a Cube texture from a stream.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/>.</param>
        /// <param name="stream">The stream to load the texture from.</param>
        /// <param name="isUnorderedReadWrite">True to load the texture with unordered access enabled. Default is false.</param>
        /// <param name="usage">Usage of the resource. Default is <see cref="GraphicsResourceUsage.Immutable"/> </param>
        /// <exception cref="ArgumentException">If the texture is not of type Cube</exception>
        /// <returns>A texture</returns>
        public static TextureCube Load(GraphicsDevice device, Stream stream, bool isUnorderedReadWrite = false, GraphicsResourceUsage usage = GraphicsResourceUsage.Immutable)
        {
            var texture = Texture.Load(device, stream, TextureFlags.ShaderResource, usage);
            if (!(texture is TextureCube))
                throw new ArgumentException(string.Format("Texture is not type of [TextureCube] but [{0}]", texture.GetType().Name));
            return (TextureCube)texture;
        }

        protected static TextureDescription NewTextureCubeDescription(int size, PixelFormat format, TextureFlags textureFlags, int mipCount, GraphicsResourceUsage usage)
        {
            var desc = NewDescription(size, size, format, textureFlags, mipCount, 6, usage);
            desc.Dimension = TextureDimension.TextureCube;
            return desc;
        }
    }
}