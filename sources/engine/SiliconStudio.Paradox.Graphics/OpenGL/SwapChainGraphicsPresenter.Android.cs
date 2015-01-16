// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_ANDROID
using System;
using SiliconStudio.Core.Diagnostics;
using OpenTK;
using OpenTK.Platform.Android;

namespace SiliconStudio.Paradox.Graphics
{
    public class SwapChainGraphicsPresenter : GraphicsPresenter
    {
        private static Logger Log = GlobalLogger.GetLogger("SwapChainGraphicsPresenter");
        private Texture backBuffer;

        public SwapChainGraphicsPresenter(GraphicsDevice device, PresentationParameters presentationParameters) : base(device, presentationParameters)
        {
            device.InitDefaultRenderTarget(presentationParameters);
            backBuffer = device.DefaultRenderTarget;
            DepthStencilBuffer = device.windowProvidedDepthTexture;
        }

        public override Texture BackBuffer
        {
            get { return backBuffer; }
        }

        public override object NativePresenter
        {
            get { return null; }
        }

        public override bool IsFullScreen
        {
            get
            {
                return ((AndroidGameView)Description.DeviceWindowHandle.NativeHandle).WindowState == WindowState.Fullscreen;
            }
            set
            {
                ((AndroidGameView)Description.DeviceWindowHandle.NativeHandle).WindowState = value ? WindowState.Fullscreen : WindowState.Normal;
            }
        }

        public override void Present()
        {
            GraphicsDevice.Begin();

            // If we made a fake render target to avoid OpenGL limitations on window-provided back buffer, let's copy the rendering result to it
            if (GraphicsDevice.DefaultRenderTarget != GraphicsDevice.windowProvidedRenderTexture)
                GraphicsDevice.Copy(GraphicsDevice.DefaultRenderTarget, GraphicsDevice.windowProvidedRenderTexture);
            var graphicsContext = ((AndroidGameView)Description.DeviceWindowHandle.NativeHandle).GraphicsContext;

            ((AndroidGraphicsContext)graphicsContext).Swap();

            GraphicsDevice.End();
        }

        protected override void ResizeBackBuffer(int width, int height, PixelFormat format)
        {
        }

        protected override void ResizeDepthStencilBuffer(int width, int height, PixelFormat format)
        {
            ReleaseCurrentDepthStencilBuffer();
        }

        protected override void CreateDepthStencilBuffer()
        {
        }
    }
}
#endif