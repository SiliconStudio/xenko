// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Base class for sound that creates voices 
    /// </summary>
    public abstract class SoundInstanceBase: SoundBase, IPlayableSound
    {
        /// <summary>
        /// Constructor for serialization
        /// </summary>
        internal SoundInstanceBase()
        {
            DataBufferLoaded = false;
            PlayState = SoundPlayState.Stopped;
        }

        #region Buffer Management

        internal bool DataBufferLoaded;

        private void CheckBufferNotLoaded(string msg)
        {
            if (DataBufferLoaded)
                throw new InvalidOperationException(msg);
        }

        #endregion
        
        protected override void Destroy()
        {
            base.Destroy();

            if (IsDisposed)
                return;

            Stop();
            DestroyImpl();
        }

        internal abstract void DestroyImpl();

        #region IPlayableSound

        public virtual float Volume
        {
            get
            {
                CheckNotDisposed(); 
                return volume;
            }
            set
            {
                CheckNotDisposed();
                volume = MathUtil.Clamp(value, 0, 1);

                if(EngineState != AudioEngineState.Invalidated)
                    UpdateVolume();
            }
        }
        private float volume;
        internal abstract void UpdateVolume();


        public virtual bool IsLooped
        {
            get
            {
                CheckNotDisposed(); 
                return isLooped;
            }
            set
            {
                CheckNotDisposed();

                if (isLooped == value)
                    return;

                CheckBufferNotLoaded("The looping status of the sound can not be modified after it started playing.");
                isLooped = value;

                if (EngineState != AudioEngineState.Invalidated)
                    UpdateLooping();
            }
        }

        private bool isLooped;
        internal abstract void UpdateLooping();


        public virtual SoundPlayState PlayState { get; internal set; }

        public virtual void Play()
        {
            PlayExtended(true);
        }

        /// <summary>
        /// Play or resume the current sound instance with extended parameters.
        /// </summary>
        /// <param name="stopSiblingInstances">Indicate if the sibling instances should be stopped or not</param>
        protected void PlayExtended(bool stopSiblingInstances)
        {
            CheckNotDisposed();

            if (EngineState == AudioEngineState.Invalidated)
                return;

            if (AudioEngine.State == AudioEngineState.Paused) // drop the call to play if the audio engine is paused.
                return;

            if (PlayState == SoundPlayState.Playing)
                return;

            if (stopSiblingInstances)
                StopConcurrentInstances();

            if (!DataBufferLoaded)
                LoadBuffer();

            PlayImpl();

            DataBufferLoaded = true;

            PlayState = SoundPlayState.Playing;
        }

        /// <summary>
        /// Stops the sound instances in competition with this sound instance.
        /// </summary>
        protected virtual void StopConcurrentInstances()
        {
            // does nothing by default should be override
        }

        internal virtual void LoadBuffer()
        {
            // does nothing by default should be override
        }

        internal abstract void PlayImpl();


        public virtual void Pause()
        {
            CheckNotDisposed();

            if (EngineState == AudioEngineState.Invalidated)
                return;

            if(PlayState != SoundPlayState.Playing)
                return; 

            PauseImpl();

            PlayState = SoundPlayState.Paused;
        }
        internal abstract void PauseImpl();


        public virtual void Stop()
        {
            CheckNotDisposed();

            if (EngineState == AudioEngineState.Invalidated)
                return;

            if (PlayState == SoundPlayState.Stopped)
                return;

            StopImpl();

            DataBufferLoaded = false;

            PlayState = SoundPlayState.Stopped;
        }
        internal abstract void StopImpl();


        public virtual void ExitLoop()
        {
            CheckNotDisposed();

            if (EngineState == AudioEngineState.Invalidated)
                return;

            if (PlayState == SoundPlayState.Stopped || IsLooped == false)
                return;

            ExitLoopImpl();
        }
        internal abstract void ExitLoopImpl();

        #endregion
    }
}
