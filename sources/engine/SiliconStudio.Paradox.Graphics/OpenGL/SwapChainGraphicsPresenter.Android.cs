// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_ANDROID
using System;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
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
            device.InitDefaultRenderTarget(Description);
            //backBuffer = device.DefaultRenderTarget;
            // TODO: Review Depth buffer creation for both Android and iOS
            //DepthStencilBuffer = device.windowProvidedDepthTexture;

            backBuffer = Texture.New2D(device, Description.BackBufferWidth, Description.BackBufferHeight, presentationParameters.BackBufferFormat, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
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

        protected override void ProcessPresentationParameters()
        {
            // Use aspect ratio of device
            var androidGameView = (AndroidGameView)Description.DeviceWindowHandle.NativeHandle;
            var panelWidth = androidGameView.Size.Width;
            var panelHeight = androidGameView.Size.Height;
            var panelRatio = (float)panelWidth / panelHeight;

            var desiredWidth = Description.BackBufferWidth;
            var desiredHeight = Description.BackBufferHeight;

            if (panelRatio >= 1.0f) // Landscape => use height as base
            {
                Description.BackBufferHeight = (int)(desiredWidth / panelRatio);
            }
            else // Portrait => use width as base
            {
                Description.BackBufferWidth = (int)(desiredHeight * panelRatio);
            }
        }

        public override void Present()
        {
            GraphicsDevice.Begin();

            var androidGameView = (AndroidGameView)Description.DeviceWindowHandle.NativeHandle;
            GraphicsDevice.windowProvidedRenderTexture.InternalSetSize(androidGameView.Width, androidGameView.Height);

            // If we made a fake render target to avoid OpenGL limitations on window-provided back buffer, let's copy the rendering result to it
            if (backBuffer != GraphicsDevice.windowProvidedRenderTexture)
                GraphicsDevice.CopyScaler2D(backBuffer, GraphicsDevice.windowProvidedRenderTexture,
                    new Rectangle(0, 0, backBuffer.Width, backBuffer.Height),
                    new Rectangle(0, 0, GraphicsDevice.windowProvidedRenderTexture.Width, GraphicsDevice.windowProvidedRenderTexture.Height), false);
            var graphicsContext = androidGameView.GraphicsContext;

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

        // TODO: Review Depth buffer creation for both Android and iOS
        //protected override void CreateDepthStencilBuffer()
        //{
        //}
    }
}
#endif