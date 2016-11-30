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
        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrStartup", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Startup(IntPtr ctx);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrGetMaxRenderSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetMaxRenderSize(out int width, out int height);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrInit", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Init(int width, int height);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrGetPerspectiveMatrix", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetPerspectiveMatrix(int eyeIndex, float near, float far, out Matrix headMatrix);

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
        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrGetFBOIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetFBOIndex(IntPtr frame, int index);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(NativeInvoke.Library, EntryPoint = "xnGvrSubmitFrame", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SubmitFrame(IntPtr frame, ref Matrix headMatrix);
    }
}
