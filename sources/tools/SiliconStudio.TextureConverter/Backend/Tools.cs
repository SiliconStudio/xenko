// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.TextureConverter
{
    /// <summary>
    /// Provides general methods used by the libraries.
    /// </summary>
    internal class Tools
    {
        /// <summary>
        /// Computes the pitch.
        /// </summary>
        /// <param name="fmt">The format.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="rowPitch">output row pitch.</param>
        /// <param name="slicePitch">output slice pitch.</param>
        public static void ComputePitch(PixelFormat fmt, int width, int height, out int rowPitch, out int slicePitch)
        {
            int widthCount = width;
            int heightCount = height;

            int bpp = (int)GetBPP(fmt);

            if (IsCompressed(fmt))
            {
                widthCount = Math.Max(1, (width + 3) / 4);
                heightCount = Math.Max(1, (height + 3) / 4);
                rowPitch = widthCount * bpp;

                slicePitch = rowPitch * heightCount;
            }
            else if (fmt.IsPacked())
            {
                rowPitch = ((width + 1) >> 1) * 4;

                slicePitch = rowPitch * height;
            }
            else
            {
                if (bpp == 0)
                    bpp = fmt.SizeInBits();

                rowPitch = (width * bpp + 7) / 8;
                slicePitch = rowPitch * height;
            }
        }


        /// <summary>
        /// Gets the BPP of the specified format.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>the bytes per pixel of the specified format</returns>
        public static uint GetBPP(PixelFormat format)
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

                case PixelFormat.None:
                default: return 0;
            }
        }


        /// <summary>
        /// Determines whether two different formats are in same channel order.
        /// </summary>
        /// <param name="format1">The format1.</param>
        /// <param name="format2">The format2.</param>
        /// <returns>
        ///   <c>true</c> if the formats are in the same channel order; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInSameChannelOrder(PixelFormat format1, PixelFormat format2)
        {
            return IsInBGRAOrder(format1) && IsInBGRAOrder(format2) || IsInRGBAOrder(format1) && IsInRGBAOrder(format2);
        }


        /// <summary>
        /// Determines whether the specified format is in RGBA order.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        ///   <c>true</c> if the specified format is in RGBA order; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInRGBAOrder(PixelFormat format)
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
        public static bool IsInBGRAOrder(PixelFormat format)
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
        /// Determines whether the specified format is compressed.
        /// </summary>
        /// <param name="fmt">The format.</param>
        /// <returns>
        ///   <c>true</c> if the specified format is compressed; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCompressed(PixelFormat fmt)
        {
            switch (fmt)
            {
                case PixelFormat.BC1_Typeless:
                case PixelFormat.BC1_UNorm:
                case PixelFormat.BC1_UNorm_SRgb:
                case PixelFormat.BC2_Typeless:
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC2_UNorm_SRgb:
                case PixelFormat.BC3_Typeless:
                case PixelFormat.BC3_UNorm:
                case PixelFormat.BC3_UNorm_SRgb:
                case PixelFormat.BC4_Typeless:
                case PixelFormat.BC4_UNorm:
                case PixelFormat.BC4_SNorm:
                case PixelFormat.BC5_Typeless:
                case PixelFormat.BC5_UNorm:
                case PixelFormat.BC5_SNorm:
                case PixelFormat.BC6H_Typeless:
                case PixelFormat.BC6H_Uf16:
                case PixelFormat.BC6H_Sf16:
                case PixelFormat.BC7_Typeless:
                case PixelFormat.BC7_UNorm:
                case PixelFormat.BC7_UNorm_SRgb:
                case PixelFormat.PVRTC_2bpp_RGB:
                case PixelFormat.PVRTC_2bpp_RGBA:
                case PixelFormat.PVRTC_4bpp_RGB:
                case PixelFormat.PVRTC_4bpp_RGBA:
                case PixelFormat.PVRTC_II_2bpp:
                case PixelFormat.PVRTC_II_4bpp:
                case PixelFormat.ETC1:
                case PixelFormat.ETC2_RGB:
                case PixelFormat.ETC2_RGBA:
                case PixelFormat.ETC2_RGB_A1:
                case PixelFormat.EAC_R11_Unsigned:
                case PixelFormat.EAC_R11_Signed:
                case PixelFormat.EAC_RG11_Unsigned:
                case PixelFormat.EAC_RG11_Signed:
                case PixelFormat.ATC_RGB:
                case PixelFormat.ATC_RGBA_Explicit:
                case PixelFormat.ATC_RGBA_Interpolated:
                    return true;
                default:
                    return false;
            }
        }
    }
}
