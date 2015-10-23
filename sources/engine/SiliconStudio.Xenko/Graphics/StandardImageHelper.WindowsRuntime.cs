// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// This class is responsible to provide image loader for png, gif, bmp.
    /// TODO: Replace using System.Drawing, as it is not available on all platforms (not on Windows 8/WP8).
    /// </summary>
    partial class StandardImageHelper
    {
        public unsafe static Image LoadFromMemory(IntPtr pSource, int size, bool makeACopy, GCHandle? handle)
        {
            return WICHelper.LoadFromWICMemory(pSource, size, makeACopy, handle);
        }

        public static void SaveGifFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            WICHelper.SaveGifToWICMemory(pixelBuffers, count, description, imageStream);
        }

        public static void SaveTiffFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            WICHelper.SaveTiffToWICMemory(pixelBuffers, count, description, imageStream);
        }

        public static void SaveBmpFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            WICHelper.SaveBmpToWICMemory(pixelBuffers, count, description, imageStream);
        }

        public static void SaveJpgFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            WICHelper.SaveJpgToWICMemory(pixelBuffers, count, description, imageStream);
        }

        public static void SavePngFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            WICHelper.SavePngToWICMemory(pixelBuffers, count, description, imageStream);
        }

        public static void SaveWmpFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            throw new NotImplementedException();
        }
    }
}
#endif