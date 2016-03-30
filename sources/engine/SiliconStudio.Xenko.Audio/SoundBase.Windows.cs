// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS && !SILICONSTUDIO_XENKO_SOUND_SDL

using System;
using SharpDX.XAudio2;

namespace SiliconStudio.Xenko.Audio
{
    public partial class SoundBase
    {
        internal MasteringVoice MasterVoice 
        {
            get
            {
                return AudioEngine.MasteringVoice;
            }
        }

        /// <summary>
        /// Create the audio engine to the sound base instance.
        /// </summary>
        /// <param name="engine">A valid AudioEngine</param>
        /// <exception cref="ArgumentNullException">The engine argument is null</exception>
        /// <exception cref="ArgumentException">The engine argument is not an instance of AudioEngineWindows</exception>
        internal void AttachEngine(AudioEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException("engine");

            AudioEngineWindows e = engine as AudioEngineWindows;
            if (e == null)
            {
                throw new ArgumentException("Invalid type, expected AudioEngineWindows", "enging");
                
            }
            AudioEngine = e;
        }

        internal AudioEngineWindows AudioEngine { get; private set; }
    }
}

#endif