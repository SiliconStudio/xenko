// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading;

using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Paradox.Assets.Materials;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.TextureConverter;

namespace SiliconStudio.Paradox.Assets.Textures
{
    /// <summary>
    /// An helper for the compile commands that needs to process textures.
    /// </summary>
    public static class TextureCommandHelper
    {
        /// <summary>
        /// Returns true if the provided int is a power of 2.
        /// </summary>
        /// <param name="x">the int value to test</param>
        /// <returns>true if power of two</returns>
        public static  bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        /// <summary>
        /// Returns true if the PVRTC can be used for the provided texture size.
        /// </summary>
        /// <param name="textureSize">the size of the texture</param>
        /// <returns>true if PVRTC is supported</returns>
        public static bool SupportPVRTC(Int2 textureSize)
        {
            return textureSize.X == textureSize.Y && IsPowerOfTwo(textureSize.X);
        }

        /// <summary>
        /// Utility function to check that the texture size is supported on the graphics platform for the provided graphics profile.
        /// </summary>
        /// <param name="textureFormat">The desired type of format for the output texture</param>
        /// <param name="textureSize">The size of the texture</param>
        /// <param name="graphicsProfile">The graphics profile</param>
        /// <param name="platform">The graphics platform</param>
        /// <param name="generateMipmaps">Indicate if mipmaps should be generated for the output texture</param>
        /// <param name="logger">The logger used to log messages</param>
        /// <returns>true if the texture size is supported</returns>
        public static bool TextureSizeSupported(TextureFormat textureFormat, GraphicsPlatform platform, GraphicsProfile graphicsProfile, Int2 textureSize, bool generateMipmaps, Logger logger)
        {
            // compressed DDS files has to have a size multiple of 4.
            if (platform == GraphicsPlatform.Direct3D11 && textureFormat == TextureFormat.Compressed
                && ((textureSize.X % 4) != 0 || (textureSize.Y % 4) != 0))
            {
                logger.Error("DDS compression does not support texture files that do not have a size multiple of 4." +
                             "Please disable texture compression or adjust your texture size to multiple of 4.");
                return false;
            }

            int maxTextureSize;

            // determine if the desired size if valid depending on the graphics profile
            switch (graphicsProfile)
            {
                case GraphicsProfile.Level_9_1:
                case GraphicsProfile.Level_9_2:
                case GraphicsProfile.Level_9_3:
                    if (generateMipmaps && (!IsPowerOfTwo(textureSize.Y) || !IsPowerOfTwo(textureSize.X)))
                    {
                        logger.Error("Graphic profiles 9.x do not support mipmaps with textures that are not power of 2. " +
                                     "Please disable mipmap generation, modify your texture resolution or upgrade your graphic profile to a value >= 10.0.");
                        return false;
                    }
                    maxTextureSize = graphicsProfile >= GraphicsProfile.Level_9_3 ? 4096 : 2048;
                    break;
                case GraphicsProfile.Level_10_0:
                case GraphicsProfile.Level_10_1:
                    maxTextureSize = 8192;
                    break;
                case GraphicsProfile.Level_11_0:
                case GraphicsProfile.Level_11_1:
                    maxTextureSize = 16384;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("graphicsProfile");
            }

            if (textureSize.X > maxTextureSize || textureSize.Y > maxTextureSize)
            {
                logger.Error("Graphic profile {0} do not support texture with resolution {2} x {3} because it is larger than {1}. " +
                             "Please reduce texture size or upgrade your graphic profile.", graphicsProfile, maxTextureSize, textureSize.X, textureSize.Y);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if alpha channel should be separated from a given texture's attribute and graphics profile
        /// </summary>
        /// <param name="alphaFormat">Alpha format for a texture</param>
        /// <param name="textureFormat">Texture format</param>
        /// <param name="platform">Platform</param>
        /// <param name="graphicsProfile">Level of graphics</param>
        /// <returns></returns>
        public static bool ShouldSeparateAlpha(AlphaFormat alphaFormat, TextureFormat textureFormat, PlatformType platform, GraphicsProfile graphicsProfile)
        {
            if (alphaFormat != AlphaFormat.None && textureFormat == TextureFormat.Compressed && platform == PlatformType.Android)
            {
                switch (graphicsProfile)
                {
                    case GraphicsProfile.Level_9_1:
                    case GraphicsProfile.Level_9_2:
                    case GraphicsProfile.Level_9_3:
                        // Android with OpenGLES < 3.0 require alpha splitting if the image is compressed since ETC1 compresses only RGB
                        return true;
                    case GraphicsProfile.Level_10_0:
                    case GraphicsProfile.Level_10_1:
                    case GraphicsProfile.Level_11_0:
                    case GraphicsProfile.Level_11_1:
                    case GraphicsProfile.Level_11_2:
                        // Since OpenGLES 3.0, ETC2 RGBA is used instead of ETC1 RGB so alpha is compressed along with RGB; therefore, no need to split alpha
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException("graphicsProfile");
                }
            }

            return false;
        }

        /// <summary>
        /// Determine the output format of the texture depending on the platform and asset properties.
        /// </summary>
        /// <param name="textureFormat">The desired texture output format type</param>
        /// <param name="alphaFormat">The alpha format desired in output</param>
        /// <param name="platform">The platform type</param>
        /// <param name="graphicsPlatform">The graphics platform</param>
        /// <param name="graphicsProfile">The graphics profile</param>
        /// <param name="imageSize">The texture output size</param>
        /// <param name="inputImageFormat">The pixel format of the input image</param>
        /// <returns>The pixel format to use as output</returns>
        public static PixelFormat DetermineOutputFormat(TextureFormat textureFormat, AlphaFormat alphaFormat, PlatformType platform, GraphicsPlatform graphicsPlatform,
            GraphicsProfile graphicsProfile, Int2 imageSize, PixelFormat inputImageFormat)
        {
            PixelFormat outputFormat;
            switch (textureFormat)
            {
                case TextureFormat.Compressed:
                    switch (platform)
                    {
                        case PlatformType.Android:
                            switch (graphicsProfile)
                            {
                                case GraphicsProfile.Level_9_1:
                                case GraphicsProfile.Level_9_2:
                                case GraphicsProfile.Level_9_3:
                                    outputFormat = alphaFormat == AlphaFormat.None ? PixelFormat.ETC1 : PixelFormat.R8G8B8A8_UNorm;
                                    break;
                                case GraphicsProfile.Level_10_0:
                                case GraphicsProfile.Level_10_1:
                                case GraphicsProfile.Level_11_0:
                                case GraphicsProfile.Level_11_1:
                                case GraphicsProfile.Level_11_2:
                                    // GLES3.0 starting from Level_10_0, this profile enables ETC2 compression on Android
                                    outputFormat = alphaFormat == AlphaFormat.None ? PixelFormat.ETC1 : PixelFormat.ETC2_RGBA;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException("graphicsProfile");
                            }
                            break;
                        case PlatformType.iOS:
                            // PVRTC works only for square POT textures
                            if (SupportPVRTC(imageSize))
                            {
                                switch (alphaFormat)
                                {
                                    case AlphaFormat.None:
                                        // DXT1 handles 1-bit alpha channel
                                        outputFormat = PixelFormat.PVRTC_4bpp_RGB;
                                        break;
                                    case AlphaFormat.Mask:
                                        // DXT1 handles 1-bit alpha channel
                                        // TODO: Not sure about the equivalent here?
                                        outputFormat = PixelFormat.PVRTC_4bpp_RGBA;
                                        break;
                                    case AlphaFormat.Explicit:
                                    case AlphaFormat.Interpolated:
                                        // DXT3 is good at sharp alpha transitions
                                        // TODO: Not sure about the equivalent here?
                                        outputFormat = PixelFormat.PVRTC_4bpp_RGBA;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                            else
                            {
                                outputFormat = PixelFormat.R8G8B8A8_UNorm;
                            }
                            break;
                        case PlatformType.Windows:
                        case PlatformType.WindowsPhone:
                        case PlatformType.WindowsStore:
                            switch (graphicsPlatform)
                            {
                                case GraphicsPlatform.Direct3D11:
                                    switch (alphaFormat)
                                    {
                                        case AlphaFormat.None:
                                        case AlphaFormat.Mask:
                                            // DXT1 handles 1-bit alpha channel
                                            outputFormat = PixelFormat.BC1_UNorm;
                                            break;
                                        case AlphaFormat.Explicit:
                                            // DXT3 is good at sharp alpha transitions
                                            outputFormat = PixelFormat.BC2_UNorm;
                                            break;
                                        case AlphaFormat.Interpolated:
                                            // DXT5 is good at alpha gradients
                                            outputFormat = PixelFormat.BC3_UNorm;
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }
                                    break;
                                case GraphicsPlatform.OpenGLES: // OpenGLES on Windows
                                    switch (graphicsProfile)
                                    {
                                        case GraphicsProfile.Level_9_1:
                                        case GraphicsProfile.Level_9_2:
                                        case GraphicsProfile.Level_9_3:
                                            outputFormat = alphaFormat == AlphaFormat.None ? PixelFormat.ETC1 : PixelFormat.R8G8B8A8_UNorm;
                                            break;
                                        case GraphicsProfile.Level_10_0:
                                        case GraphicsProfile.Level_10_1:
                                        case GraphicsProfile.Level_11_0:
                                        case GraphicsProfile.Level_11_1:
                                        case GraphicsProfile.Level_11_2:
                                            // GLES3.0 starting from Level_10_0, this profile enables ETC2 compression on Android
                                            outputFormat = alphaFormat == AlphaFormat.None ? PixelFormat.ETC1 : PixelFormat.ETC2_RGBA;
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException("graphicsProfile");
                                    }
                                    break;
                                default:
                                    // OpenGL on Windows
                                    // TODO: Need to handle OpenGL Desktop compression
                                    outputFormat = PixelFormat.R8G8B8A8_UNorm;
                                    break;
                            }
                            break;
                        default:
                            throw new NotSupportedException("Platform " + platform + " is not supported by TextureTool");
                    }
                    break;
                case TextureFormat.HighColor:
                    if (alphaFormat == AlphaFormat.None)
                        outputFormat = PixelFormat.B5G6R5_UNorm;
                    else if (alphaFormat == AlphaFormat.Mask)
                        outputFormat = PixelFormat.B5G5R5A1_UNorm;
                    else
                        throw new NotImplementedException("This alpha format requires a TrueColor texture format.");
                    break;
                case TextureFormat.TrueColor:
                    outputFormat = PixelFormat.R8G8B8A8_UNorm;
                    break;
                //case TextureFormat.Custom:
                //    throw new NotSupportedException();
                //    break;
                case TextureFormat.AsIs:
                    outputFormat = inputImageFormat;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return outputFormat;
        }

        public static ResultStatus ImportAndSaveTextureImage(UFile sourcePath, string outputUrl, TextureAsset textureAsset, TextureConvertParameters parameters, bool separateAlpha, CancellationToken cancellationToken, Logger logger)
        {
            var assetManager = new AssetManager();

            using (var texTool = new TextureTool())
            using (var texImage = texTool.Load(sourcePath))
            {
                // Apply transformations
                texTool.Decompress(texImage);

                if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                    return ResultStatus.Cancelled;
                
                // Resize the image
                if (textureAsset.IsSizeInPercentage)
                    texTool.Rescale(texImage, textureAsset.Width / 100.0f, textureAsset.Height / 100.0f, Filter.Rescaling.Lanczos3);
                else
                    texTool.Resize(texImage, (int)textureAsset.Width, (int)textureAsset.Height, Filter.Rescaling.Lanczos3);

                if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                    return ResultStatus.Cancelled;

                // texture size is now determined, we can cache it
                var textureSize = new Int2(texImage.Width, texImage.Height);

                // Check that the resulting texture size is supported by the targeted graphics profile
                if (!TextureSizeSupported(textureAsset.Format, parameters.GraphicsPlatform, parameters.GraphicsProfile, textureSize, textureAsset.GenerateMipmaps, logger))
                    return ResultStatus.Failed;


                // Apply the color key
                if (textureAsset.ColorKeyEnabled)
                    texTool.ColorKey(texImage, textureAsset.ColorKeyColor);

                if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                    return ResultStatus.Cancelled;


                // Pre-multiply alpha
                if (textureAsset.PremultiplyAlpha)
                    texTool.PreMultiplyAlpha(texImage);

                if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                    return ResultStatus.Cancelled;


                // Generate mipmaps
                if (textureAsset.GenerateMipmaps)
                    texTool.GenerateMipMaps(texImage, Filter.MipMapGeneration.Box);

                if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                    return ResultStatus.Cancelled;


                // Convert/Compress to output format
                // TODO: Change alphaFormat depending on actual image content (auto-detection)?
                var outputFormat = DetermineOutputFormat(textureAsset.Format, textureAsset.Alpha, parameters.Platform, parameters.GraphicsPlatform, parameters.GraphicsProfile, textureSize, texImage.Format);
                texTool.Compress(texImage, outputFormat, (TextureConverter.Requests.TextureQuality)parameters.TextureQuality);

                if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                    return ResultStatus.Cancelled;


                // Save the texture
                if (separateAlpha)
                {
                    TextureAlphaComponentSplitter.CreateAndSaveSeparateTextures(texTool, texImage, outputUrl, textureAsset.GenerateMipmaps);
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
    }
}