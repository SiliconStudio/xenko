// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP

using System;
using System.Runtime.InteropServices;

namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// This static class gives access to the Pause/Resume API of VTune Amplifier. It is available on Windows Desktop platform only.
    /// </summary>
    public static class VTuneProfiler
    {
        /// <summary>
        /// Resumes the profiler.
        /// </summary>
        public static void Resume()
        {
            try
            {
                __itt_resume();
            }
            catch (DllNotFoundException)
            {
            }
        }

        /// <summary>
        /// Suspends the profiler.
        /// </summary>
        public static void Pause()
        {
            try
            {
                __itt_pause();
            }
            catch (DllNotFoundException)
            {
            }
        }

        [DllImport("libittnotify.dll")]
        private static extern void __itt_resume();

        [DllImport("libittnotify.dll")]
        private static extern void __itt_pause();
    }
}

#endif
