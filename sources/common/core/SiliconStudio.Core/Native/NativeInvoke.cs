// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
#if SILICONSTUDIO_PLATFORM_IOS
using ObjCRuntime;
#endif

namespace SiliconStudio.Core.Native
{
    public static class NativeInvoke
    {
#if SILICONSTUDIO_PLATFORM_IOS
        internal const string Library = "__Internal";
        internal const string LibraryName = "libcore.so";
#else
        internal const string Library = "libcore";
#if SILICONSTUDIO_PLATFORM_WINDOWS
        internal const string LibraryName = "libcore.dll";
#else
        internal const string LibraryName = "libcore.so";
#endif
#endif

        static NativeInvoke()
        {
            NativeLibrary.PreloadLibrary(LibraryName);
        }

        /// <summary>
        /// Suspends current thread for <paramref name="ms"/> milliseconds.
        /// </summary>
        /// <param name="ms">Number of milliseconds to sleep.</param>
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Library, EntryPoint = "cnSleep", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Sleep(int ms);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void ManagedLogDelegate(string log);

        private static ManagedLogDelegate managedLogDelegateSingleton;

#if SILICONSTUDIO_PLATFORM_IOS
        [MonoPInvokeCallback(typeof(ManagedLogDelegate))]
#endif
        private static void ManagedLog(string log)
        {
            Debug.WriteLine(log);
        }

        public static void Setup()
        {
            managedLogDelegateSingleton = ManagedLog;

#if !SILICONSTUDIO_PLATFORM_IOS
            var ptr = Marshal.GetFunctionPointerForDelegate(managedLogDelegateSingleton);
#else
            var ptr = managedLogDelegateSingleton;
#endif

            CoreNativeSetup(ptr);
        }

#if !SILICONSTUDIO_PLATFORM_IOS
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Library, EntryPoint = "cnSetup", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CoreNativeSetup(IntPtr logger);
#else
        [SuppressUnmanagedCodeSecurity]
        [DllImport(Library, EntryPoint = "cnSetup", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CoreNativeSetup(ManagedLogDelegate logger);
#endif
    }
}
