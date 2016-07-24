// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Core.Native
{
    internal static class NativeInvoke
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
    }
}
