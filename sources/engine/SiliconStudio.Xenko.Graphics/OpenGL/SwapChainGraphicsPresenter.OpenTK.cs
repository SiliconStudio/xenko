// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if (SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_LINUX) && SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using SiliconStudio.Core.Mathematics;
using OpenTK;
using Rectangle = SiliconStudio.Core.Mathematics.Rectangle;
#if SILICONSTUDIO_XENKO_UI_SDL
using WindowState = SiliconStudio.Xenko.Graphics.SDL.FormWindowState;
using OpenGLWindow = SiliconStudio.Xenko.Graphics.SDL.Window;
#else
using WindowState = OpenTK.WindowState;
using OpenGLWindow = OpenTK.GameWindow;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    public class SwapChainGraphicsPresenter : GraphicsPresenter
    {
        private Texture backBuffer;

        public SwapChainGraphicsPresenter(GraphicsDevice device, PresentationParameters presentationParameters) : base(device, presentationParameters)
        {
            device.InitDefaultRenderTarget(presentationParameters);
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
                return ((OpenGLWindow)Description.DeviceWindowHandle.NativeWindow).WindowState == WindowState.Fullscreen;
            }
            set
            {
                var gameWindow = (OpenGLWindow)Description.DeviceWindowHandle.NativeWindow;
                if (gameWindow.Exists)
                    gameWindow.WindowState = value ? WindowState.Fullscreen : WindowState.Normal;
            }
        }

        public override void EndDraw(CommandList commandList, bool present)
        {
            if (present)
            {
                // If we made a fake render target to avoid OpenGL limitations on window-provided back buffer, let's copy the rendering result to it
                commandList.CopyScaler2D(backBuffer, GraphicsDevice.WindowProvidedRenderTexture,
                    new Rectangle(0, 0, backBuffer.Width, backBuffer.Height),
                    new Rectangle(0, 0, GraphicsDevice.WindowProvidedRenderTexture.Width, GraphicsDevice.WindowProvidedRenderTexture.Height), true);

                OpenTK.Graphics.GraphicsContext.CurrentContext.SwapBuffers();
            }
        }

        public override void Present()
        {
        }
        
        protected override void ResizeBackBuffer(int width, int height, PixelFormat format)
        {
        }

        protected override void ResizeDepthStencilBuffer(int width, int height, PixelFormat format)
        {
            ReleaseCurrentDepthStencilBuffer();
        }
    }
}
#endif
