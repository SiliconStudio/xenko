// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS && !SILICONSTUDIO_XENKO_SOUND_SDL

namespace SiliconStudio.Xenko.Audio
{
    public partial class SoundEffect
    {
        private void AdaptAudioDataImpl()
        {
            // Nothing to do here. Audio input of different sampling rate is correctly handle by the Windows OS
        }

        private void DestroyImpl()
        {
        }
    }
}

#endif