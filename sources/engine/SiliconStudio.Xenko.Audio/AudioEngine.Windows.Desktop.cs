// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && !SILICONSTUDIO_XENKO_SOUND_SDL

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Win32;
using SharpDX.Mathematics.Interop;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Audio
{
    // We use MediaFoundation.MediaSession on windows desktop to play SoundMusics.
    // The class has an internal thread to process MediaSessionEvents.
    public class AudioEngineDesktop : AudioEngineWindows
    {
        /// <summary>
        /// This method is called during engine construction to initialize Windows.Desktop specific components.
        /// </summary>
        /// <remarks>Variables do not need to be locked here since there are no concurrent calls before the end of the construction.</remarks>
        internal override void InitImpl()
        {
        }

        internal override void DisposeImpl()
        {
        }
    }
}

#endif
