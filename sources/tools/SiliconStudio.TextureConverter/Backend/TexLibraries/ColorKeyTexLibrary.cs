// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.TextureConverter.Requests;

namespace SiliconStudio.TextureConverter.TexLibraries
{
    /// <summary>
    /// Allows the creation and manipulation of texture atlas.
    /// </summary>
    internal class ColorKeyTexLibrary : ITexLibrary
    {
        private readonly static Logger Log = GlobalLogger.GetLogger("ColorKeyTexLibrary");

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorKeyTexLibrary"/> class.
        /// </summary>
        public ColorKeyTexLibrary() { }

        public bool CanHandleRequest(TexImage image, IRequest request) => CanHandleRequest(image.Format, request);

        public bool CanHandleRequest(PixelFormat format, IRequest request) => request.Type == RequestType.ColorKey;

        public void Execute(TexImage image, IRequest request)
        {
            switch (request.Type)
            {
                case RequestType.ColorKey:
                    ApplyColorKey(image, (ColorKeyRequest)request);
                    break;
                default:
                    Log.Error("ColorKeyTexLibrary can't handle this request: " + request.Type);
                    throw new TextureToolsException("ColorKeyTexLibrary can't handle this request: " + request.Type);
            }
        }

        public void Dispose(TexImage image)
        {
            Marshal.FreeHGlobal(image.Data);
        }

        public void Dispose() { }

        public void StartLibrary(TexImage image) { }

        public void EndLibrary(TexImage image) { }

        public bool SupportBGRAOrder()
        {
            return true;
        }

        public unsafe void ApplyColorKey(TexImage image, ColorKeyRequest request)
        {
            Log.Info($"Apply color key [{request.ColorKey}]");

            var colorKey = request.ColorKey;
            var rowPtr = image.Data;
            if (image.Format == PixelFormat.R8G8B8A8_UNorm || image.Format == PixelFormat.R8G8B8A8_UNorm_SRgb)
            {
                for (int i = 0; i < image.Height; i++)
                {
                    var colors = (Core.Mathematics.Color*)rowPtr;
                    for (int x = 0; x < image.Width; x++)
                    {
                        if (colors[x] == colorKey)
                        {
                            colors[x] = Core.Mathematics.Color.Transparent;
                        }
                    }
                    rowPtr = IntPtr.Add(rowPtr, image.RowPitch);
                }
            }
            else if (image.Format == PixelFormat.B8G8R8A8_UNorm || image.Format == PixelFormat.B8G8R8A8_UNorm_SRgb)
            {
                var rgbaColorKey = colorKey.ToRgba();
                for (int i = 0; i < image.Height; i++)
                {
                    var colors = (Core.Mathematics.ColorBGRA*)rowPtr;
                    for (int x = 0; x < image.Width; x++)
                    {
                        if (colors[x].ToRgba() == rgbaColorKey)
                        {
                            colors[x] = Core.Mathematics.Color.Transparent;
                        }
                    }
                    rowPtr = IntPtr.Add(rowPtr, image.RowPitch);
                }
            }
        }
    }
}
