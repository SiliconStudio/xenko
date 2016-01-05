// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_SOUND_SDL

using System;
using SiliconStudio.Xenko.Audio.Wave;

namespace SiliconStudio.Xenko.Audio
{
    public partial class SoundEffectInstance
    {
#region Implementation for SoundInstanceBase
        internal override void UpdateLooping()
        {
            throw new NotImplementedException();
        }

        internal override void PauseImpl()
        {
            throw new NotImplementedException();
        }

        internal override void ExitLoopImpl()
        {
            throw new NotImplementedException();
        }

        internal override void PlayImpl()
        {
            throw new NotImplementedException();
        }

        internal override void StopImpl()
        {
            throw new NotImplementedException();
        }

        internal override void UpdateVolume()
        {
            UpdateStereoVolumes();
        }

#endregion

        internal void CreateVoice(WaveFormat format)
        {
            throw new NotImplementedException();
        }

        private void UpdatePan()
        {
            throw new NotImplementedException();
        }

        private void UpdatePitch()
        {
            throw new NotImplementedException();
        }

        internal void Apply3DImpl(AudioListener listener, AudioEmitter emitter)
        {
            throw new NotImplementedException();
        }

        private void Reset3DImpl()
        {
            throw new NotImplementedException();
        }

#region Implementation of the IDisposable Interface

        internal void PlatformSpecificDisposeImpl()
        {
            throw new NotImplementedException();
        }

#endregion

        private void UpdateStereoVolumes()
        { }


    }
}
#endif
