// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Base class for sound that creates voices 
    /// </summary>
    public class SoundInstance: ComponentBase, IPositionableSound
    {
        private readonly DynamicSoundSource soundSource;
        private readonly Sound sound;
        private readonly AudioEngine engine;

        /// <summary>
        /// Multiplicative factor to apply to the pitch that comes from the Doppler effect.
        /// </summary>
        private float dopplerPitchFactor;

        /// <summary>
        /// Channel Volume multiplicative factors that come from the 3D localization.
        /// </summary>
        private float[] localizationChannelVolumes;

        /// <summary>
        /// Channel Volume multiplicative factors that come from the user panning.
        /// </summary>
        private float[] panChannelVolumes = { 1f, 1f };

        private bool isLooped;
        private float pan;
        private float pitch;
        private float volume;
        private readonly bool spatialized;

        internal uint Source { get; }

        public SoundInstance(AudioEngine engine, DynamicSoundSource dynamicSoundSource, bool spatialized = false)
        {
            this.engine = engine;
            this.spatialized = spatialized;
            soundSource = dynamicSoundSource;
            Source = OpenAl.SourceCreate();
            ResetStateToDefault();
        }

        internal SoundInstance(Sound staticSound)
        {
            engine = staticSound.AudioEngine;
            sound = staticSound;
            spatialized = staticSound.Spatialized;
            Source = OpenAl.SourceCreate();
            ResetStateToDefault();
            if (staticSound.StreamFromDisk)
            {
                soundSource = new CompressedSoundSource(this, staticSound.CompressedDataUrl, staticSound.SampleRate, staticSound.Channels, staticSound.MaxPacketLength);
            }
            else
            {
                OpenAl.SourceSetBuffer(Source, staticSound.PreloadedBuffer);
            }          
        }

        public bool IsLooped
        {
            get
            {
                return isLooped;
            }
            set
            {
                isLooped = value;
                if (soundSource == null) OpenAl.SourceSetLooping(Source, isLooped);
                else soundSource.SetLooped(isLooped);
            }
        }

        public float Pan
        {
            get
            {
                return pan;
            }
            set
            {
                pan = value;
                OpenAl.SourceSetPan(Source, value);                
            }
        }

        public float Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume = value;
                OpenAl.SourceSetGain(Source, volume);
            }
        }

        public float Pitch
        {
            get
            {
                return pitch;
            }
            set
            {
                pitch = value;
                OpenAl.SourceSetPitch(Source, pitch);
            }
        }

        public async Task<bool> ReadyToPlay()
        {
            if (soundSource == null) return await Task.FromResult(true);
            return await soundSource.ReadyToPlay.Task;
        }

        public unsafe void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            if (!spatialized) return;

            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            if (emitter == null)
                throw new ArgumentNullException(nameof(emitter));

            OpenAl.ListenerPush3D((float*)Interop.Fixed(ref listener.Position), (float*)Interop.Fixed(ref listener.Orientation), (float*)Interop.Fixed(ref listener.Velocity));
            OpenAl.SourcePush3D(Source, (float*)Interop.Fixed(ref emitter.Position), (float*)Interop.Fixed(ref emitter.Orientation), (float*)Interop.Fixed(ref emitter.Velocity));
        }

        public void Pause()
        {
            if (engine.State == AudioEngineState.Invalidated)
                return;

            if (PlayState != SoundPlayState.Playing)
                return;

            OpenAl.SourcePause(Source);

            playState = SoundPlayState.Paused;
        }

        /// <summary>
        /// Play or resume the sound effect instance, stopping sibling instances.
        /// </summary>
        public void Play()
        {
            Play(true);
        }

        /// <summary>
        /// Play or resume the sound effect instance, specifying explicitly how to deal with sibling instances.
        /// </summary>
        /// <param name="stopSiblingInstances">Indicate if sibling instances (instances coming from the same <see cref="Sound"/>) currently playing should be stopped or not.</param>
        public void Play(bool stopSiblingInstances)
        {
            PlayExtended(stopSiblingInstances);
        }

        public void Stop()
        {
            if (engine.State == AudioEngineState.Invalidated)
                return;

            if (playState == SoundPlayState.Stopped)
                return;

            OpenAl.SourceStop(Source);

            soundSource?.Restart();

            playState = SoundPlayState.Stopped;
        }

        internal void ResetStateToDefault()
        {
            Pan = 0;
            Volume = 1;
            IsLooped = false;
            Stop();
        }

        protected override void Destroy()
        {
            base.Destroy();

            if (IsDisposed)
                return;

            Stop();

            soundSource?.Dispose();
            sound?.UnregisterInstance(this);

            OpenAl.SourceDestroy(Source);
        }

        protected void PlayExtended(bool stopSiblingInstances)
        {
            if (engine.State == AudioEngineState.Invalidated || engine.State == AudioEngineState.Paused)
                return;

            if (PlayState == SoundPlayState.Playing)
                return;

            if (stopSiblingInstances)
                StopConcurrentInstances();

            OpenAl.SourcePlay(Source);

            playState = SoundPlayState.Playing;
        }

        protected void StopConcurrentInstances()
        {
            sound?.StopConcurrentInstances(this);
        }

        private void ComputeDopplerFactor(AudioListener listener, AudioEmitter emitter)
        {
            // To evaluate the Doppler effect we calculate the distance to the listener from one wave to the next one and divide it by the sound speed
            // we use 343m/s for the sound speed which correspond to the sound speed in the air.
            // we use 600Hz for the sound frequency which correspond to the middle of the human hearable sounds frequencies.

            const float soundSpeed = 343f;
            const float soundFreq = 600f;
            const float soundPeriod = 1 / soundFreq;

            // avoid useless calculations.
            if (emitter.DopplerScale <= float.Epsilon || (emitter.Velocity == Vector3.Zero && listener.Velocity == Vector3.Zero))
            {
                dopplerPitchFactor = 1f;
                return;
            }

            var vecListEmit = emitter.Position - listener.Position;
            var distListEmit = vecListEmit.Length();

            var vecListEmitSpeed = emitter.Velocity - listener.Velocity;
            if (Vector3.Dot(vecListEmitSpeed, Vector3.Normalize(vecListEmit)) < -soundSpeed) // emitter and listener are getting closer more quickly than the speed of the sound.
            {
                dopplerPitchFactor = float.PositiveInfinity; // will be clamped later
                return;
            }

            var timeSinceLastWaveArrived = 0f; // time elapsed since the previous wave arrived to the listener.
            var lastWaveDistToListener = 0f; // the distance that the last wave still have to travel to arrive to the listener.
            const float distLastWave = soundPeriod * soundSpeed; // distance traveled by the previous wave.
            if (distLastWave > distListEmit)
                timeSinceLastWaveArrived = (distLastWave - distListEmit) / soundSpeed;
            else
                lastWaveDistToListener = distListEmit - distLastWave;
            var nextVecListEmit = vecListEmit + soundPeriod * vecListEmitSpeed;
            var nextWaveDistToListener = nextVecListEmit.Length();
            var timeBetweenTwoWaves = timeSinceLastWaveArrived + (nextWaveDistToListener - lastWaveDistToListener) / soundSpeed;
            var apparentFrequency = 1 / timeBetweenTwoWaves;
            dopplerPitchFactor = (float)Math.Pow(apparentFrequency / soundFreq, emitter.DopplerScale);
        }

        private SoundPlayState playState = SoundPlayState.Stopped;

        public SoundPlayState PlayState
        {
            get
            {
                if (playState == SoundPlayState.Playing && !OpenAl.SourceIsPlaying(Source))
                {
                    Stop();
                }

                return playState;
            }
        }
    }
}
