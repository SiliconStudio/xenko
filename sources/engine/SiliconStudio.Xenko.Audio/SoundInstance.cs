// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Base class for sound that creates voices 
    /// </summary>
    public partial class SoundInstance: ComponentBase, IPositionableSound
    {
        internal bool DataBufferLoaded;

        [DataMemberIgnore]
        internal SoundSource SoundSource;

        /// <summary>
        /// Multiplicative factor to apply to the pitch that comes from the Doppler effect.
        /// </summary>
        private float dopplerPitchFactor;

        private bool isLooped;

        /// <summary>
        /// Channel Volume multiplicative factors that come from the 3D localization.
        /// </summary>
        private float[] localizationChannelVolumes;

        private float pan;

        /// <summary>
        /// Channel Volume multiplicative factors that come from the user panning.
        /// </summary>
        private float[] panChannelVolumes = { 1f, 1f };

        private float pitch;

        private float volume;

        //prevent creation of SoundEffectInstance to the user
        internal SoundInstance(Sound correspSound)
        {
            Sound = correspSound;

            if (Sound.StreamFromDisk)
            {
                SoundSource = new CompressedSoundSource(Sound.CompressedDataUrl, Sound.SampleRate, Sound.Channels, Sound.MaxPacketLength);
            }

            if (Sound.EngineState != AudioEngineState.Invalidated)
                CreateVoice(Sound.SampleRate, Sound.Channels);

            ResetStateToDefault();
        }

        public virtual bool IsLooped
        {
            get
            {
                Sound.CheckNotDisposed();
                return isLooped;
            }
            set
            {
                Sound.CheckNotDisposed();

                if (isLooped == value)
                    return;

                CheckBufferNotLoaded("The looping status of the sound can not be modified after it started playing.");
                isLooped = value;

                if (Sound.EngineState != AudioEngineState.Invalidated)
                    UpdateLooping();
            }
        }

        public float Pan
        {
            get
            {
                Sound.CheckNotDisposed();
                return pan;
            }
            set
            {
                Sound.CheckNotDisposed();

                Reset3D();
                pan = MathUtil.Clamp(value, -1, 1);

                panChannelVolumes = pan < 0 ? new[] { 1f, 1f + pan } : new[] { 1f - pan, 1f };

                if (Sound.EngineState != AudioEngineState.Invalidated)
                    UpdatePan();
            }
        }

        public virtual SoundPlayState PlayState { get; internal set; } = SoundPlayState.Stopped;

        public Sound Sound { get; }

        public virtual float Volume
        {
            get
            {
                Sound.CheckNotDisposed();
                return volume;
            }
            set
            {
                Sound.CheckNotDisposed();
                volume = MathUtil.Clamp(value, 0, 1);

                if (Sound.EngineState != AudioEngineState.Invalidated)
                    UpdateVolume();
            }
        }

        internal float Pitch
        {
            get
            {
                Sound.CheckNotDisposed();
                return pitch;
            }
            set
            {
                Sound.CheckNotDisposed();
                Reset3D();
                pitch = MathUtil.Clamp(value, -1, 1);

                if (Sound.EngineState != AudioEngineState.Invalidated)
                    UpdatePitch();
            }
        }

        public void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            if (!Sound.Spatialized) return;

            Sound.CheckNotDisposed();

            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            if (emitter == null)
                throw new ArgumentNullException(nameof(emitter));

            if (Sound.Channels > 1)
                throw new InvalidOperationException("Apply3D cannot be used on multi-channels sounds.");

            // reset Pan its default values.
            if (Pan != 0)
                Pan = 0;

            if (Sound.EngineState != AudioEngineState.Invalidated)
                Apply3DImpl(listener, emitter);
        }

        public virtual void ExitLoop()
        {
            Sound.CheckNotDisposed();

            if (Sound.EngineState == AudioEngineState.Invalidated)
                return;

            if (PlayState == SoundPlayState.Stopped || IsLooped == false)
                return;

            ExitLoopImpl();
        }

        public virtual void Pause()
        {
            Sound.CheckNotDisposed();

            if (Sound.EngineState == AudioEngineState.Invalidated)
                return;

            if (PlayState != SoundPlayState.Playing)
                return;

            PauseImpl();

            PlayState = SoundPlayState.Paused;
        }

        /// <summary>
        /// Play or resume the sound effect instance, stopping sibling instances.
        /// </summary>
        public void Play()
        {
            //todo Our current interface requires a void Play() method.. should we change this to be async as well?
            // ReSharper disable once UnusedVariable
            var task = Play(true);
        }

        /// <summary>
        /// Play or resume the sound effect instance, specifying explicitly how to deal with sibling instances.
        /// </summary>
        /// <param name="stopSiblingInstances">Indicate if sibling instances (instances coming from the same <see cref="Sound"/>) currently playing should be stopped or not.</param>
        public async Task Play(bool stopSiblingInstances)
        {
            await PlayExtended(stopSiblingInstances);
        }

        public void Reset3D()
        {
            if (!Sound.Spatialized) return;

            dopplerPitchFactor = 1f;
            localizationChannelVolumes = new[] { 0.5f, 0.5f };

            if (Sound.EngineState == AudioEngineState.Invalidated)
                return;

            UpdatePitch();
            UpdateStereoVolumes();

            Reset3DImpl();
        }

        public virtual void Stop()
        {
            Sound.CheckNotDisposed();

            if (Sound.EngineState == AudioEngineState.Invalidated)
                return;

            if (PlayState == SoundPlayState.Stopped)
                return;

            StopImpl();

            DataBufferLoaded = false;

            PlayState = SoundPlayState.Stopped;
        }

        internal void DestroyImpl()
        {
            Sound?.UnregisterInstance(this);

            PlatformSpecificDisposeImpl();

            SoundSource.Dispose();
        }

        internal void ResetStateToDefault()
        {
            Reset3D();
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
            DestroyImpl();
        }

        /// <summary>
        /// Play or resume the current sound instance with extended parameters.
        /// </summary>
        /// <param name="stopSiblingInstances">Indicate if the sibling instances should be stopped or not</param>
        protected async Task PlayExtended(bool stopSiblingInstances)
        {
            Sound.CheckNotDisposed();

            if (Sound.EngineState == AudioEngineState.Invalidated)
                return;

            if (Sound.AudioEngine.State == AudioEngineState.Paused) // drop the call to play if the audio engine is paused.
                return;

            if (PlayState == SoundPlayState.Playing)
                return;

            if (stopSiblingInstances)
                StopConcurrentInstances();

            if (!DataBufferLoaded && !Sound.StreamFromDisk)
            {
                LoadBuffer();
            }

            if (Sound.StreamFromDisk)
            {
                await SoundSource.ReadyToPlay.Task;

                for (var i = 0; i < SoundSource.NumberOfBuffers; i++)
                {
                    SoundSourceBuffer samples;
                    if (SoundSource.ReadSamples(out samples))
                    {
                        LoadBuffer(samples, samples.EndOfStream, samples.Length);
                        if (samples.EndOfStream && !IsLooped) break;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            PlayImpl();

            DataBufferLoaded = true;

            PlayState = SoundPlayState.Playing;
        }

        protected void StopConcurrentInstances()
        {
            Sound?.StopConcurrentInstances(this);
        }

        private void CheckBufferNotLoaded(string msg)
        {
            if (DataBufferLoaded)
                throw new InvalidOperationException(msg);
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
    }
}
