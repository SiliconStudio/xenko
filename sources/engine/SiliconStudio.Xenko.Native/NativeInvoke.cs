// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;

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
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        public static extern void AppendTextToVertexBuffer(RectangleF constantInfos, RectangleF renderInfos, string text, out IntPtr textLength, out IntPtr vertexBufferPointer);
    }

    internal class Module
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            Core.Native.NativeInvoke.Setup();
        }
    }
}
