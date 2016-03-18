// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTK.Graphics;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Used internally to provide a context for async resource creation
    /// (such as texture or buffer created on a thread where no context is active).
    /// </summary>
    internal struct UseOpenGLCreationContext : IDisposable
    {
        public readonly CommandList CommandList;

        private readonly bool useDeviceCreationContext;
        private readonly bool needUnbindContext;

        private readonly bool asyncCreationLockTaken;
        private readonly object asyncCreationLockObject;

        private readonly IGraphicsContext deviceCreationContext;

#if SILICONSTUDIO_PLATFORM_ANDROID
        private readonly IGraphicsContext androidDeviceCreationContext;
        private bool tegraWorkaround;
#endif

        public bool UseDeviceCreationContext
        {
            get { return useDeviceCreationContext; }
        }

        public UseOpenGLCreationContext(GraphicsDevice graphicsDevice)
            : this()
        {
#if SILICONSTUDIO_PLATFORM_ANDROID
            // Unfortunately, android seems to not use GraphicsContext.CurrentContext to register its AndroidGraphicsContext,
            // so let's query EGL directly.
            if (GraphicsDevice.EglGetCurrentContext() == IntPtr.Zero)
#elif SILICONSTUDIO_PLATFORM_IOS
            if (OpenGLES.EAGLContext.CurrentContext == null)
#else
            if (OpenTK.Graphics.GraphicsContext.CurrentContext == null)
#endif
            {
                needUnbindContext = true;
                useDeviceCreationContext = true;

#if SILICONSTUDIO_PLATFORM_ANDROID
                tegraWorkaround = graphicsDevice.Workaround_Context_Tegra2_Tegra3;

                // Notify main rendering thread there is some pending async work to do
                if (tegraWorkaround)
                {
                    useDeviceCreationContext = false; // We actually use real main context, so states will be kept
                    graphicsDevice.AsyncPendingTaskWaiting = true;
                }
#endif

                // Lock, since there is only one deviceCreationContext.
                // TODO: Support multiple deviceCreationContext (TLS creation of context was crashing, need to investigate why)
                asyncCreationLockObject = graphicsDevice.asyncCreationLockObject;
                Monitor.Enter(graphicsDevice.asyncCreationLockObject, ref asyncCreationLockTaken);

#if SILICONSTUDIO_PLATFORM_ANDROID
                if (tegraWorkaround)
                    graphicsDevice.AsyncPendingTaskWaiting = false;

                // On android, bind the actual android context
                // The deviceCreationContext is a dummy one, so that CurrentContext works.
                androidDeviceCreationContext = graphicsDevice.androidAsyncDeviceCreationContext;
                if (androidDeviceCreationContext != null)
                    androidDeviceCreationContext.MakeCurrent(graphicsDevice.deviceCreationWindowInfo);
#endif

                // Bind the context
                deviceCreationContext = graphicsDevice.deviceCreationContext;
                deviceCreationContext.MakeCurrent(graphicsDevice.deviceCreationWindowInfo);
            }
            else
            {
                // TODO Hardcoded to the fact it uses only one command list, this should be fixed
                CommandList = graphicsDevice.MainCommandList;
            }
        }

        public void Dispose()
        {
            try
            {
                if (needUnbindContext)
                {
                    GL.Flush();

#if SILICONSTUDIO_PLATFORM_ANDROID
                    // On Android, the graphics context was just dummy so unbind the actual one.
                    // Best would be integration within OpenTK but since everything is internal and closed source,
                    // couldn't find a way around that
                    if (androidDeviceCreationContext != null)
                        androidDeviceCreationContext.MakeCurrent(null);
#endif

                    // Restore graphics context
                    GraphicsDevice.UnbindGraphicsContext(deviceCreationContext);
                }
            }
            finally
            {
                // Unlock
                if (asyncCreationLockTaken)
                {
#if SILICONSTUDIO_PLATFORM_ANDROID
                    if (tegraWorkaround)
                    {
                        // Notify GraphicsDevice.ExecutePendingTasks() that we are done.
                        Monitor.Pulse(asyncCreationLockObject);
                    }
#endif
                    Monitor.Exit(asyncCreationLockObject);
                }
            }
        }
    }
}
#endif