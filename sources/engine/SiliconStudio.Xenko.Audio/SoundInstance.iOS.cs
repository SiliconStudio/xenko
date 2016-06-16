// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Audio
{
    public partial class SoundInstance
    {
        internal AudioSource AudioSource;

        private SoundPlayState theoricPlayState;

        public SoundPlayState PlayState
        {
            get
            {
                if (theoricPlayState != SoundPlayState.Stopped && AudioSource.DidSourcePlaybackEnd())
                {
                    AudioSource.Stop();
                    DataBufferLoaded = false;
                    theoricPlayState = SoundPlayState.Stopped;
                }

                return theoricPlayState;
            }
            internal set { theoricPlayState = value; }
        }

        internal void ExitLoopImpl()
        {
            AudioSource.SetLoopingPoints(0, int.MaxValue, 0, false);
        }

        internal void LoadBuffer()
        {
            AudioSource.LoadBuffer();
        }

        internal void LoadBuffer(SoundSourceBuffer samples, bool eos, int length)
        {

        }

        internal void PauseImpl()
        {
            AudioSource.Pause();
        }

        internal void PlayImpl()
        {
            AudioSource.Play();
        }

        internal void StopImpl()
        {
            AudioSource.Stop();
        }

        internal void UpdateLooping()
        {
            AudioSource.SetLoopingPoints(0, int.MaxValue, 0, IsLooped);
        }

        internal void UpdateVolume()
        {
            AudioSource.SetVolume(Volume);
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

            AudioSource.Apply3D(azimut, elevation, distance / emitter.DistanceScale, MathUtil.Clamp((float)Math.Pow(2, Pitch) * dopplerPitchFactor, 0.5f, 2f));
        }

        private void CreateSource(int sampleRate, int channels)
        {
            AudioSource = new AudioSource(Sound.AudioEngine, this, sampleRate, channels);
        }

        private void PlatformSpecificDisposeImpl()
        {
            AudioSource.Dispose();
        }

        private void PreparePlay()
        {
            AudioSource.PreparePlay();
        }

        private void Reset3DImpl()
        {
            AudioSource.Reset3D();
        }

        private void UpdatePan()
        {
            AudioSource.SetPan(Pan);
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
