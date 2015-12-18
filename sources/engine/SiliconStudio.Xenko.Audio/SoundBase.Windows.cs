// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS

using SharpDX.XAudio2;


namespace SiliconStudio.Xenko.Audio
{
    public partial class SoundBase
    {
        internal MasteringVoice MasterVoice 
        {
            get
            {
                // TODO: Find a way to not perform a cast!
                return ((AudioEngineWindows) AudioEngine).MasteringVoice;
            }
        }
    }
}

#endif