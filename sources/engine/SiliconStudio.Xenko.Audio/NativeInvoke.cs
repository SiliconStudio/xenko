﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Audio
{
    internal static class NativeInvoke
    {
#if SILICONSTUDIO_PLATFORM_IOS
        internal const string Library = "__Internal";
#else
        internal const string Library = "libxenkoaudio";
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
    }
}
