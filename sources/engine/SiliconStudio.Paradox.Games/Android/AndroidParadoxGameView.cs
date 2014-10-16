// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_ANDROID
using System;
using SiliconStudio.Paradox.Graphics;
using Android.Content;
using OpenTK.Graphics;
using OpenTK.Platform.Android;

namespace SiliconStudio.Paradox.Games.Android
{
    public class AndroidParadoxGameView : AndroidGameView
    {
        public EventHandler<EventArgs> OnPause;

        public AndroidParadoxGameView(Context context) : base(context)
        {
            RequestedBackBufferFormat = PixelFormat.R8G8B8A8_UNorm;
            RequestedDepthStencilFormat = PixelFormat.D24_UNorm_S8_UInt;
        }

        /// <summary>
        /// Gets or sets the requested back buffer format.
        /// </summary>
        /// <value>
        /// The requested back buffer format.
        /// </value>
        public PixelFormat RequestedBackBufferFormat { get; set; }

        /// <summary>
        /// Gets or sets the requested depth stencil format.
        /// </summary>
        /// <value>
        /// The requested depth stencil format.
        /// </value>
        public PixelFormat RequestedDepthStencilFormat { get; set; }
        
        public override void Pause()
        {
            base.Pause();

            var handler = OnPause;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected override void CreateFrameBuffer()
        {
            // Request OpenGL ES 2.0
            ContextRenderingApi = GLVersion.ES2;

            int requestedDepth = 0;
            int requestedStencil = 0;
            ColorFormat requestedColorFormat = 32;

            switch (RequestedBackBufferFormat)
            {
                case PixelFormat.R8G8B8A8_UNorm:
                case PixelFormat.B8G8R8A8_UNorm:
                    requestedColorFormat = 32;
                    break;
                case PixelFormat.B8G8R8X8_UNorm:
                    requestedColorFormat = 24;
                    break;
                case PixelFormat.B5G6R5_UNorm:
                    requestedColorFormat = new ColorFormat(5, 6, 5, 0);
                    break;
                case PixelFormat.B5G5R5A1_UNorm:
                    requestedColorFormat = new ColorFormat(5, 5, 5, 1);
                    break;
                default:
                    throw new NotSupportedException("RequestedBackBufferFormat");
            }

            switch (RequestedDepthStencilFormat)
            {
                case PixelFormat.D16_UNorm:
                    requestedDepth = 16;
                    break;
                case PixelFormat.D24_UNorm_S8_UInt:
                    requestedDepth = 24;
                    requestedStencil = 8;
                    break;
                case PixelFormat.D32_Float:
                    requestedDepth = 32;
                    break;
                case PixelFormat.D32_Float_S8X24_UInt:
                    requestedDepth = 32;
                    requestedStencil = 8;
                    break;
                default:
                    throw new NotSupportedException("RequestedDepthStencilFormat");
            }

            try
            {
                GraphicsMode = new GraphicsMode(requestedColorFormat, requestedDepth, requestedStencil);
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception)
            {
                // TODO: PDX-364: Log some warning message: "Could not create appropriate graphics mode"
            }

            // Some devices only allow D16_S8, let's try it as well
            if (requestedDepth > 16)
                requestedDepth = 16;

            GraphicsMode = new GraphicsMode(requestedColorFormat, requestedDepth, requestedStencil);
            base.CreateFrameBuffer();
        }
    }
}
#endif