// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_ANDROID
using System;
using System.IO;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
using Android.Graphics;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// This class is responsible to provide image loader for png, gif, bmp.
    /// </summary>
    partial class StandardImageHelper
    {
        public unsafe static Image LoadFromMemory(IntPtr pSource, int size, bool makeACopy, GCHandle? handle)
        {
            using (var memoryStream = new UnmanagedMemoryStream((byte*)pSource, size))
            using (var bitmap = (Bitmap)BitmapFactory.DecodeStream(memoryStream))
            {
                var bitmapData = bitmap.LockPixels();
            
                var image = Image.New2D(bitmap.Width, bitmap.Height, 1, PixelFormat.B8G8R8A8_UNorm, 1, bitmap.RowBytes);
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
                // Directly load image as RGBA instead of BGRA, because OpenGL ES devices don't support it out of the box (extension).
                CopyMemoryBGRA(image.PixelBuffer[0].DataPointer, bitmapData, image.PixelBuffer[0].BufferStride);
#else
                Utilities.CopyMemory(image.PixelBuffer[0].DataPointer, bitmapData, image.PixelBuffer[0].BufferStride);
#endif
                bitmap.UnlockPixels();
            
                if (handle != null)
                    handle.Value.Free();
                else if (!makeACopy)
                    Utilities.FreeMemory(pSource);
            
                return image;
            }

        }

        public static void SaveGifFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            throw new NotImplementedException();
        }

        public static void SaveTiffFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            throw new NotImplementedException();
        }

        public static void SaveBmpFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            throw new NotImplementedException();
        }

        public static void SaveJpgFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            throw new NotImplementedException();
        }

        public static void SavePngFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            SaveFromMemory(pixelBuffers, count, description, imageStream, Bitmap.CompressFormat.Png);
        }

        public static void SaveWmpFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream)
        {
            throw new NotImplementedException();
        }

        private static void SaveFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, Stream imageStream, Bitmap.CompressFormat imageFormat)
        {
            var colors = pixelBuffers[0].GetPixels<int>();
            using (var bitmap = Bitmap.CreateBitmap(colors, description.Width, description.Height, Bitmap.Config.Argb8888))
            {
                bitmap.Compress(imageFormat, 0, imageStream);
            }
        }
    }
}
#endif