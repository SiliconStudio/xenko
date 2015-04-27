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
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Data;
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
        /// <param name="platform">The graphics platform</param>
        /// <param name="graphicsProfile">The graphics profile</param>
        /// <param name="textureSizeInput">The texture size input.</param>
        /// <param name="textureSizeRequested">The texture size requested.</param>
        /// <param name="generateMipmaps">Indicate if mipmaps should be generated for the output texture</param>
        /// <param name="logger">The logger.</param>
        /// <returns>true if the texture size is supported</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">graphicsProfile</exception>
        public static Size2 FindBestTextureSize(TextureFormat textureFormat, GraphicsPlatform platform, GraphicsProfile graphicsProfile, Size2 textureSizeInput, Size2 textureSizeRequested, bool generateMipmaps, ILogger logger)
        {
            var textureSize = textureSizeRequested;

            // compressed DDS files has to have a size multiple of 4.
            if (platform == GraphicsPlatform.Direct3D11 && textureFormat == TextureFormat.Compressed
                && ((textureSizeRequested.Width % 4) != 0 || (textureSizeRequested.Height % 4) != 0))
            {
                textureSize.Width = unchecked((int)(((uint)(textureSizeRequested.Width + 3)) & ~(uint)3));
                textureSize.Height = unchecked((int)(((uint)(textureSizeRequested.Height + 3)) & ~(uint)3));
            }

            var maxTextureSize = 0;

            // determine if the desired size if valid depending on the graphics profile
            switch (graphicsProfile)
            {
                case GraphicsProfile.Level_9_1:
                case GraphicsProfile.Level_9_2:
                case GraphicsProfile.Level_9_3:
                    if (generateMipmaps && (!IsPowerOfTwo(textureSize.Width) || !IsPowerOfTwo(textureSize.Height)))
                    {
                        // TODO: TEMPORARY SETUP A MAX TEXTURE OF 1024. THIS SHOULD BE SPECIFIED DONE IN THE ASSET INSTEAD
                        textureSize.Width = Math.Min(MathUtil.NextPowerOfTwo(textureSize.Width), 1024);
                        textureSize.Height = Math.Min(MathUtil.NextPowerOfTwo(textureSize.Height), 1024);
                        logger.Warning("Graphic profiles 9.1/9.2/9.3 do not support mipmaps with textures that are not power of 2. Asset is automatically resized to " + textureSize);
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
            }

            if (textureSize.Width > maxTextureSize || textureSize.Height > maxTextureSize)
            {
                logger.Error("Graphic profile {0} do not support texture with resolution {2} x {3} because it is larger than {1}. " +
                             "Please reduce texture size or upgrade your graphic profile.", graphicsProfile, maxTextureSize, textureSize.Width, textureSize.Height);
                return new Size2(Math.Min(textureSize.Width, maxTextureSize), Math.Min(textureSize.Height, maxTextureSize));
            }

            return textureSize;
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
        /// <param name="parameters">The conversion request parameters</param>
        /// <param name="graphicsPlatform">The graphics platform</param>
        /// <param name="graphicsProfile">The graphics profile</param>
        /// <param name="imageSize">The texture output size</param>
        /// <param name="inputImageFormat">The pixel format of the input image</param>
        /// <param name="textureAsset">The texture asset</param>
        /// <returns>The pixel format to use as output</returns>
        public static PixelFormat DetermineOutputFormat(TextureAsset textureAsset, TextureConvertParameters parameters, Int2 imageSize, PixelFormat inputImageFormat, PlatformType platform, GraphicsPlatform graphicsPlatform,
            GraphicsProfile graphicsProfile)
        {
            if (textureAsset.SRgb && ((int)parameters.GraphicsProfile < (int)GraphicsProfile.Level_9_2 && parameters.GraphicsPlatform != GraphicsPlatform.Direct3D11))
                throw new NotSupportedException("sRGB is not supported on OpenGl profile level {0}".ToFormat(parameters.GraphicsProfile));

            var hint = textureAsset.Hint;

            // Default output format
            var outputFormat = PixelFormat.R8G8B8A8_UNorm;
            switch (textureAsset.Format)
            {
                case TextureFormat.Compressed:
                    switch (parameters.Platform)
                    {
                        case PlatformType.Android:
                            if (inputImageFormat.IsHDR())
                            {
                                outputFormat = inputImageFormat;
                            }
                            else if (textureAsset.SRgb)
                            {
                                outputFormat = PixelFormat.R8G8B8A8_UNorm_SRgb;
                            }
                            else
                            {
                                switch (graphicsProfile)
                                {
                                    case GraphicsProfile.Level_9_1:
                                    case GraphicsProfile.Level_9_2:
                                    case GraphicsProfile.Level_9_3:
                                        outputFormat = textureAsset.Alpha == AlphaFormat.None ? PixelFormat.ETC1 : PixelFormat.R8G8B8A8_UNorm;
                                        break;
                                    case GraphicsProfile.Level_10_0:
                                    case GraphicsProfile.Level_10_1:
                                    case GraphicsProfile.Level_11_0:
                                    case GraphicsProfile.Level_11_1:
                                    case GraphicsProfile.Level_11_2:
                                        // GLES3.0 starting from Level_10_0, this profile enables ETC2 compression on Android
                                        outputFormat = textureAsset.Alpha == AlphaFormat.None ? PixelFormat.ETC1 : PixelFormat.ETC2_RGBA;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException("graphicsProfile");
                                }
                            }
                            break;
                        case PlatformType.iOS:
                            // PVRTC works only for square POT textures
                            if (textureAsset.SRgb)
                            {
                                outputFormat = PixelFormat.R8G8B8A8_UNorm_SRgb;
                            }
                            else if (SupportPVRTC(imageSize))
                            {
                                switch (textureAsset.Alpha)
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
                            switch (parameters.GraphicsPlatform)
                            {
                                case GraphicsPlatform.Direct3D11:


                                    // https://msdn.microsoft.com/en-us/library/windows/desktop/hh308955%28v=vs.85%29.aspx
                                    // http://www.reedbeta.com/blog/2012/02/12/understanding-bcn-texture-compression-formats/
                                    // ----------------------------------------------    ----------------------------------------------------                        ---    ---------------------------------
                                    // Source data                                       Minimum required data compression resolution 	                  Recommended format	Minimum supported feature level
                                    // ----------------------------------------------    ----------------------------------------------------                        ---    ---------------------------------
                                    // Three-channel color with alpha channel            Three color channels (5 bits:6 bits:5 bits), with 0 or 1 bit(s) of alpha    BC1    Direct3D 9.1     (color maps, cutout color maps - 1 bit alpha, normal maps if memory is tight)
                                    // Three-channel color with alpha channel            Three color channels (5 bits:6 bits:5 bits), with 4 bits of alpha           BC2    Direct3D 9.1     (idem)
                                    // Three-channel color with alpha channel            Three color channels (5 bits:6 bits:5 bits) with 8 bits of alpha            BC3    Direct3D 9.1     (color maps with alpha, packing color and mono maps together)
                                    // One-channel color                                 One color channel (8 bits)                                                  BC4    Direct3D 10      (Height maps, gloss maps, font atlases, any gray scales image)
                                    // Two-channel color	                             Two color channels (8 bits:8 bits)                                          BC5    Direct3D 10      (Tangent space normal maps)
                                    // Three-channel high dynamic range (HDR) color      Three color channels (16 bits:16 bits:16 bits) in "half" floating point*    BC6H   Direct3D 11      (HDR images)
                                    // Three-channel color, alpha channel optional       Three color channels (4 to 7 bits per channel) with 0 to 8 bits of alpha    BC7    Direct3D 11      (High quality color maps, Color maps with full alpha)

                                    switch (textureAsset.Alpha)
                                    {
                                        case AlphaFormat.None:
                                        case AlphaFormat.Mask:
                                            // DXT1 handles 1-bit alpha channel
                                            outputFormat = textureAsset.SRgb ? PixelFormat.BC1_UNorm_SRgb : PixelFormat.BC1_UNorm;
                                            break;
                                        case AlphaFormat.Explicit:
                                            // DXT3 is good at sharp alpha transitions
                                            outputFormat = textureAsset.SRgb ? PixelFormat.BC2_UNorm_SRgb : PixelFormat.BC2_UNorm;
                                            break;
                                        case AlphaFormat.Interpolated:
                                            // DXT5 is good at alpha gradients
                                            outputFormat = textureAsset.SRgb ? PixelFormat.BC3_UNorm_SRgb : PixelFormat.BC3_UNorm;
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }

                                    // Overrides the format when profile is >= 10.0
                                    // Support some specific optimized formats based on the hint or input type
                                    if (parameters.GraphicsProfile >= GraphicsProfile.Level_10_0)
                                    {
                                        if (hint == TextureHint.NormalMap)
                                        {
                                            outputFormat = PixelFormat.BC5_SNorm;
                                        }
                                        else if (hint == TextureHint.Grayscale)
                                        {
                                            outputFormat = PixelFormat.BC4_UNorm;
                                        }
                                        else if (inputImageFormat.IsHDR())
                                        {
                                            // BC6H is too slow to compile
                                            //outputFormat = parameters.GraphicsProfile >= GraphicsProfile.Level_11_0 && textureAsset.Alpha == AlphaFormat.None ? PixelFormat.BC6H_Uf16 : inputImageFormat;
                                            outputFormat = inputImageFormat;
                                        }
                                        // TODO support the BC6/BC7 but they are so slow to compile that we can't use them right now
                                    }
                                    break;
                                case GraphicsPlatform.OpenGLES: // OpenGLES on Windows
                                    if (inputImageFormat.IsHDR())
                                    {
                                        outputFormat = inputImageFormat;
                                    }
                                    else if (textureAsset.SRgb)
                                    {
                                        outputFormat = PixelFormat.R8G8B8A8_UNorm_SRgb;
                                    }
                                    else
                                    {
                                        switch (graphicsProfile)
                                        {
                                            case GraphicsProfile.Level_9_1:
                                            case GraphicsProfile.Level_9_2:
                                            case GraphicsProfile.Level_9_3:
                                                outputFormat = textureAsset.Alpha == AlphaFormat.None ? PixelFormat.ETC1 : PixelFormat.R8G8B8A8_UNorm;
                                                break;
                                            case GraphicsProfile.Level_10_0:
                                            case GraphicsProfile.Level_10_1:
                                            case GraphicsProfile.Level_11_0:
                                            case GraphicsProfile.Level_11_1:
                                            case GraphicsProfile.Level_11_2:
                                                // GLES3.0 starting from Level_10_0, this profile enables ETC2 compression on Android
                                                outputFormat = textureAsset.Alpha == AlphaFormat.None ? PixelFormat.ETC1 : PixelFormat.ETC2_RGBA;
                                                break;
                                            default:
                                                throw new ArgumentOutOfRangeException("graphicsProfile");
                                        }
                                    }
                                    break;
                                default:
                                    // OpenGL on Windows
                                    // TODO: Need to handle OpenGL Desktop compression
                                    outputFormat = textureAsset.SRgb ? PixelFormat.R8G8B8A8_UNorm_SRgb : PixelFormat.R8G8B8A8_UNorm;
                                    break;
                            }
                            break;
                        default:
                            throw new NotSupportedException("Platform " + parameters.Platform + " is not supported by TextureTool");
                    }
                    break;
                case TextureFormat.Color16Bits:
                    if (textureAsset.SRgb)
                    {
                        outputFormat = PixelFormat.R8G8B8A8_UNorm_SRgb;
                    }
                    else
                    {
                        if (textureAsset.Alpha == AlphaFormat.None)
                        {
                            outputFormat = PixelFormat.B5G6R5_UNorm;
                        }
                        else if (textureAsset.Alpha == AlphaFormat.Mask)
                        {
                            outputFormat = PixelFormat.B5G5R5A1_UNorm;
                        }
                    }
                    break;
                case TextureFormat.Color32Bits:
                    if (textureAsset.SRgb)
                    {
                        outputFormat = PixelFormat.R8G8B8A8_UNorm_SRgb;
                    }
                    break;
                case TextureFormat.AsIs:
                    outputFormat = inputImageFormat;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return outputFormat;
        }

        public static ResultStatus ImportAndSaveTextureImage(UFile sourcePath, string outputUrl, TextureAsset textureAsset, TextureConvertParameters parameters, CancellationToken cancellationToken, Logger logger)
        {
            var assetManager = new AssetManager();

            using (var texTool = new TextureTool())
            using (var texImage = texTool.Load(sourcePath, textureAsset.SRgb))
            {
                // Apply transformations
                texTool.Decompress(texImage, textureAsset.SRgb);

                if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                    return ResultStatus.Cancelled;

                var fromSize =  new Size2(texImage.Width, texImage.Height);
                var targetSize = new Size2((int)textureAsset.Width, (int)textureAsset.Height);

                // Resize the image
                if (textureAsset.IsSizeInPercentage)
                {
                    targetSize = new Size2((int)(fromSize.Width * (float)textureAsset.Width / 100.0f), (int)(fromSize.Height * (float) textureAsset.Height / 100.0f));
                }

                // Find the target size
                targetSize = FindBestTextureSize(textureAsset.Format, parameters.GraphicsPlatform, parameters.GraphicsProfile, fromSize, targetSize, textureAsset.GenerateMipmaps, logger);

                // Resize the image only if needed
                if (targetSize != fromSize)
                {
                    texTool.Resize(texImage, targetSize.Width, targetSize.Height, Filter.Rescaling.Lanczos3);
                }

                if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                    return ResultStatus.Cancelled;

                // texture size is now determined, we can cache it
                var textureSize = new Int2(texImage.Width, texImage.Height);

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
                {
                    var boxFilteringIsSupported = texImage.Format != PixelFormat.B8G8R8A8_UNorm_SRgb || (IsPowerOfTwo(textureSize.X) && IsPowerOfTwo(textureSize.Y));
                    texTool.GenerateMipMaps(texImage, boxFilteringIsSupported? Filter.MipMapGeneration.Box: Filter.MipMapGeneration.Linear);
                }
                
                if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                    return ResultStatus.Cancelled;


                // Convert/Compress to output format
                // TODO: Change alphaFormat depending on actual image content (auto-detection)?
                var outputFormat = DetermineOutputFormat(textureAsset, parameters, textureSize, texImage.Format, parameters.Platform, parameters.GraphicsPlatform, parameters.GraphicsProfile);
                texTool.Compress(texImage, outputFormat, (TextureConverter.Requests.TextureQuality)parameters.TextureQuality);

                if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                    return ResultStatus.Cancelled;


                // Save the texture
                if (parameters.SeparateAlpha)
                {
                    //TextureAlphaComponentSplitter.CreateAndSaveSeparateTextures(texTool, texImage, outputUrl, textureAsset.GenerateMipmaps);
                }
                else
                {
                    using (var outputImage = texTool.ConvertToParadoxImage(texImage))
                    {
                        if (cancellationToken.IsCancellationRequested) // abort the process if cancellation is demanded
                            return ResultStatus.Cancelled;

                        assetManager.Save(outputUrl, outputImage.ToSerializableVersion());

                        logger.Info("Compression successful [{3}] to ({0}x{1},{2})", outputImage.Description.Width, outputImage.Description.Height, outputImage.Description.Format, outputUrl);
                    }
                }
            }

            return ResultStatus.Successful;
        }
    }
}