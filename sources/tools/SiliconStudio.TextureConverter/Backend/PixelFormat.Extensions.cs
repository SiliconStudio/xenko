// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.TextureConverter
{
    public static class PixelFormatExtensions
    {
        private static readonly Dictionary<PixelFormat, PixelFormat> SRgbConvertion = new Dictionary<PixelFormat, PixelFormat>
        {
            { PixelFormat.R8G8B8A8_UNorm_SRgb, PixelFormat.R8G8B8A8_UNorm },
            { PixelFormat.R8G8B8A8_UNorm, PixelFormat.R8G8B8A8_UNorm_SRgb },
            { PixelFormat.BC1_UNorm_SRgb, PixelFormat.BC1_UNorm },
            { PixelFormat.BC1_UNorm, PixelFormat.BC1_UNorm_SRgb },
            { PixelFormat.BC2_UNorm_SRgb, PixelFormat.BC2_UNorm },
            { PixelFormat.BC2_UNorm, PixelFormat.BC2_UNorm_SRgb },
            { PixelFormat.BC3_UNorm_SRgb, PixelFormat.BC3_UNorm },
            { PixelFormat.BC3_UNorm, PixelFormat.BC3_UNorm_SRgb },
            { PixelFormat.B8G8R8A8_UNorm_SRgb, PixelFormat.B8G8R8A8_UNorm },
            { PixelFormat.B8G8R8A8_UNorm, PixelFormat.B8G8R8A8_UNorm_SRgb },
            { PixelFormat.B8G8R8X8_UNorm_SRgb, PixelFormat.B8G8R8X8_UNorm },
            { PixelFormat.B8G8R8X8_UNorm, PixelFormat.B8G8R8X8_UNorm_SRgb },
            { PixelFormat.BC7_UNorm_SRgb, PixelFormat.BC7_UNorm },
            { PixelFormat.BC7_UNorm, PixelFormat.BC7_UNorm_SRgb },
        };
        
        /// <summary>
        /// Gets the BPP of the specified format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>the bytes per pixel of the specified format</returns>
        public static uint GetBPP(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32G32B32A32_Typeless:
                case PixelFormat.R32G32B32A32_Float:
                case PixelFormat.R32G32B32A32_UInt:
                case PixelFormat.R32G32B32A32_SInt:
                    return 128;

                case PixelFormat.R32G32B32_Typeless:
                case PixelFormat.R32G32B32_Float:
                case PixelFormat.R32G32B32_UInt:
                case PixelFormat.R32G32B32_SInt:
                    return 96;

                case PixelFormat.R16G16B16A16_Typeless:
                case PixelFormat.R16G16B16A16_Float:
                case PixelFormat.R16G16B16A16_UNorm:
                case PixelFormat.R16G16B16A16_UInt:
                case PixelFormat.R16G16B16A16_SNorm:
                case PixelFormat.R16G16B16A16_SInt:
                case PixelFormat.R32G32_Typeless:
                case PixelFormat.R32G32_Float:
                case PixelFormat.R32G32_UInt:
                case PixelFormat.R32G32_SInt:
                case PixelFormat.R32G8X24_Typeless:
                case PixelFormat.D32_Float_S8X24_UInt:
                case PixelFormat.R32_Float_X8X24_Typeless:
                case PixelFormat.X32_Typeless_G8X24_UInt:
                    return 64;

                case PixelFormat.R10G10B10A2_Typeless:
                case PixelFormat.R10G10B10A2_UNorm:
                case PixelFormat.R10G10B10A2_UInt:
                case PixelFormat.R11G11B10_Float:
                case PixelFormat.R8G8B8A8_Typeless:
                case PixelFormat.R8G8B8A8_UNorm:
                case PixelFormat.R8G8B8A8_UNorm_SRgb:
                case PixelFormat.R8G8B8A8_UInt:
                case PixelFormat.R8G8B8A8_SNorm:
                case PixelFormat.R8G8B8A8_SInt:
                case PixelFormat.R16G16_Typeless:
                case PixelFormat.R16G16_Float:
                case PixelFormat.R16G16_UNorm:
                case PixelFormat.R16G16_UInt:
                case PixelFormat.R16G16_SNorm:
                case PixelFormat.R16G16_SInt:
                case PixelFormat.R32_Typeless:
                case PixelFormat.D32_Float:
                case PixelFormat.R32_Float:
                case PixelFormat.R32_UInt:
                case PixelFormat.R32_SInt:
                case PixelFormat.R24G8_Typeless:
                case PixelFormat.D24_UNorm_S8_UInt:
                case PixelFormat.R24_UNorm_X8_Typeless:
                case PixelFormat.X24_Typeless_G8_UInt:
                case PixelFormat.R9G9B9E5_Sharedexp:
                case PixelFormat.R8G8_B8G8_UNorm:
                case PixelFormat.G8R8_G8B8_UNorm:
                case PixelFormat.B8G8R8A8_UNorm:
                case PixelFormat.B8G8R8X8_UNorm:
                case PixelFormat.R10G10B10_Xr_Bias_A2_UNorm:
                case PixelFormat.B8G8R8A8_Typeless:
                case PixelFormat.B8G8R8A8_UNorm_SRgb:
                case PixelFormat.B8G8R8X8_Typeless:
                case PixelFormat.B8G8R8X8_UNorm_SRgb:
                    return 32;

                case PixelFormat.R8G8_Typeless:
                case PixelFormat.R8G8_UNorm:
                case PixelFormat.R8G8_UInt:
                case PixelFormat.R8G8_SNorm:
                case PixelFormat.R8G8_SInt:
                case PixelFormat.R16_Typeless:
                case PixelFormat.R16_Float:
                case PixelFormat.D16_UNorm:
                case PixelFormat.R16_UNorm:
                case PixelFormat.R16_UInt:
                case PixelFormat.R16_SNorm:
                case PixelFormat.R16_SInt:
                case PixelFormat.B5G6R5_UNorm:
                case PixelFormat.B5G5R5A1_UNorm:
                    return 16;

                case PixelFormat.R8_Typeless:
                case PixelFormat.R8_UNorm:
                case PixelFormat.R8_UInt:
                case PixelFormat.R8_SNorm:
                case PixelFormat.R8_SInt:
                case PixelFormat.A8_UNorm:
                    return 8;


                case PixelFormat.BC1_Typeless:
                case PixelFormat.BC1_UNorm:
                case PixelFormat.BC1_UNorm_SRgb:
                case PixelFormat.BC4_Typeless:
                case PixelFormat.BC4_UNorm:
                case PixelFormat.BC4_SNorm:
                    return 8;


                case PixelFormat.BC2_Typeless:
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC2_UNorm_SRgb:
                case PixelFormat.BC3_Typeless:
                case PixelFormat.BC3_UNorm:
                case PixelFormat.BC3_UNorm_SRgb:
                case PixelFormat.BC5_Typeless:
                case PixelFormat.BC5_UNorm:
                case PixelFormat.BC5_SNorm:
                case PixelFormat.BC6H_Typeless:
                case PixelFormat.BC6H_Uf16:
                case PixelFormat.BC6H_Sf16:
                case PixelFormat.BC7_Typeless:
                case PixelFormat.BC7_UNorm:
                case PixelFormat.BC7_UNorm_SRgb:
                    return 16;


                case PixelFormat.PVRTC_2bpp_RGB:
                    return 3;
                case PixelFormat.PVRTC_2bpp_RGBA:
                case PixelFormat.PVRTC_II_2bpp:
                    return 4;
                case PixelFormat.PVRTC_4bpp_RGB:
                    return 6;
                case PixelFormat.PVRTC_4bpp_RGBA:
                case PixelFormat.PVRTC_II_4bpp:
                    return 8;


                case PixelFormat.ETC1:
                case PixelFormat.ETC2_RGBA:
                case PixelFormat.ETC2_RGB_A1:
                case PixelFormat.EAC_R11_Unsigned:
                case PixelFormat.EAC_R11_Signed:
                case PixelFormat.EAC_RG11_Unsigned:
                case PixelFormat.EAC_RG11_Signed:
                    return 8;
                case PixelFormat.ETC2_RGB:
                    return 6;


                case PixelFormat.ATC_RGB:
                    return 12;
                case PixelFormat.ATC_RGBA_Explicit:
                case PixelFormat.ATC_RGBA_Interpolated:
                    return 16;

                case PixelFormat.R1_UNorm:
                    return 1;

                default: 
                    return 0;
            }
        }

        /// <summary>
        /// Determines whether the specified format is in RGBA order.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        ///   <c>true</c> if the specified format is in RGBA order; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInRGBAOrder(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32G32B32A32_Typeless:
                case PixelFormat.R32G32B32A32_Float:
                case PixelFormat.R32G32B32A32_UInt:
                case PixelFormat.R32G32B32A32_SInt:
                case PixelFormat.R32G32B32_Typeless:
                case PixelFormat.R32G32B32_Float:
                case PixelFormat.R32G32B32_UInt:
                case PixelFormat.R32G32B32_SInt:
                case PixelFormat.R16G16B16A16_Typeless:
                case PixelFormat.R16G16B16A16_Float:
                case PixelFormat.R16G16B16A16_UNorm:
                case PixelFormat.R16G16B16A16_UInt:
                case PixelFormat.R16G16B16A16_SNorm:
                case PixelFormat.R16G16B16A16_SInt:
                case PixelFormat.R32G32_Typeless:
                case PixelFormat.R32G32_Float:
                case PixelFormat.R32G32_UInt:
                case PixelFormat.R32G32_SInt:
                case PixelFormat.R32G8X24_Typeless:
                case PixelFormat.R10G10B10A2_Typeless:
                case PixelFormat.R10G10B10A2_UNorm:
                case PixelFormat.R10G10B10A2_UInt:
                case PixelFormat.R11G11B10_Float:
                case PixelFormat.R8G8B8A8_Typeless:
                case PixelFormat.R8G8B8A8_UNorm:
                case PixelFormat.R8G8B8A8_UNorm_SRgb:
                case PixelFormat.R8G8B8A8_UInt:
                case PixelFormat.R8G8B8A8_SNorm:
                case PixelFormat.R8G8B8A8_SInt:
                    return true;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Determines whether the specified format is in BGRA order.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        ///   <c>true</c> if the specified format is in BGRA order; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInBGRAOrder(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.B8G8R8A8_UNorm:
                case PixelFormat.B8G8R8X8_UNorm:
                case PixelFormat.B8G8R8A8_Typeless:
                case PixelFormat.B8G8R8A8_UNorm_SRgb:
                case PixelFormat.B8G8R8X8_Typeless:
                case PixelFormat.B8G8R8X8_UNorm_SRgb:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determine if the format has an equivalent sRGB format.
        /// </summary>
        /// <param name="format">the non-sRGB format</param>
        /// <returns>true if the format has an sRGB equivalent</returns>
        public static bool HasSRgbEquivalent(this PixelFormat format)
        {
            if (format.IsSRgb())
                throw new ArgumentException("The '{0}' format is already an sRGB format".ToFormat(format));

            return SRgbConvertion.ContainsKey(format);
        }

        /// <summary>
        /// Determine if the format has an equivalent non-sRGB format.
        /// </summary>
        /// <param name="format">the sRGB format</param>
        /// <returns>true if the format has an non-sRGB equivalent</returns>
        public static bool HasNonSRgbEquivalent(this PixelFormat format)
        {
            if (!format.IsSRgb())
                throw new ArgumentException("The provided format is not a sRGB format");

            return SRgbConvertion.ContainsKey(format);
        }

        /// <summary>
        /// Find the equivalent sRGB format to the provided format.
        /// </summary>
        /// <param name="format">The non sRGB format.</param>
        /// <returns>
        /// The equivalent sRGB format if any, the provided format else.
        /// </returns>
        public static PixelFormat ToSRgb(this PixelFormat format)
        {
            if (format.IsSRgb() || !SRgbConvertion.ContainsKey(format))
                return format;

            return SRgbConvertion[format];
        }

        /// <summary>
        /// Find the equivalent non sRGB format to the provided sRGB format.
        /// </summary>
        /// <param name="format">The non sRGB format.</param>
        /// <returns>
        /// The equivalent non sRGB format if any, the provided format else.
        /// </returns>
        public static PixelFormat ToNonSRgb(this PixelFormat format)
        {
            if (!format.IsSRgb() || !SRgbConvertion.ContainsKey(format))
                return format;

            return SRgbConvertion[format];
        }
    }
}