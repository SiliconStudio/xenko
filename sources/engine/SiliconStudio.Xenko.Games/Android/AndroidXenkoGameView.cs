// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_ANDROID
using System;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.OpenGL;
using Android.Content;
using OpenTK.Graphics;
using OpenTK.Platform.Android;
using OpenTK.Graphics.ES30;
using SiliconStudio.Xenko.Data;
using PixelFormat = SiliconStudio.Xenko.Graphics.PixelFormat;

namespace SiliconStudio.Xenko.Games.Android
{
    public class AndroidXenkoGameView : AndroidGameView
    {
        public EventHandler<EventArgs> OnPause;

        public AndroidXenkoGameView(Context context) : base(context)
        {
            RequestedBackBufferFormat = PixelFormat.R8G8B8A8_UNorm;
            RequestedGraphicsProfile =  new [] { GraphicsProfile.Level_10_0, GraphicsProfile.Level_9_1 };
        }

        /// <summary>
        /// Gets or sets the requested back buffer format.
        /// </summary>
        /// <value>
        /// The requested back buffer format.
        /// </value>
        public PixelFormat RequestedBackBufferFormat { get; set; }

        /// <summary>
        /// Gets or Sets the requested graphics profiles.
        /// </summary>
        /// <value>
        /// The requested graphics profiles.
        /// </value>
        public GraphicsProfile[] RequestedGraphicsProfile { get; set; }

        public override void Pause()
        {
            base.Pause();

            var handler = OnPause;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected override void CreateFrameBuffer()
        {
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

            // Query first the maximum supported profile, as some devices are crashing if we try to instantiate a 3.0 on a device supporting only 2.0
            var maximumVersion = GetMaximumSupportedProfile();

            foreach (var profile in RequestedGraphicsProfile)
            {
                var version = OpenGLUtils.GetGLVersion(profile);
                if (version > maximumVersion)
                {
                    continue;
                }
                ContextRenderingApi = version;
                GraphicsMode = new GraphicsMode(requestedColorFormat, 0, 0);
                base.CreateFrameBuffer();
                return;
            }

            throw new Exception("Unable to create a graphics context on the device. Maybe you should lower the preferred GraphicsProfile.");
        }

        private GLVersion GetMaximumSupportedProfile()
        {
            var window = ((AndroidWindow)this.WindowInfo);
            using (var context = new OpenTK.Graphics.GraphicsContext(GraphicsMode.Default, window, (int)GLVersion.ES2, 0, GraphicsContextFlags.Embedded))
            {
                context.MakeCurrent(window);

                PlatformConfigurations.RendererName = GL.GetString(StringName.Renderer);

                int version;
                if (!OpenGLUtils.GetCurrentGLVersion(out version))
                {
                    version = 200;
                }

                context.MakeCurrent(null);
                window.DestroySurface();

                if (version >= 300)
                {
                    return GLVersion.ES3;
                }
                return GLVersion.ES2;
            }
        }
    }
}
#endif