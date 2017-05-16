// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Graphics
{
    public partial struct TextureDescription
    {
        /// <summary>
        /// Creates a new Cube <see cref="TextureDescription" />.
        /// </summary>
        /// <param name="size">The size (in pixels) of the top-level faces of the cube texture.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="textureFlags">The texture flags.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>A new instance of <see cref="TextureDescription" /> class.</returns>
        public static TextureDescription NewCube(int size, PixelFormat format, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Default)
        {
            return NewCube(size, false, format, textureFlags, usage);
        }

        /// <summary>
        /// Creates a new Cube <see cref="TextureDescription"/>.
        /// </summary>
        /// <param name="size">The size (in pixels) of the top-level faces of the cube texture.</param>
        /// <param name="mipCount">Number of mipmaps, set to true to have all mipmaps, set to an int &gt;=1 for a particular mipmap count.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="textureFlags">The texture flags.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>A new instance of <see cref="TextureDescription"/> class.</returns>
        public static TextureDescription NewCube(int size, MipMapCount mipCount, PixelFormat format, TextureFlags textureFlags = TextureFlags.ShaderResource, GraphicsResourceUsage usage = GraphicsResourceUsage.Default)
        {
            return NewCube(size, format, textureFlags, mipCount, usage);
        }

        private static TextureDescription NewCube(int size, PixelFormat format, TextureFlags textureFlags, int mipCount, GraphicsResourceUsage usage)
        {
            var desc = New2D(size, size, format, textureFlags, mipCount, 6, usage, MultisampleCount.None);
            desc.Dimension = TextureDimension.TextureCube;
            return desc;
        }
    }
}
