// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace SiliconStudio.Core
{
    public static class NativeLibrary
    {
        private static readonly Dictionary<string, IntPtr> LoadedLibraries = new Dictionary<string, IntPtr>();

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
        [DllImport("kernel32", EntryPoint = "LoadLibrary", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32", EntryPoint = "FreeLibrary", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int FreeLibrary(IntPtr libraryHandle);

#elif SILICONSTUDIO_PLATFORM_WINDOWS_PHONE
        [DllImport("PhoneAppModelHost", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadPackagedLibrary(string libraryName, uint reserved);
#elif SILICONSTUDIO_PLATFORM_WINDOWS_STORE
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadPackagedLibrary(string libraryName, uint reserved);
#endif

        /// <summary>
        /// Defines the location of the core native DLL.
        /// </summary>
#if SILICONSTUDIO_PLATFORM_IOS
        public const string LibraryName = "__Internal";
#else
        public const string LibraryName = "libcore.dll";
#endif

        /// <summary>
        /// Defines the calling convention for P/Invoking the native core methods.
        /// </summary>
        public const CallingConvention CallConvention = CallingConvention.Cdecl;

        /// <summary>
        /// Try to preload the library.
        /// This is useful when we want to have AnyCPU .NET and CPU-specific native code.
        /// Only available on Windows for now.
        /// </summary>
        /// <param name="libraryName">Name of the library.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        public static void PreloadLibrary(string libraryName)
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            lock (LoadedLibraries)
            {
                // If already loaded, just exit as we want to load it just once
                var libraryNameNormalized = libraryName.ToLowerInvariant();
                if (LoadedLibraries.ContainsKey(libraryNameNormalized))
                {
                    return;
                }

                var systemInfo = new SYSTEM_INFO();
                GetNativeSystemInfo(out systemInfo);

                string cpu;
                if (systemInfo.processorArchitecture == PROCESSOR_ARCHITECTURE.PROCESSOR_ARCHITECTURE_ARM)
                    cpu = "ARM";
                else
                    cpu = IntPtr.Size == 8 ? "x64" : "x86";

                // We are trying to load the dll from a shadow path if it is already registered, otherwise we use it directly from the folder
                var dllFolder = NativeLibraryInternal.GetShadowPathForNativeDll(libraryName) ?? Path.Combine(Path.GetDirectoryName(typeof(NativeLibrary).Assembly.Location), cpu);
                var libraryFilename = Path.Combine(dllFolder, libraryName);
                var result = LoadLibrary(libraryFilename);

                if (result == IntPtr.Zero)
                {
                    throw new InvalidOperationException(string.Format("Could not load native library {0} from path [{1}] using CPU architecture {2}.", libraryName, libraryFilename, cpu));
                }
                else
                {
                    LoadedLibraries.Add(libraryName.ToLowerInvariant(), result);
                }
            }
#endif
        }

        /// <summary>
        /// UnLoad a specific native dynamic library loaded previously by <see cref="LoadLibrary" />.
        /// </summary>
        /// <param name="libraryName">Name of the library to unload.</param>
        public static void UnLoad(string libraryName)
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            lock (LoadedLibraries)
            {
                var libName = libraryName.ToLowerInvariant();

                IntPtr libHandle;
                if (LoadedLibraries.TryGetValue(libName, out libHandle))
                {
                    FreeLibrary(libHandle);
                    LoadedLibraries.Remove(libName);
                }
            }
#endif
        }

        /// <summary>
        /// UnLoad all native dynamic library loaded previously by <see cref="LoadLibrary"/>.
        /// </summary>
        public static void UnLoadAll()
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
            lock (LoadedLibraries)
            {
                foreach (var libraryItem in LoadedLibraries)
                {
                    FreeLibrary(libraryItem.Value);
                }
                LoadedLibraries.Clear();
            }
#endif
        }

#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
        private const string SYSINFO_FILE = "API-MS-WIN-CORE-SYSINFO-L1-2-1.DLL";
#else
        private const string SYSINFO_FILE = "kernel32.dll";
#endif

        [DllImport(SYSINFO_FILE)]
        static extern void GetNativeSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [StructLayout(LayoutKind.Sequential)]
        struct SYSTEM_INFO
        {
            public PROCESSOR_ARCHITECTURE processorArchitecture;
            ushort reserved;
            public uint pageSize;
            public IntPtr minimumApplicationAddress;
            public IntPtr maximumApplicationAddress;
            public IntPtr activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }

        enum PROCESSOR_ARCHITECTURE : ushort
        {
            PROCESSOR_ARCHITECTURE_AMD64 = 9,
            PROCESSOR_ARCHITECTURE_ARM = 5,
            PROCESSOR_ARCHITECTURE_IA64 = 6,
            PROCESSOR_ARCHITECTURE_INTEL = 0,
            PROCESSOR_ARCHITECTURE_UNKNOWN = 0xffff
        }
    }
}