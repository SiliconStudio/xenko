// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Runtime.InteropServices;
using System.Security;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.VirtualReality
{
    public static class Fove
    {
        static Fove()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS
            NativeLibrary.PreloadLibrary(NativeInvoke.Library + ".dll");
#endif
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnFoveStartup", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Startup();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnFoveShutdown", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Shutdown();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnFoveSubmit", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Submit(IntPtr texture, ref Vector4 bounds, int eyeIndex);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnFoveCommit", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Commit();

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct FrameProperties
        {
            public float Near;
            public float Far;
            public Matrix ProjLeft;
            public Matrix ProjRight;
            public Vector3 Pos;
            public Quaternion Rot;
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnFovePrepareRender", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PrepareRender(ref FrameProperties properties);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnFoveGetLeftEyePoint", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetLeftEyePoint(ref Vector2 point);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnFoveRecenter", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Recenter();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnFoveIsHardwareReady", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsHardwareReady();
    }
}
