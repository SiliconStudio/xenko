// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Audio.Wave;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Audio
{
    /// <summary>
    /// Instance of a SoundEffect sound which can be independently localized and played.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You can create a SoundEffectInstance by calling <see cref="SoundEffect.CreateInstance"/>. 
    /// Initially, the SoundEffectInstance is created as stopped, but you can play it by calling <see cref="SoundInstanceBase.Play"/>.
    /// You can modify the volume and the panning of the SoundEffectInstance by setting the <see cref="SoundInstanceBase.Volume"/> and <see cref="Pan"/> properties 
    /// or directly apply a localization to the sound by calling the <see cref="IPositionableSound.Apply3D"/> function.
    /// </para>
    /// <para>
    /// A SoundEffectInstance is invalidated when the corresponding <see cref="SoundEffect"/> is Disposed.
    /// </para>
    /// <para>
    /// By default, playing an sound effect stops all the sibling instances currently playing. That is, the instances coming from the same <see cref="SoundEffect"/>.
    /// This behavior is important on mobile devices which most of the time have only a limited number audio tracks. 
    /// It prevents all the audio tracks from being occupied by the same short repeating sound and favors the variety of sounds.
    /// This behavior can nevertheless be overridden by using the <see cref="SoundEffectInstance.Play(bool)"/> function.
    /// </para>
    /// </remarks>
    /// <seealso cref="SoundEffect"/>
    /// <seealso cref="IPositionableSound"/>"/>
    /// <seealso cref="DynamicSoundEffectInstance"/>
    public partial class SoundEffectInstance : SoundInstanceBase, IPositionableSound
    {
        private readonly SoundEffect soundEffect;

        internal virtual WaveFormat WaveFormat
        {
            get { return soundEffect.WaveFormat; }
        }

        //prevent creation of SoundEffectInstance to the user
        internal SoundEffectInstance(SoundEffect correspSoundEffect)
            : base(correspSoundEffect.AudioEngine)
        {
            soundEffect = correspSoundEffect;
            
            if (EngineState != AudioEngineState.Invalidated)
                CreateVoice(soundEffect.WaveFormat);

            ResetStateToDefault();
        }

        //prevent creation of SoundEffectInstance to the user and other classes
        internal SoundEffectInstance(AudioEngine engine)
            : base(engine)
        {
            soundEffect = null;
        }

        internal void ResetStateToDefault()
        {
            Reset3D();
            Pan = 0;
            Volume = 1;
            IsLooped = false;
            Stop();
        }

        protected override void StopConcurrentInstances()
        {
            if(soundEffect != null)
                soundEffect.StopConcurrentInstances(this);
        }
        
        /// <summary>
        /// Play or resume the sound effect instance, specifying explicitly how to deal with sibling instances.
        /// </summary>
        /// <param name="stopSiblingInstances">Indicate if sibling instances (instances coming from the same <see cref="SoundEffect"/>) currently playing should be stopped or not.</param>
        public void Play(bool stopSiblingInstances)
        {
            PlayExtended(stopSiblingInstances);
        }
        
        #region Implementation of the ILocalizable Interface

        public float Pan
        {
            get
            {
                CheckNotDisposed(); 
                return pan;
            }
            set
            {
                CheckNotDisposed();

                Reset3D();
                pan = MathUtil.Clamp(value, -1, 1);

                if (pan < 0)
                    panChannelVolumes = new[] { 1f, 1f + pan };
                else
                    panChannelVolumes = new[] { 1f - pan, 1f };

                if (EngineState != AudioEngineState.Invalidated)
                    UpdatePan(); 
            }
        }
        private float pan;

        /// <summary>
        /// Channel Volume multiplicative factors that come from the user panning.
        /// </summary>
        private float[] panChannelVolumes = { 1f, 1f };

        public void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            CheckNotDisposed();

            if (listener == null)
                throw new ArgumentNullException("listener");

            if(emitter == null)
                throw new ArgumentNullException("emitter");

            if(soundEffect.WaveFormat.Channels > 1)
                throw new InvalidOperationException("Apply3D cannot be used on multi-channels sounds.");

            // reset Pan its default values.
            if (Pan != 0)
                Pan = 0;

            if (EngineState != AudioEngineState.Invalidated)
                Apply3DImpl(listener, emitter);
        }
        /// <summary>
        /// Channel Volume multiplicative factors that come from the 3D localization.
        /// </summary>
        private float[] localizationChannelVolumes;

        internal float Pitch
        {
            get
            {
                CheckNotDisposed();
                return pitch;
            }
            set
            {
                CheckNotDisposed();
                Reset3D();
                pitch = MathUtil.Clamp(value, -1, 1);

                if (EngineState != AudioEngineState.Invalidated)
                    UpdatePitch();
            }
        }
        private float pitch;

        /// <summary>
        /// Multiplicative factor to apply to the pitch that comes from the Doppler effect.
        /// </summary>
        private float dopplerPitchFactor;

        private void ComputeDopplerFactor(AudioListener listener, AudioEmitter emitter)
        {
            // To evaluate the Doppler effect we calculate the distance to the listener from one wave to the next one and divide it by the sound speed
            // we use 343m/s for the sound speed which correspond to the sound speed in the air.
            // we use 600Hz for the sound frequency which correspond to the middle of the human hearable sounds frequencies.

            const float SoundSpeed = 343f;
            const float SoundFreq = 600f;
            const float SoundPeriod = 1 / SoundFreq;

            // avoid useless calculations.
            if (emitter.DopplerScale <= float.Epsilon || (emitter.Velocity == Vector3.Zero && listener.Velocity == Vector3.Zero))
            {
                dopplerPitchFactor = 1f;
                return;
            }

            var vecListEmit = emitter.Position - listener.Position;
            var distListEmit = vecListEmit.Length();

            var vecListEmitSpeed = emitter.Velocity - listener.Velocity;
            if (Vector3.Dot(vecListEmitSpeed, Vector3.Normalize(vecListEmit)) < -SoundSpeed) // emitter and listener are getting closer more quickly than the speed of the sound.
            {
                dopplerPitchFactor = float.PositiveInfinity; // will be clamped later
                return;
            }

            var timeSinceLastWaveArrived = 0f; // time elapsed since the previous wave arrived to the listener.
            var lastWaveDistToListener = 0f; // the distance that the last wave still have to travel to arrive to the listener.
            const float DistLastWave = SoundPeriod * SoundSpeed; // distance traveled by the previous wave.
            if (DistLastWave > distListEmit)
                timeSinceLastWaveArrived = (DistLastWave - distListEmit) / SoundSpeed;
            else
                lastWaveDistToListener = distListEmit - DistLastWave;
            var nextVecListEmit = vecListEmit + SoundPeriod * vecListEmitSpeed;
            var nextWaveDistToListener = nextVecListEmit.Length();
            var timeBetweenTwoWaves = timeSinceLastWaveArrived + (nextWaveDistToListener - lastWaveDistToListener) / SoundSpeed;
            var apparentFrequency = 1 / timeBetweenTwoWaves;
            dopplerPitchFactor = (float) Math.Pow(apparentFrequency / SoundFreq, emitter.DopplerScale);
        }

        #endregion

        internal override void DestroyImpl()
        {
            if(soundEffect != null)
                soundEffect.UnregisterInstance(this);

            PlatformSpecificDisposeImpl();
        }

        public void Reset3D()
        {
            dopplerPitchFactor = 1f;
            localizationChannelVolumes = new[] { 0.5f, 0.5f };
            
            if (EngineState == AudioEngineState.Invalidated)
                return;

            UpdatePitch();
            UpdateStereoVolumes();

            Reset3DImpl();
        }
    }
}
