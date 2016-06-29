// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Runtime.InteropServices;
using System.Security;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Native
{
    public static class OculusOvr
    {
        static OculusOvr()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS
            NativeLibrary.PreloadLibrary(NativeInvoke.Library + ".dll");
#endif
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrStartup", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Startup();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrShutdown", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Shutdown();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrCreateSessionDx", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateSessionDx(out long adapterLuidStr);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrDestroySession", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroySession(IntPtr outSessionPtr);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrCreateTexturesDx", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CreateTexturesDx(IntPtr session, IntPtr dxDevice, out int outTextureCount, int backBufferWidth, int backBufferHeight);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrGetTextureAtIndexDx", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetTextureDx(IntPtr session, Guid textureGuid, int index);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrGetMirrorTextureDx", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetMirrorTexture(IntPtr session, Guid textureGuid);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrGetCurrentTargetIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCurrentTargetIndex(IntPtr session);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrCommitFrame", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool CommitFrame(IntPtr session);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrPrepareRender", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void PrepareRender(IntPtr session,
            float near, float far,
            float* projLeft, float* projRight,
            float* positionLeft, float* positionRight,
            float* rotationLeft, float* rotationRight
            );

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnOvrGetError", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetError(IntPtr errorString);

        public static unsafe string GetError()
        {
            var buffer = stackalloc char[256];
            var errorCStr = new IntPtr(buffer);
            var error = GetError(errorCStr);
            var errorStr = Marshal.PtrToStringAnsi(errorCStr);
            return $"OculusOVR-Error({error}): {errorStr}";
        }
    }
}
