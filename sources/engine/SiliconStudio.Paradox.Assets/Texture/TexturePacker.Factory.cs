using System;
using System.Threading;

using SiliconStudio.BuildEngine;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.TextureConverter;

namespace SiliconStudio.Paradox.Assets.Texture
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
                        ptr[i] = Color.Transparent;
                }

                // Fill in textureData from textureAtlas
                foreach (var intemediateTexture in textureAtlas.Textures)
                {
                    var isRotated = intemediateTexture.Region.IsRotated;
                    var sourceTexture = intemediateTexture.Texture;
                    var sourceTextureWidth = sourceTexture.Description.Width;
                    var sourceTextureHeight = sourceTexture.Description.Height;

                    var borderSize = intemediateTexture.BorderSize;
                    var addressModeU = intemediateTexture.AddressModeU;
                    var addressModeV = intemediateTexture.AddressModeV;
                    var borderColor = intemediateTexture.BorderColor;

                    unsafe
                    {
                        var atlasData = (Color*)atlasTexture.DataPointer;
                        var textureData = (Color*)sourceTexture.DataPointer;

                        for (var y = 0; y < intemediateTexture.Region.Value.Height; ++y)
                            for (var x = 0; x < intemediateTexture.Region.Value.Width; ++x)
                            {
                                var targetIndexX = intemediateTexture.Region.Value.X + x;
                                var targetIndexY = intemediateTexture.Region.Value.Y + y;

                                var sourceIndexX = GetSourceTextureIndex(x - borderSize, isRotated ? sourceTextureHeight : sourceTextureWidth, addressModeU);
                                var sourceIndexY = GetSourceTextureIndex(y - borderSize, isRotated ? sourceTextureWidth : sourceTextureHeight, addressModeV);

                                atlasData[targetIndexY * textureAtlas.Width + targetIndexX] = (sourceIndexX < 0 || sourceIndexY < 0)
                                    ? borderColor ?? Color.Transparent :
                                        textureData[isRotated ? (sourceTextureHeight - 1 - sourceIndexX) * sourceTextureWidth + sourceIndexY
                                            : (sourceIndexY * sourceTextureWidth + sourceIndexX)];
                            }
                    }
                }

                return atlasTexture;
            }

            /// <summary>
            /// Creates and Saves texture atlas image to a disk
            /// </summary>
            /// <typeparam name="T">ImageGroupAsset</typeparam>
            /// <param name="textureAtlas">Input texture atlas</param>
            /// <param name="outputUrl">Output url to be saved</param>
            /// <param name="parameters">Parameters of image group asset</param>
            /// <param name="separateAlpha">Should alpha be saved separately; This is true for Android</param>
            /// <param name="cancellationToken">Cancellation token for cancelling the build task</param>
            /// <param name="logger">Logger to output the warnings and errors</param>
            /// <returns></returns>
            public static ResultStatus CreateAndSaveTextureAtlasImage<T>(TextureAtlas textureAtlas, string outputUrl,
                ImageGroupParameters<T> parameters, bool separateAlpha, CancellationToken cancellationToken, Logger logger)
                where T : ImageGroupAsset
            {
                var assetManager = new AssetManager();

                var imageGroup = parameters.GroupAsset;

                using (var atlasImage = CreateTextureAtlas(textureAtlas))
                using (var texTool = new TextureTool())
                using (var texImage = texTool.Load(atlasImage))
                {
                    // Apply transformations
                    texTool.Decompress(texImage);

                    if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                        return ResultStatus.Cancelled;

                    // texture size is now determined, we can cache it
                    var textureSize = new Int2(texImage.Width, texImage.Height);

                    // Check that the resulting texture size is supported by the targeted graphics profile
                    if (!TextureCommandHelper.TextureSizeSupported(imageGroup.Format, parameters.GraphicsPlatform, parameters.GraphicsProfile, textureSize, imageGroup.GenerateMipmaps, logger))
                        return ResultStatus.Failed;

                    // Apply the color key
                    if (imageGroup.ColorKeyEnabled)
                        texTool.ColorKey(texImage, imageGroup.ColorKeyColor);

                    if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                        return ResultStatus.Cancelled;

                    // Pre-multiply alpha
                    if (imageGroup.PremultiplyAlpha)
                        texTool.PreMultiplyAlpha(texImage);

                    if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                        return ResultStatus.Cancelled;

                    // Generate mipmaps
                    if (imageGroup.GenerateMipmaps)
                        texTool.GenerateMipMaps(texImage, Filter.MipMapGeneration.Box);

                    if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                        return ResultStatus.Cancelled;

                    // Convert/Compress to output format
                    // TODO: Change alphaFormat depending on actual image content (auto-detection)?
                    var outputFormat = TextureCommandHelper.DetermineOutputFormat(imageGroup.Format, imageGroup.Alpha, parameters.Platform,
                        parameters.GraphicsPlatform, textureSize, texImage.Format);

                    texTool.Compress(texImage, outputFormat, (TextureConverter.Requests.TextureQuality)parameters.TextureQuality);

                    if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                        return ResultStatus.Cancelled;

                    // Save the texture
                    if (separateAlpha)
                    {
                        TextureAlphaComponentSplitter.CreateAndSaveSeparateTextures(texTool, texImage, outputUrl, imageGroup.GenerateMipmaps);
                    }
                    else
                    {
                        using (var outputImage = texTool.ConvertToParadoxImage(texImage))
                        {
                            if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                                return ResultStatus.Cancelled;

                            assetManager.Save(outputUrl, outputImage);

                            logger.Info("Compression successful [{3}] to ({0}x{1},{2})", outputImage.Description.Width, outputImage.Description.Height, outputImage.Description.Format, outputUrl);
                        }
                    }
                }
                return ResultStatus.Successful;
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
