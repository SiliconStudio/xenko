using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Textures.Packing
{
    /// <summary>
    /// A Atlas Texture Factory that contains APIs related to atlas texture creation
    /// </summary>
    public static class AtlasTextureFactory
    {
        /// <summary>
        /// Creates texture atlas image from a given texture atlas
        /// </summary>
        /// <param name="atlasTextureLayout">Input texture atlas</param>
        /// <returns></returns>
        public static Image CreateTextureAtlas(AtlasTextureLayout atlasTextureLayout)
        {
            var atlasTexture = Image.New2D(atlasTextureLayout.Width, atlasTextureLayout.Height, 1, PixelFormat.R8G8B8A8_UNorm);

            unsafe
            {
                var ptr = (Color*)atlasTexture.DataPointer;

                // Clean the data
                for (var i = 0; i < atlasTexture.PixelBuffer[0].Height * atlasTexture.PixelBuffer[0].Width; ++i)
                    ptr[i] = Color.Zero;
            }

            // Fill in textureData from AtlasTextureLayout
            foreach (var element in atlasTextureLayout.Textures)
            {
                var isRotated = element.DestinationRegion.IsRotated;
                var sourceTexture = element.Texture;
                var sourceTextureWidth = sourceTexture.Description.Width;

                var addressModeU = element.BorderModeU;
                var addressModeV = element.BorderModeV;
                var borderColor = element.BorderColor;

                var targetRegionWidth = isRotated ? element.SourceRegion.Height : element.SourceRegion.Width;
                var targetRegionHeight = isRotated ? element.SourceRegion.Width : element.SourceRegion.Height;

                unsafe
                {
                    var atlasData = (Color *)atlasTexture.DataPointer;
                    var textureData = (Color *)sourceTexture.DataPointer;

                    for (var y = 0; y < element.DestinationRegion.Height; ++y)
                    {

                        for (var x = 0; x < element.DestinationRegion.Width; ++x)
                        {
                            // Get index of source image, if it's the border at this point sourceIndexX and sourceIndexY will be -1
                            var sourceCoordinateX = GetSourceTextureCoordinate(x - element.BorderSize, targetRegionWidth, addressModeU);
                            var sourceCoordinateY = GetSourceTextureCoordinate(y - element.BorderSize, targetRegionHeight, addressModeV);

                            // Check if this image uses border mode, and is in the border area
                            var isBorderMode = sourceCoordinateX < 0 || sourceCoordinateY < 0;

                            if (isRotated)
                            {
                                // Modify index for rotating
                                var tmp = sourceCoordinateY;

                                // Since intemediateTexture.DestinationRegion contains the border, we need to delete the border out
                                sourceCoordinateY = (element.DestinationRegion.Width - element.BorderSize * 2) - 1 - sourceCoordinateX;
                                sourceCoordinateX = tmp;
                            }

                            // Add offset from the region
                            sourceCoordinateX += element.SourceRegion.X;
                            sourceCoordinateY += element.SourceRegion.Y;
                            var readFromIndex = sourceCoordinateY*sourceTextureWidth + sourceCoordinateX; // read index from source image

                            // Prepare writeToIndex
                            var targetCoordinateX = element.DestinationRegion.X + x;
                            var targetCoordinateY = element.DestinationRegion.Y + y;
                            var writeToIndex = targetCoordinateY*atlasTextureLayout.Width + targetCoordinateX; // write index to atlas buffer

                            atlasData[writeToIndex] = isBorderMode ? borderColor : textureData[readFromIndex];
                        }
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
        internal static int GetSourceTextureCoordinate(int value, int maxValue, TextureAddressMode mode)
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
                    return Math.Min(Math.Abs(value), maxValue - 1);
                case TextureAddressMode.Border:
                    return -1;
                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }
    }
}
