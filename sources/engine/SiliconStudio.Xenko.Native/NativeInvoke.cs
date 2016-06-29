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
        internal const string LibraryName = "libxenko.so";
#else
        internal const string Library = "libxenko";
#if SILICONSTUDIO_PLATFORM_WINDOWS
        internal const string LibraryName = "libxenko.dll";
#else
        internal const string LibraryName = "libxenko.so";
#endif
#endif
        internal static void PreLoad()
        {
            NativeLibrary.PreloadLibrary(LibraryName);
        }

        static NativeInvoke()
        {
            PreLoad();
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void UpdateBufferValuesFromElementInfo(IntPtr drawInfo, IntPtr vertexPtr, IntPtr indexPtr, int vertexOffset);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(Library, EntryPoint = "xnSleep", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void Sleep(int ms);
    }
}
