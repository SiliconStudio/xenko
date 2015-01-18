// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_ANDROID
using System;
using Android.Runtime;
using Javax.Microedition.Khronos.Egl;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;
using OpenTK.Platform.Android;
using System.Linq;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Used internally on Android to provide a context for async resource creation (through <see cref="UseOpenGLCreationContext"/>).
    /// </summary>
    internal class AndroidAsyncGraphicsContext : IGraphicsContext, IGraphicsContextInternal
    {
        private readonly IEGL10 egl;

        public EGLContext EGLContext { get; private set; }
        public EGLDisplay EGLDisplay { get; private set; }
        public EGLSurface EGLSurface { get; private set; }

        public EGLConfig EGLConfig { get; private set; }


        private bool isDisposed;

        internal AndroidAsyncGraphicsContext(AndroidGraphicsContext graphicsContext, AndroidWindow androidWindow, int versionMajor)
        {
            egl = EGLContext.EGL.JavaCast<IEGL10>();

            var pbufferAttribList = new[] 
                {
                    EGL10.EglWidth, 1,
                    EGL10.EglHeight, 1,
                    EGL10.EglNone
                };

            EGLDisplay = androidWindow.Display;
            var androidGraphicsContext = graphicsContext;
            var config = androidGraphicsContext.EGLConfig;

            var attribList = new[] 
                { 
                    EGL10.EglSurfaceType, EGL10.EglPbufferBit,
                    EGL10.EglRenderableType, 4, // (opengl es 2.0)
            
                    EGL10.EglRedSize, graphicsContext.GraphicsMode.ColorFormat.Red,
                    EGL10.EglGreenSize, graphicsContext.GraphicsMode.ColorFormat.Green,
                    EGL10.EglBlueSize, graphicsContext.GraphicsMode.ColorFormat.Blue,
                    EGL10.EglAlphaSize, graphicsContext.GraphicsMode.ColorFormat.Alpha,
            
                    EGL10.EglDepthSize, graphicsContext.GraphicsMode.Depth,
                    EGL10.EglStencilSize, graphicsContext.GraphicsMode.Stencil,
            
                    //Egl.SAMPLE_BUFFERS, samples > 0 ? 1 : 0,
                    EGL10.EglSamples, 0,
            
                    EGL10.EglNone,
                };

            // first ask the number of config available
            var numConfig = new int[1];
            if (!egl.EglChooseConfig(EGLDisplay, attribList, null, 0, numConfig))
            {
                throw new InvalidOperationException(string.Format("EglChooseConfig {0:x}", egl.EglGetError()));
            }

            // retrieve the available configs
            var configs = new EGLConfig[numConfig[0]];
            if (!egl.EglChooseConfig(EGLDisplay, attribList, configs, configs.Length, numConfig))
            {
                throw new InvalidOperationException(string.Format("EglChooseConfig {0:x}", egl.EglGetError()));
            }

            // choose the best config
            EGLConfig = ChooseConfigEGL(configs);

            // create the surface
            EGLSurface = egl.EglCreatePbufferSurface(EGLDisplay, EGLConfig, pbufferAttribList);
            if (EGLSurface == EGL10.EglNoSurface)
            {
                throw new InvalidOperationException(string.Format("EglCreatePBufferSurface {0:x}", egl.EglGetError()));
            }

            // 0x3098 is EGL_CONTEXT_CLIENT_VERSION
            var attribList3 = new[] { 0x3098, versionMajor, EGL10.EglNone };
            EGLContext = egl.EglCreateContext(EGLDisplay, config, androidGraphicsContext.EGLContext, attribList3);
            if (EGLContext == EGL10.EglNoContext)
            {
                throw new InvalidOperationException(string.Format("EglCreateContext {0:x}", egl.EglGetError()));
            }
        }

        private struct UserReadableEglConfig
        {
            public int SurfaceType;
            public int RenderableType;
            public int RedSize;
            public int GreenSize;
            public int BlueSize;
            public int AlphaSize;
            public int DepthSize;
            public int StencilSize;
            public int Samples;
        }

        private UserReadableEglConfig EglConfigToUserReadableEglConfig(EGLConfig eglConfig)
        {
            var surfaceType = new int[1];
            var renderableType = new int[1];
            var redSize = new int[1];
            var greenSize = new int[1];
            var blueSize = new int[1];
            var alphaSize = new int[1];
            var depthSize = new int[1];
            var stencilSize = new int[1];
            var samples = new int[1];

            if (!egl.EglGetConfigAttrib(EGLDisplay, eglConfig, EGL10.EglSurfaceType, surfaceType))
                throw new InvalidOperationException(string.Format("EglGetConfigAttrib {0:x}", egl.EglGetError()));
            if (!egl.EglGetConfigAttrib(EGLDisplay, eglConfig, EGL10.EglRenderableType, renderableType))
                throw new InvalidOperationException(string.Format("EglGetConfigAttrib {0:x}", egl.EglGetError()));
            if (!egl.EglGetConfigAttrib(EGLDisplay, eglConfig, EGL10.EglRedSize, redSize))
                throw new InvalidOperationException(string.Format("EglGetConfigAttrib {0:x}", egl.EglGetError()));
            if (!egl.EglGetConfigAttrib(EGLDisplay, eglConfig, EGL10.EglGreenSize, greenSize))
                throw new InvalidOperationException(string.Format("EglGetConfigAttrib {0:x}", egl.EglGetError()));
            if (!egl.EglGetConfigAttrib(EGLDisplay, eglConfig, EGL10.EglBlueSize, blueSize))
                throw new InvalidOperationException(string.Format("EglGetConfigAttrib {0:x}", egl.EglGetError()));
            if (!egl.EglGetConfigAttrib(EGLDisplay, eglConfig, EGL10.EglAlphaSize, alphaSize))
                throw new InvalidOperationException(string.Format("EglGetConfigAttrib {0:x}", egl.EglGetError()));
            if (!egl.EglGetConfigAttrib(EGLDisplay, eglConfig, EGL10.EglDepthSize, depthSize))
                throw new InvalidOperationException(string.Format("EglGetConfigAttrib {0:x}", egl.EglGetError()));
            if (!egl.EglGetConfigAttrib(EGLDisplay, eglConfig, EGL10.EglStencilSize, stencilSize))
                throw new InvalidOperationException(string.Format("EglGetConfigAttrib {0:x}", egl.EglGetError()));
            if (!egl.EglGetConfigAttrib(EGLDisplay, eglConfig, EGL10.EglSamples, samples))
                throw new InvalidOperationException(string.Format("EglGetConfigAttrib {0:x}", egl.EglGetError()));

            return new UserReadableEglConfig
                {
                    SurfaceType = surfaceType[0],
                    RenderableType = renderableType[0],
                    RedSize = redSize[0],
                    GreenSize = greenSize[0],
                    BlueSize = blueSize[0],
                    AlphaSize = alphaSize[0],
                    DepthSize = depthSize[0],
                    StencilSize = stencilSize[0],
                    Samples = samples[0]
                };
        }

        private EGLConfig ChooseConfigEGL(EGLConfig[] configs)
        {
            if(configs.Length == 0)
                throw new NotSupportedException("The graphic device configuration demanded is not supported.");

            var readableConfigs = new UserReadableEglConfig[configs.Length];

            // convert the configs into user readable configs
            for (int i = 0; i < configs.Length; i++)
                readableConfigs[i] = EglConfigToUserReadableEglConfig(configs[i]);

            return configs[0];
        }

        public void Dispose()
        {
            EGLContext.Dispose();
            EGLSurface.Dispose();
        }

        public void SwapBuffers()
        {
            if (!egl.EglSwapBuffers(EGLDisplay, EGLSurface))
            {
                var error = egl.EglGetError();
                if (error == 0x300e)
                    throw new InvalidOperationException(string.Format("EglSwapBuffers {0:x}", error));
            }
        }

        public void MakeCurrent(IWindowInfo window)
        {
            if (window != null)
            {
                if (!egl.EglMakeCurrent(EGLDisplay, EGLSurface, EGLSurface, EGLContext))
                    throw new InvalidOperationException();
            }
            else
            {
                if (!egl.EglMakeCurrent(EGLDisplay, EGL10.EglNoSurface, EGL10.EglNoSurface, EGL10.EglNoContext))
                    throw new InvalidOperationException();
            }
        }

        public void Update(IWindowInfo window)
        {
            MakeCurrent(null);
            MakeCurrent(window);
        }

        public void LoadAll()
        {
        }

        public bool IsCurrent
        {
            get { return (egl.EglGetCurrentContext() == EGLContext);
            }
        }

        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public bool VSync
        {
            get { return false; }
            set {}
        }

        public int SwapInterval { get; set; }

        public GraphicsMode GraphicsMode
        {
            get { throw new NotImplementedException(); }
        }

        public bool ErrorChecking
        {
            get { return false; }
            set {}
        }

        IntPtr IGraphicsContextInternal.GetAddress(string function)
        {
            return IntPtr.Zero;
        }

        public IGraphicsContext Implementation
        {
            get { return this; }
        }

        public ContextHandle Context
        {
            get { return new ContextHandle(EGLContext.Handle); }
        }
    }
}
#endif