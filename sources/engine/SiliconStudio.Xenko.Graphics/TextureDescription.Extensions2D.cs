// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Graphics
{
    public partial struct TextureDescription
    {
        /// <summary>
        /// Creates a new <see cref="TextureDescription" /> with a single mipmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="textureFlags">true if the texture needs to support unordered read write.</param>
        /// <param name="arraySize">Size of the texture 2D array, default to 1.</param>
        /// <param name="usage">The usage.</param>
        /// <returns>A new instance of <see cref="TextureDescription" /> class.</returns>
        public static TextureDescription New2D(int width, int height, PixelFormat format, TextureFlags textureFlags = TextureFlags.ShaderResource, int arraySize = 1, GraphicsResourceUsage usage = GraphicsResourceUsage.Default)
        {
            return New2D(width, height, false, format, textureFlags, arraySize, usage);
        }

        /// <summary>
        /// Creates a new <see cref="TextureDescription" />.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="mipCount">Number of mipmaps, set to true to have all mipmaps, set to an int &gt;=1 for a particular mipmap count.</param>
        /// <param name="format">Describes the format to use.</param>
        /// <param name="textureFlags">true if the texture needs to support unordered read write.</param>
        /// <param name="arraySize">Size of the texture 2D array, default to 1.</param>
        /// <param name="usage">The usage.</param>
        /// <param name="msaaLevel">The MSAA Level</param>
        /// <returns>A new instance of <see cref="TextureDescription" /> class.</returns>
        public static TextureDescription New2D(int width, int height, MipMapCount mipCount, PixelFormat format, TextureFlags textureFlags = TextureFlags.ShaderResource, int arraySize = 1, GraphicsResourceUsage usage = GraphicsResourceUsage.Default, MSAALevel msaaLevel = MSAALevel.None)
        {
            return New2D(width, height, format, textureFlags, mipCount, arraySize, usage, msaaLevel);
        }

        private static TextureDescription New2D(int width, int height, PixelFormat format, TextureFlags textureFlags, int mipCount, int arraySize, GraphicsResourceUsage usage, MSAALevel msaaLevel)
        {
            if ((textureFlags & TextureFlags.UnorderedAccess) != 0)
                usage = GraphicsResourceUsage.Default;

            var desc = new TextureDescription
            {
                Dimension = TextureDimension.Texture2D,
                Width = width,
                Height = height,
                Depth = 1,
                ArraySize = arraySize,
                MultiSampleLevel = msaaLevel,
                Flags = textureFlags,
                Format = format,
                MipLevels = Texture.CalculateMipMapCount(mipCount, width, height),
                Usage = Texture.GetUsageWithFlags(usage, textureFlags),
            };
            return desc;
        }
    }
}