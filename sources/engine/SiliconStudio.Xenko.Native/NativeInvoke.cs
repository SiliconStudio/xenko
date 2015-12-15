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
        internal const string Library = "libxenkonative";
#endif

        static NativeInvoke()
        {
            NativeLibrary.PreloadLibrary(Library + ".dll");
        }

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void UpdateBufferValuesFromElementInfo(IntPtr drawInfo, IntPtr vertexPtr, IntPtr indexPtr, int vertexOffset);
    }
}