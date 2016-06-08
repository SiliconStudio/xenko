// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Audio
{
    public partial class SoundInstance
    {
        internal AudioVoice AudioVoice;

        private SoundPlayState theoricPlayState;

        public SoundPlayState PlayState
        {
            get
            {
                if (theoricPlayState != SoundPlayState.Stopped && AudioVoice.DidVoicePlaybackEnd())
                {
                    AudioVoice.Stop();
                    DataBufferLoaded = false;
                    theoricPlayState = SoundPlayState.Stopped;
                }

                return theoricPlayState;
            }
            internal set { theoricPlayState = value; }
        }

        internal void ExitLoopImpl()
        {
            AudioVoice.SetLoopingPoints(0, int.MaxValue, 0, false);
        }

        internal void LoadBuffer()
        {
            AudioVoice.LoadBuffer();
        }

        internal void LoadBuffer(SoundSourceBuffer samples, bool eos, int length)
        {

        }

        internal void PauseImpl()
        {
            AudioVoice.Pause();
        }

        internal void PlayImpl()
        {
            AudioVoice.Play();
        }

        internal void StopImpl()
        {
            AudioVoice.Stop();
        }

        internal void UpdateLooping()
        {
            AudioVoice.SetLoopingPoints(0, int.MaxValue, 0, IsLooped);
        }
        internal void UpdateVolume()
        {
            AudioVoice.SetVolume(Volume);
        }

        private void Apply3DImpl(AudioListener listener, AudioEmitter emitter)
        {
            var vectDirWorldBase = emitter.Position - listener.Position;
            var baseChangeMat = Matrix.Identity;
            baseChangeMat.Row2 = new Vector4(listener.Up, 0);
            baseChangeMat.Row3 = new Vector4(listener.Forward, 0);
            baseChangeMat.Row1 = new Vector4(Vector3.Cross(listener.Up, listener.Forward), 0);

            var vectDirListBase = Vector3.TransformNormal(vectDirWorldBase, baseChangeMat);
            var azimut = 180f * (float)(Math.Atan2(vectDirListBase.X, vectDirListBase.Z) / Math.PI);
            var elevation = 180f * (float)(Math.Atan2(vectDirListBase.Y, new Vector2(vectDirListBase.X, vectDirListBase.Z).Length()) / Math.PI);
            var distance = vectDirListBase.Length();

            ComputeDopplerFactor(listener, emitter);

            AudioVoice.Apply3D(azimut, elevation, distance / emitter.DistanceScale, MathUtil.Clamp((float)Math.Pow(2, Pitch) * dopplerPitchFactor, 0.5f, 2f));
        }

        private void CreateVoice(int sampleRate, int channels)
        {
            AudioVoice = new AudioVoice(Sound.AudioEngine, this, sampleRate, channels);
        }

        private void PlatformSpecificDisposeImpl()
        {
            AudioVoice.Dispose();
        }

        private void PreparePlay()
        {
            AudioVoice.PreparePlay();
        }

        private void Reset3DImpl()
        {
            AudioVoice.Reset3D();
        }

        private void UpdatePan()
        {
            AudioVoice.SetPan(Pan);
        }

        private void UpdatePitch()
        {
            //todo
        }

        private void UpdateStereoVolumes()
        {
            // nothing to do here.
        }
    }
}

#endif
