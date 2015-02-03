// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS
using OpenTK;
using OpenTK.Platform.iPhoneOS;


namespace SiliconStudio.Paradox.Graphics
{
    public class SwapChainGraphicsPresenter : GraphicsPresenter
    {
        private iPhoneOSGameView gameWindow;
        private Texture backBuffer;

        public SwapChainGraphicsPresenter(GraphicsDevice device, PresentationParameters presentationParameters) : base(device, presentationParameters)
        {
            gameWindow = (iPhoneOSGameView)Description.DeviceWindowHandle.NativeHandle;
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
                return gameWindow.WindowState == WindowState.Fullscreen;
            }
            set
            {
                gameWindow.WindowState = value ? WindowState.Fullscreen : WindowState.Normal;
            }
        }

        public override void Present()
        {
            GraphicsDevice.Begin();

            // If we made a fake render target to avoid OpenGL limitations on window-provided back buffer, let's copy the rendering result to it
            if (GraphicsDevice.DefaultRenderTarget != GraphicsDevice.windowProvidedRenderTexture)
                GraphicsDevice.Copy(GraphicsDevice.DefaultRenderTarget, GraphicsDevice.windowProvidedRenderTexture);

            gameWindow.SwapBuffers();

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