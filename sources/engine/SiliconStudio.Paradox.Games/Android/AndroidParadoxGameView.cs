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

            // Some devices only allow D16_S8, let's try it as well
            // D24 and D32 are supported on OpenGL ES 3 devices
            var requestedDepthFallback = requestedDepth > 16 ? 16 : requestedDepth;
            var configs = new ContextCreationOptions[]
            {
                new ContextCreationOptions(GLVersion.ES3, requestedColorFormat, requestedDepth, requestedStencil),
                new ContextCreationOptions(GLVersion.ES2, requestedColorFormat, requestedDepth, requestedStencil),
                new ContextCreationOptions(GLVersion.ES2, requestedColorFormat, requestedDepthFallback, requestedStencil)
            };
            for (var i = 0; i < configs.Length; ++i)
            {
                if (TryCreateFrameBuffer(configs[i]))
                    return;
            }

            throw new Exception("Unable to create a graphics context on the device.");
        }

        private bool TryCreateFrameBuffer(ContextCreationOptions contextCreationOptions)
        {
            try
            {
                ContextRenderingApi = contextCreationOptions.Version;
                GraphicsMode = new GraphicsMode(contextCreationOptions.RequestedColorFormat, contextCreationOptions.RequestedDepth, contextCreationOptions.RequestedStencil);
                base.CreateFrameBuffer();
                return true;
            }
            catch (Exception)
            {
                base.DestroyFrameBuffer(); // Destroy to prevent side effects on future calls to CreateFrameBuffer
                return false;
                // TODO: PDX-364: Log some warning message: "Could not create appropriate graphics mode"
            }
        }

        private struct ContextCreationOptions
        {
            public GLVersion Version;
            public ColorFormat RequestedColorFormat;
            public int RequestedDepth;
            public int RequestedStencil;

            public ContextCreationOptions(GLVersion version, ColorFormat requestedColorFormat, int requestedDepth, int requestedStencil)
            {
                Version = version;
                RequestedColorFormat = requestedColorFormat;
                RequestedDepth = requestedDepth;
                RequestedStencil = requestedStencil;
            }
        }
    }
}
#endif