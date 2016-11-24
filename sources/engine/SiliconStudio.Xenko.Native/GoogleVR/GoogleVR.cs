// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Native
{
    public static class GoogleVr
    {
//        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
//        delegate void ManagedLogDelegate(string log);
//
//        private static ManagedLogDelegate managedLogDelegateSingleton;
//
//        private static void ManagedLog(string log)
//        {
//            Debug.WriteLine(log);
//        }
//
//        public static void Setup()
//        {
//            managedLogDelegateSingleton = ManagedLog;
//            var ptr = Marshal.GetFunctionPointerForDelegate(managedLogDelegateSingleton);
//            NativeSetup(ptr);
//        }
//
//        [SuppressUnmanagedCodeSecurity]
//        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrSetup", CallingConvention = CallingConvention.Cdecl)]
//        private static extern void NativeSetup(IntPtr logger);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrStartup", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Startup(IntPtr ctx, out int width, out int height);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrInit", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Init();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrGetHeadMatrix", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetHeadMatrix(out Matrix headMatrix);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrGetEyeMatrix", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetEyeMatrix(int eyeIndex, out Matrix headMatrix);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrGetNextFrame", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetNextFrame();

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrBindBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void BindFrame(IntPtr frame, int index);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrUnbindBuffer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UnbindFrame(IntPtr frame);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrSubmitFrame", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SubmitFrame(IntPtr frame, ref Matrix headMatrix);
    }
}
