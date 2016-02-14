// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using System;
using SharpVulkan;

namespace SiliconStudio.Xenko.Graphics
{
    internal static class VulkanConvertExtensions
    {
        public static CompareOperation ConvertComparisonFunction(CompareFunction comparison)
        {
            switch (comparison)
            {
                case CompareFunction.Always:
                    return CompareOperation.Always;
                case CompareFunction.Never:
                    return CompareOperation.Never;
                case CompareFunction.Equal:
                    return CompareOperation.Equal;
                case CompareFunction.Greater:
                    return CompareOperation.Greater;
                case CompareFunction.GreaterEqual:
                    return CompareOperation.GreaterOrEqual;
                case CompareFunction.Less:
                    return CompareOperation.Less;
                case CompareFunction.LessEqual:
                    return CompareOperation.LessOrEqual;
                case CompareFunction.NotEqual:
                    return CompareOperation.NotEqual;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static SharpVulkan.StencilOperation ConvertStencilOperation(StencilOperation operation)
        {
            switch (operation)
            {
                case StencilOperation.Decrement:
                    return SharpVulkan.StencilOperation.DecrementAndWrap;
                case StencilOperation.DecrementSaturation:
                    return SharpVulkan.StencilOperation.DecrementAndClamp;
                case StencilOperation.Increment:
                    return SharpVulkan.StencilOperation.IncrementAndWrap;
                case StencilOperation.IncrementSaturation:
                    return SharpVulkan.StencilOperation.IncrementAndClamp;
                case StencilOperation.Invert:
                    return SharpVulkan.StencilOperation.Invert;
                case StencilOperation.Keep:
                    return SharpVulkan.StencilOperation.Keep;
                case StencilOperation.Replace:
                    return SharpVulkan.StencilOperation.Replace;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static BlendOperation ConvertBlendFunction(BlendFunction blendFunction)
        {
            // TODO: Binary compatible
            switch (blendFunction)
            {
                case BlendFunction.Add:
                    return BlendOperation.Add;
                case BlendFunction.Subtract:
                    return BlendOperation.Subtract;
                case BlendFunction.ReverseSubtract:
                    return BlendOperation.ReverseSubtract;
                case BlendFunction.Max:
                    return BlendOperation.Max;
                case BlendFunction.Min:
                    return BlendOperation.Min;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static BlendFactor ConvertBlend(Blend blend)
        {
            switch (blend)
            {
                case Blend.BlendFactor:
                    return BlendFactor.ConstantColor;
                case Blend.DestinationAlpha:
                    return BlendFactor.DestinationAlpha;
                case Blend.DestinationColor:
                    return BlendFactor.DestinationColor;
                case Blend.InverseBlendFactor:
                    return BlendFactor.OneMinusConstantColor;
                case Blend.InverseDestinationAlpha:
                    return BlendFactor.OneMinusDestinationAlpha;
                case Blend.InverseDestinationColor:
                    return BlendFactor.OneMinusDestinationColor;
                case Blend.InverseSecondarySourceAlpha:
                    return BlendFactor.OneMinusSource1Alpha;
                case Blend.InverseSecondarySourceColor:
                    return BlendFactor.OneMinusSource1Color;
                case Blend.InverseSourceAlpha:
                    return BlendFactor.OneMinusSource1Alpha;
                case Blend.InverseSourceColor:
                    return BlendFactor.OneMinusSourceColor;
                case Blend.One:
                    return BlendFactor.One;
                case Blend.SecondarySourceAlpha:
                    return BlendFactor.Source1Alpha;
                case Blend.SecondarySourceColor:
                    return BlendFactor.Source1Color;
                case Blend.SourceAlpha:
                    return BlendFactor.SourceAlpha;
                case Blend.SourceAlphaSaturate:
                    return BlendFactor.SourceAlphaSaturate;
                case Blend.SourceColor:
                    return BlendFactor.SourceColor;
                case Blend.Zero:
                    return BlendFactor.Zero;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Format ConvertPixelFormat(PixelFormat inputFormat)
        {
            Format format;
            int pixelSize;
            bool compressed;

            ConvertPixelFormat(inputFormat, out format, out pixelSize, out compressed);
            return format;
        }

        public static void ConvertPixelFormat(PixelFormat inputFormat, out Format format, out int pixelSize, out bool compressed)
        {
            compressed = false;

            switch (inputFormat)
            {
                //case PixelFormat.A8_UNorm:
                //    format = Format.;
                //    pixelSize = 1;
                //    break;
                case PixelFormat.R8_UNorm:
                    format = Format.R8UNorm;
                    pixelSize = 1;
                    break;
                case PixelFormat.R8G8B8A8_UNorm:
                    format = Format.R8G8B8A8UNorm;
                    pixelSize = 4;
                    break;
                case PixelFormat.B8G8R8A8_UNorm:
                    format = Format.B8G8R8A8UNorm;
                    pixelSize = 4;
                    break;
                case PixelFormat.R8G8B8A8_UNorm_SRgb:
                    format = Format.R8G8B8A8SRgb;
                    pixelSize = 4;
                    break;
                case PixelFormat.B8G8R8A8_UNorm_SRgb:
                    format = Format.B8G8R8A8SRgb;
                    pixelSize = 4;
                    break;
                case PixelFormat.R16_Float:
                    format = Format.R16SFloat;
                    pixelSize = 2;
                    break;
                case PixelFormat.R16G16_Float:
                    format = Format.R16G16SFloat;
                    pixelSize = 4;
                    break;
                case PixelFormat.R16G16B16A16_Float:
                    format = Format.R16G16B16A16SFloat;
                    pixelSize = 8;
                    break;
                case PixelFormat.R32_UInt:
                    format = Format.R32UInt;
                    pixelSize = 4;
                    break;
                case PixelFormat.R32_Float:
                    format = Format.R32SFloat;
                    pixelSize = 4;
                    break;
                case PixelFormat.R32G32_Float:
                    format = Format.R32G32SFloat;
                    pixelSize = 8;
                    break;
                case PixelFormat.R32G32B32_Float:
                    format = Format.R32G32B32SFloat;
                    pixelSize = 12;
                    break;
                case PixelFormat.R32G32B32A32_Float:
                    format = Format.R32G32B32A32SFloat;
                    pixelSize = 16;
                    break;
                case PixelFormat.D16_UNorm:
                    format = Format.R16UNorm;
                    pixelSize = 2;
                    break;
                case PixelFormat.D24_UNorm_S8_UInt:
                    format = Format.D24UNormS8UInt;
                    pixelSize = 4;
                    break;
                // TODO: Temporary depth format (need to decide relation between RenderTarget1D and Texture)
                case PixelFormat.D32_Float:
                    format = Format.D32SFloat;
                    pixelSize = 4;
                    break;
                case PixelFormat.ETC1:
                case PixelFormat.ETC2_RGB: // ETC1 upper compatible
                    format = Format.Etc2R8G8B8UNormBlock;
                    compressed = true;
                    pixelSize = 1;  // 4bpp
                    break;
                case PixelFormat.ETC2_RGB_A1:
                    format = Format.Etc2R8G8B8A1UNormBlock;
                    compressed = true;
                    pixelSize = 1;  // 4bpp
                    break;
                case PixelFormat.ETC2_RGBA: // ETC2 + EAC
                    format = Format.Etc2R8G8B8A8UNormBlock;
                    compressed = true;
                    pixelSize = 2;  // 8bpp
                    break;
                case PixelFormat.EAC_R11_Unsigned:
                    format = Format.EacR11UNormBlock;
                    compressed = true;
                    pixelSize = 1;  // 4bpp
                    break;
                case PixelFormat.EAC_R11_Signed:
                    format = Format.EacR11SNormBlock;
                    compressed = true;
                    pixelSize = 1;  // 4bpp
                    break;
                case PixelFormat.EAC_RG11_Unsigned:
                    format = Format.EacR11G11UNormBlock;
                    compressed = true;
                    pixelSize = 2;  // 8bpp
                    break;
                case PixelFormat.EAC_RG11_Signed:
                    format = Format.EacR11G11SNormBlock;
                    compressed = true;
                    pixelSize = 2;  // 8bpp
                    break;
                case PixelFormat.BC1_UNorm:
                    format = Format.Bc1RgbaUNormBlock;
                    //format = Format.RAD_TEXTURE_FORMAT_DXT1_RGBA;
                    compressed = true;
                    pixelSize = 1;  // 4bpp
                    break;
                case PixelFormat.BC1_UNorm_SRgb:
                    format = Format.Bc1RgbaSRgbBlock;
                    //format = Format.RAD_TEXTURE_FORMAT_DXT1_RGBA_SRgb;
                    compressed = true;
                    pixelSize = 1;  // 4bpp
                    break;
                case PixelFormat.BC2_UNorm:
                    format = Format.Bc2UNormBlock;
                    compressed = true;
                    pixelSize = 2;  // 8bpp
                    break;
                case PixelFormat.BC2_UNorm_SRgb:
                    format = Format.Bc2SRgbBlock;
                    compressed = true;
                    pixelSize = 2;  // 8bpp
                    break;
                case PixelFormat.BC3_UNorm:
                    format = Format.Bc3UNormBlock;
                    compressed = true;
                    pixelSize = 2;  // 8bpp
                    break;
                case PixelFormat.BC3_UNorm_SRgb:
                    format = Format.Bc3SRgbBlock;
                    compressed = true;
                    pixelSize = 2;  // 8bpp
                    break;
                default:
                    throw new InvalidOperationException("Unsupported texture format");
            }
        }
    }
}

#endif