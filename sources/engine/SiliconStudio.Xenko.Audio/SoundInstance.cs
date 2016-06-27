// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
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

        private bool isLooped;
        private float pan;
        private float pitch;
        private float volume;
        private readonly bool spatialized;

        internal AudioLayer.Source Source { get; }

        internal AudioListener Listener;

        public SoundInstance(AudioEngine engine, AudioListener listener, DynamicSoundSource dynamicSoundSource, int sampleRate, bool mono, bool spatialized = false)
        {
            Listener = listener;
            this.engine = engine;
            this.spatialized = spatialized;
            soundSource = dynamicSoundSource;

            if (engine.State == AudioEngineState.Invalidated)
                return;

            Source = AudioLayer.SourceCreate(listener.Listener, sampleRate, dynamicSoundSource.MaxNumberOfBuffers, mono, spatialized, true);
            if (Source.Ptr == IntPtr.Zero)
            {
                throw new Exception("Failed to create an AudioLayer Source");
            }

            ResetStateToDefault();
        }

        internal SoundInstance(Sound staticSound, AudioListener listener)
        {
            Listener = listener;
            engine = staticSound.AudioEngine;
            sound = staticSound;
            spatialized = staticSound.Spatialized;

            if (engine.State == AudioEngineState.Invalidated)
                return;

            Source = AudioLayer.SourceCreate(listener.Listener, staticSound.SampleRate, staticSound.StreamFromDisk ? CompressedSoundSource.NumberOfBuffers : 1, staticSound.Channels == 1, spatialized, staticSound.StreamFromDisk);
            if (Source.Ptr == IntPtr.Zero)
            {
                throw new Exception("Failed to create an AudioLayer Source");
            }

            if (staticSound.StreamFromDisk)
            {
                soundSource = new CompressedSoundSource(this, staticSound.CompressedDataUrl, staticSound.SampleRate, staticSound.Channels, staticSound.MaxPacketLength);
            }
            else
            {
                AudioLayer.SourceSetBuffer(Source, staticSound.PreloadedBuffer);
            }

            ResetStateToDefault();
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

                if (engine.State == AudioEngineState.Invalidated)
                    return;

                if (soundSource == null) AudioLayer.SourceSetLooping(Source, isLooped);
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

                if (engine.State == AudioEngineState.Invalidated)
                    return;

                AudioLayer.SourceSetPan(Source, value);                
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

                if (engine.State == AudioEngineState.Invalidated)
                    return;

                AudioLayer.SourceSetGain(Source, volume);
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

                if (engine.State == AudioEngineState.Invalidated)
                    return;

                AudioLayer.SourceSetPitch(Source, pitch);
            }
        }

        public async Task<bool> ReadyToPlay()
        {
            if (soundSource == null) return await Task.FromResult(true);
            return await soundSource.ReadyToPlay.Task;
        }

        public void Apply3D(AudioEmitter emitter)
        {
            if (engine.State == AudioEngineState.Invalidated)
                return;

            if (!spatialized) return;

            if (emitter == null)
                throw new ArgumentNullException(nameof(emitter));

           emitter.Apply3D(Source);
        }

        public void Pause()
        {
            if (engine.State == AudioEngineState.Invalidated)
                return;

            if (PlayState != SoundPlayState.Playing)
                return;

            AudioLayer.SourcePause(Source);

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

            AudioLayer.SourceStop(Source);

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

            if (engine.State == AudioEngineState.Invalidated)
                return;

            AudioLayer.SourceDestroy(Source);
        }

        protected void PlayExtended(bool stopSiblingInstances)
        {
            if (engine.State == AudioEngineState.Invalidated || engine.State == AudioEngineState.Paused)
                return;

            if (PlayState == SoundPlayState.Playing)
                return;

            if (stopSiblingInstances)
                StopConcurrentInstances();

            if (soundSource != null)
            {
                Task.Run(async () =>
                {
                    await soundSource.ReadyToPlay.Task;
                    AudioLayer.SourcePlay(Source);
                });
            }
            else
            {
                AudioLayer.SourcePlay(Source);
            }

            playState = SoundPlayState.Playing;
        }

        protected void StopConcurrentInstances()
        {
            sound?.StopConcurrentInstances(this);
        }

        private SoundPlayState playState = SoundPlayState.Stopped;

        public SoundPlayState PlayState
        {
            get
            {
                if (engine.State == AudioEngineState.Invalidated)
                    return SoundPlayState.Stopped;

                if (playState == SoundPlayState.Playing && !AudioLayer.SourceIsPlaying(Source))
                {
                    Stop();
                }

                return playState;
            }
        }
    }
}
