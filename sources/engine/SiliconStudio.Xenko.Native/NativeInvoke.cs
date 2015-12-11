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
        public const string Library = "__Internal";
#else
        public const string Library = "libxenkonative";
#endif

        static NativeInvoke()
        {
            NativeLibrary.PreloadLibrary(NativeInvoke.Library + ".dll");
        }

        [DllImport(Native.NativeInvoke.Library, CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
        internal static extern void UpdateBufferValuesFromElementInfo(IntPtr drawInfo, IntPtr vertexPtr, IntPtr indexPtr, int vertexOffset);
    }
}