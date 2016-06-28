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

        /// <summary>
        /// Creates a new SoundInstance using a dynamic sound source
        /// </summary>
        /// <param name="engine">The audio engine that will be used to play this instance</param>
        /// <param name="listener">The listener of this instance</param>
        /// <param name="dynamicSoundSource">The source from where the PCM data will be fetched</param>
        /// <param name="sampleRate">The sample rate of this audio stream</param>
        /// <param name="mono">Set to true if the souce is mono, false if stereo</param>
        /// <param name="spatialized">If the SoundInstance will be used for spatialized audio set to true, if not false, if true mono must also be true</param>
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

        /// <summary>
        /// Gets or sets whether the sound is automatically looping from beginning when it reaches the end.
        /// </summary>
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

        /// <summary>
        /// Set the sound balance between left and right speaker.
        /// </summary>
        /// <remarks>Panning is ranging from -1.0f (full left) to 1.0f (full right). 0.0f is centered. Values beyond this range are clamped. 
        /// Panning modifies the total energy of the signal (Pan == -1 => Energy = 1 + 0, Pan == 0 => Energy = 1 + 1, Pan == 0.5 => Energy = 1 + 0.5, ...) 
        /// <para>A call to <see cref="Pan"/> may conflict with Apply3D.</para></remarks>
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

        /// <summary>
        /// The global volume at which the sound is played.
        /// </summary>
        /// <remarks>Volume is ranging from 0.0f (silence) to 1.0f (full volume). Values beyond those limits are clamped.</remarks>
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

        /// <summary>
        /// Gets or sets the pitch of the sound, might conflict with spatialized sound spatialization.
        /// </summary>
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

        /// <summary>
        /// A task that completes when the sound is ready to play
        /// </summary>
        /// <returns>Returns a task that will complete when the sound has been buffered and ready to play</returns>
        public async Task<bool> ReadyToPlay()
        {
            if (soundSource == null) return await Task.FromResult(true);
            return await soundSource.ReadyToPlay.Task;
        }

        /// <summary>
        /// Applies 3D positioning to the sound. 
        /// More precisely adjust the channel volumes and pitch of the sound, 
        /// such that the sound source seems to come from the <paramref name="emitter"/> to the listener/>.
        /// </summary>
        /// <param name="emitter">The emitter that correspond to this sound</param>
        /// <remarks>
        /// <see cref="Apply3D"/> can be used only on mono-sounds.
        /// <para>
        /// The final resulting pitch depends on the listener and emitter relative velocity. 
        /// The final resulting channel volumes depend on the listener and emitter relative positions and the value of <see cref="IPlayableSound.Volume"/>. 
        /// </para>
        /// </remarks>
        public void Apply3D(AudioEmitter emitter)
        {
            if (engine.State == AudioEngineState.Invalidated)
                return;

            if (!spatialized) return;

            if (emitter == null)
                throw new ArgumentNullException(nameof(emitter));

           emitter.Apply3D(Source);
        }

        /// <summary>
        /// Pause the sounds.
        /// </summary>
        /// <remarks>A call to Pause when the sound is already paused or stopped has no effects.</remarks>
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

        /// <summary>
        /// Stop playing the sound immediately and reset the sound to the beginning of the track.
        /// </summary>
        /// <remarks>A call to Stop when the sound is already stopped has no effects</remarks>
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
                playingQueued = true;
                Task.Run(async () =>
                {
                    await soundSource.ReadyToPlay.Task;
                    AudioLayer.SourcePlay(Source);
                    playingQueued = false;
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

        private volatile bool playingQueued;

        private SoundPlayState playState = SoundPlayState.Stopped;

        /// <summary>
        /// Gets the state of the SoundInstance
        /// </summary>
        public SoundPlayState PlayState
        {
            get
            {
                if (engine.State == AudioEngineState.Invalidated)
                    return SoundPlayState.Stopped;

                if (playState == SoundPlayState.Playing && !playingQueued && !AudioLayer.SourceIsPlaying(Source))
                {
                    Stop();
                }

                return playState;
            }
        }
    }
}
