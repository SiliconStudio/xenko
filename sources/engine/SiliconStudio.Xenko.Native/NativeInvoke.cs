// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Runtime.InteropServices;
using System.Security;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Native
{
    internal static class NativeInvoke
    {
#if SILICONSTUDIO_PLATFORM_IOS
        internal const string Library = "__Internal";
#else
        internal const string Library = "libxenko";
#endif

        internal static void PreLoad()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS
            NativeLibrary.PreloadLibrary(Library + ".dll");
#else
            NativeLibrary.PreloadLibrary(Library + ".so");
#endif
        }

        static NativeInvoke()
        {
            PreLoad();
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void UpdateBufferValuesFromElementInfo(IntPtr drawInfo, IntPtr vertexPtr, IntPtr indexPtr, int vertexOffset);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Library, EntryPoint = "xnSleep", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Sleep(int ms);
    }
}
