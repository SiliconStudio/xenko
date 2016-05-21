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
#if SILICONSTUDIO_PLATFORM_WINDOWS
            NativeLibrary.PreloadLibrary(Library + ".dll");
#else
            NativeLibrary.PreloadLibrary(Library + ".so");
#endif
        }

#if !SILICONSTUDIO_RUNTIME_CORECLR
        [SuppressUnmanagedCodeSecurity]
#endif
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void UpdateBufferValuesFromElementInfo(IntPtr drawInfo, IntPtr vertexPtr, IntPtr indexPtr, int vertexOffset);

        internal static class OculusOvr
        {
            static OculusOvr()
            {
#if SILICONSTUDIO_PLATFORM_WINDOWS
                NativeLibrary.PreloadLibrary(Library + ".dll");
#else
            NativeLibrary.PreloadLibrary(Library + ".so");
#endif
            }

#if !SILICONSTUDIO_RUNTIME_CORECLR
            [SuppressUnmanagedCodeSecurity]
#endif
            [DllImport(Library, EntryPoint = "XenkoOvrStartup", CallingConvention = CallingConvention.Cdecl)]
            internal static extern bool Startup();

#if !SILICONSTUDIO_RUNTIME_CORECLR
            [SuppressUnmanagedCodeSecurity]
#endif
            [DllImport(Library, EntryPoint = "XenkoOvrShutdown", CallingConvention = CallingConvention.Cdecl)]
            internal static extern void Shutdown();

#if !SILICONSTUDIO_RUNTIME_CORECLR
            [SuppressUnmanagedCodeSecurity]
#endif
            [DllImport(Library, EntryPoint = "XenkoOvrCreateSession", CallingConvention = CallingConvention.Cdecl)]
            internal static extern bool Create(IntPtr outSessionPtr, IntPtr adapterLuidStr);

#if !SILICONSTUDIO_RUNTIME_CORECLR
            [SuppressUnmanagedCodeSecurity]
#endif
            [DllImport(Library, EntryPoint = "XenkoOvrDestroySession", CallingConvention = CallingConvention.Cdecl)]
            internal static extern void Destroy(IntPtr outSessionPtr);

#if !SILICONSTUDIO_RUNTIME_CORECLR
            [SuppressUnmanagedCodeSecurity]
#endif
            [DllImport(Library, EntryPoint = "XenkoOvrGetError", CallingConvention = CallingConvention.Cdecl)]
            private static extern int GetError(IntPtr errorString);

            internal static unsafe string GetError()
            {
                var buffer = stackalloc char[256];
                var errorCStr = new IntPtr(buffer);
                var error = GetError(errorCStr);
                var errorStr = Marshal.PtrToStringAnsi(errorCStr);
                return $"OculusOVR-Error({error}): {errorStr}";
            }
        }
    }
}
