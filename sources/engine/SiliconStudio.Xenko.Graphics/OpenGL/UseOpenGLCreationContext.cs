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
        private readonly bool tegraWorkaround;
#endif

        public bool UseDeviceCreationContext => useDeviceCreationContext;

        public UseOpenGLCreationContext(GraphicsDevice graphicsDevice)
            : this()
        {
            if (OpenTK.Graphics.GraphicsContext.CurrentContextHandle.Handle == IntPtr.Zero)
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