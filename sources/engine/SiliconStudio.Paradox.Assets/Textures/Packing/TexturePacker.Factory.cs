using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Textures.Packing
{
    /// <summary>
    /// A partial class of TexturePacker that contains a Factory to create and/or save texture atlas
    /// </summary>
    public partial class TexturePacker
    {
        /// <summary>
        /// A texture Atlas Factory that contains APIs related to atlas texture creation
        /// </summary>
        public static class Factory
        {
            /// <summary>
            /// Creates texture atlas image from a given texture atlas
            /// </summary>
            /// <param name="textureAtlas">Input texture atlas</param>
            /// <returns></returns>
            public static Image CreateTextureAtlas(TextureAtlas textureAtlas)
            {
                var atlasTexture = Image.New2D(textureAtlas.Width, textureAtlas.Height, 1, PixelFormat.R8G8B8A8_UNorm);

                unsafe
                {
                    var ptr = (Color*)atlasTexture.DataPointer;

                    // Clean the data
                    for (var i = 0; i < atlasTexture.PixelBuffer[0].Height * atlasTexture.PixelBuffer[0].Width; ++i)
                        ptr[i] = Color.Zero;
                }

                // Fill in textureData from textureAtlas
                foreach (var intemediateTexture in textureAtlas.Textures)
                {
                    var isRotated = intemediateTexture.PackingRegion.IsRotated;
                    var sourceTexture = intemediateTexture.Texture;
                    var sourceTextureWidth = sourceTexture.Description.Width;

                    var addressModeU = intemediateTexture.AddressModeU;
                    var addressModeV = intemediateTexture.AddressModeV;
                    var borderColor = intemediateTexture.BorderColor;

                    var targetRegionWidth = isRotated ? intemediateTexture.Region.Height : intemediateTexture.Region.Width;
                    var targetRegionHeight = isRotated ? intemediateTexture.Region.Width : intemediateTexture.Region.Height;

                    unsafe
                    {
                        var atlasData = (Color *)atlasTexture.DataPointer;
                        var textureData = (Color *)sourceTexture.DataPointer;

                        for (var y = 0; y < intemediateTexture.PackingRegion.Value.Height; ++y)
                            for (var x = 0; x < intemediateTexture.PackingRegion.Value.Width; ++x)
                            {
                                // Get index of source image, if it's the border at this point sourceIndexX and sourceIndexY will be -1
                                var sourceIndexX = GetSourceTextureIndex(x - textureAtlas.BorderSize, targetRegionWidth, addressModeU);
                                var sourceIndexY = GetSourceTextureIndex(y - textureAtlas.BorderSize, targetRegionHeight, addressModeV);

                                // Check if this image uses border mode, and is in the border area
                                var isBorderMode = sourceIndexX < 0 || sourceIndexY < 0;

                                if (isRotated)
                                {
                                    // Modify index for rotating
                                    var tmp = sourceIndexY;

                                    // Since intemediateTexture.PackingRegion contains the border, we need to delete the border out
                                    sourceIndexY = (intemediateTexture.PackingRegion.Value.Width - textureAtlas.BorderSize * 2) - 1 - sourceIndexX; 
                                    sourceIndexX = tmp;
                                }

                                // Add offset from the region
                                sourceIndexX += intemediateTexture.Region.X;
                                sourceIndexY += intemediateTexture.Region.Y;

                                var readFromIndex = sourceIndexY * sourceTextureWidth + sourceIndexX; // read index from source image

                                // Prepare writeToIndex
                                var targetIndexX = intemediateTexture.PackingRegion.Value.X + x;
                                var targetIndexY = intemediateTexture.PackingRegion.Value.Y + y;

                                var writeToIndex = targetIndexY * textureAtlas.Width + targetIndexX; // write index to atlas buffer

                                atlasData[writeToIndex] = isBorderMode ? (borderColor ?? Color.Transparent) : textureData[readFromIndex];
                            }
                    }
                }

                return atlasTexture;
            }

            /// <summary>
            /// Gets index texture from a source image from a given value, max value and texture address mode.
            /// If index is in [0, maxValue), the output index will be the same as the input index.
            /// Otherwise, the output index will be determined by the texture address mode.
            /// </summary>
            /// <param name="value">Input index value</param>
            /// <param name="maxValue">Max value of an input</param>
            /// <param name="mode">Border mode</param>
            /// <returns></returns>
            public static int GetSourceTextureIndex(int value, int maxValue, TextureAddressMode mode)
            {
                // Invariant condition
                if (0 <= value && value < maxValue) return value;

                switch (mode)
                {
                    case TextureAddressMode.Wrap:
                        return (value >= 0) ? value % maxValue : (maxValue - ((-value) % maxValue)) % maxValue;
                    case TextureAddressMode.Mirror:
                        return (value >= 0) ? (maxValue - 1) - (value % maxValue) : (-value) % maxValue;
                    case TextureAddressMode.Clamp:
                        return (value >= 0) ? maxValue - 1 : 0;
                    case TextureAddressMode.MirrorOnce:
                        var absValue = Math.Abs(value);
                        if (0 <= absValue && absValue < maxValue) return absValue;
                        return Math.Min(absValue, maxValue - 1);
                    case TextureAddressMode.Border:
                        return -1;
                    default:
                        throw new ArgumentOutOfRangeException("mode");
                }
            }
        }
    }
}
