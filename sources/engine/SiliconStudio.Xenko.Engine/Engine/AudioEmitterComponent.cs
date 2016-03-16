// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Audio;
using SiliconStudio.Xenko.Engine.Design;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Component representing an audio emitter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Associate this component to an entity to simulate a 3D localized source of sound coming from the entity center.
    /// Use the component <see cref="DistanceScale"/> and <see cref="DopplerScale"/> properties to attenuate or exaggerate the 
    /// effect the distance sound attenuation and Doppler effect.
    /// </para>
    /// <para>
    /// Several sounds can be associated to a single AudioEmitterComponent. 
    /// Use the functions <see cref="AttachSoundEffect"/> and <see cref="DetachSoundEffect"/> to associate or dissociate a <see cref="SoundEffect"/> to the emitter component.
    /// Each SoundEffect associated to the emitter component can be controlled (played, paused, stopped, ...) independently for the others.
    /// Once attached to the emitter component, a SoundEffect is controlled using a <see cref="AudioEmitterSoundController"/>.
    /// To get the AudioEmitterSoundController associated to a SoundEffect use the <see cref="GetSoundEffectController"/> function.
    /// </para>
    /// </remarks>
    [Display("Audio Emitter", Expand = ExpandRule.Once)]
    [DataContract("AudioEmitterComponent")]
    [DefaultEntityComponentProcessor(typeof(AudioEmitterProcessor), ExecutionMode = ExecutionMode.Runtime)]
    [ComponentOrder(7000)]
    public sealed class AudioEmitterComponent : EntityComponent
    {
        /// <summary>
        /// Dictionary associating each soundEffect to a single soundController.
        /// The controller a valid as long as the corresponding SoundEffect is present in the dictionary.
        /// </summary>
        internal readonly Dictionary<SoundEffect, AudioEmitterSoundController> SoundEffectToController = new Dictionary<SoundEffect, AudioEmitterSoundController>();

        /// <summary>
        /// Create an instance of <see cref="AudioEmitterComponent"/> with a list default <see cref="SoundEffect"/> associated.
        /// </summary>
        /// <param name="soundEffectToAttach">The SoundEffect to attach to the emitter by default.</param>
        public AudioEmitterComponent(IEnumerable<SoundEffect> soundEffectToAttach)
        {
            DistanceScale = 1;
            DopplerScale = 1;

            AttachSoundEffects(soundEffectToAttach);
        }

        /// <summary>
        /// Create an instance of <see cref="AudioEmitterComponent"/> with no default <see cref="SoundEffect"/> associated.
        /// </summary>
        public AudioEmitterComponent()
            : this(new List<SoundEffect>())
        {
        }

        /// <summary>
        /// Event argument class used to signal the <see cref="AudioEmitterProcessor"/> that a new AudioEmitterSoundController has new added or removed to the component.
        /// </summary>
        internal class ControllerCollectionChangedEventArgs
        {
            /// <summary>
            /// The entity associated the current component.
            /// </summary>
            public Entity Entity;

            /// <summary>
            /// The controller that have been added or removed to the component.
            /// </summary>
            public AudioEmitterSoundController Controller;

            /// <summary>
            /// Action indication if the controller has been added or removed.
            /// </summary>
            public NotifyCollectionChangedAction Action;

            public ControllerCollectionChangedEventArgs(Entity entity, AudioEmitterSoundController controller, NotifyCollectionChangedAction action)
            {
                Entity = entity;
                Controller = controller;
                Action = action;
            }
        }

        /// <summary>
        /// Event triggered when an <see cref="AudioEmitterSoundController"/> has be attached or detached to the component.
        /// </summary>
        internal event EventHandler<ControllerCollectionChangedEventArgs> ControllerCollectionChanged; 

        /// <summary>
        /// Return a <see cref="AudioEmitterSoundController"/> that can be used to control the provided <see cref="SoundEffect"/>.
        /// </summary>
        /// <param name="soundEffect">The soundEffect that the user want to control.</param>
        /// <returns>The controller that can control the <paramref name="soundEffect"/></returns>
        /// <exception cref="ArgumentNullException">The provided <paramref name="soundEffect"/> is null.</exception>
        /// <exception cref="ArgumentException">The provided <paramref name="soundEffect"/> is not attached to this component.</exception>
        /// <remarks>The return AudioEmitterSoundController is valid as long as 
        /// (1) the associated soundEffect is attached to the emitter, 
        /// (2) the associated soundEffect is not disposed and,
        /// (3) the emitter component's entity is present into Entity system.</remarks>
        public AudioEmitterSoundController GetSoundEffectController(SoundEffect soundEffect)
        {
            if (soundEffect == null)
            {
                throw new ArgumentNullException("soundEffect");
            }
            if (!SoundEffectToController.ContainsKey(soundEffect))
            {
                throw new ArgumentException("The provided soundEffect has not been attached to the EmitterComponent.");
            }

            return SoundEffectToController[soundEffect];
        }

        /// <summary>
        /// Attach a <see cref="SoundEffect"/> to this emitter component.
        /// Once attached a <see cref="AudioEmitterSoundController"/> can be queried using <see cref="GetSoundEffectController"/> to control the attached SoundEffect.
        /// </summary>
        /// <param name="soundEffect">The SoundEffect to attach</param>
        /// <exception cref="ArgumentNullException">The provided <paramref name="soundEffect"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The provided <paramref name="soundEffect"/> can not be localized (contains more than one channel).</exception>
        /// <remarks>Attaching a soundEffect already attached has no effects.</remarks>
        public void AttachSoundEffect(SoundEffect soundEffect)
        {
            if (soundEffect == null)
            {
                throw new ArgumentNullException("soundEffect");
            }
            if (soundEffect.WaveFormat.Channels > 1)
            {
                throw new InvalidOperationException("The provided SoundEffect has more than one channel. It can not be localized in the 3D scene.");
            }

            if(SoundEffectToController.ContainsKey(soundEffect))
                return;

            var newController = new AudioEmitterSoundController(this, soundEffect);
            SoundEffectToController[soundEffect] = newController;
            if(ControllerCollectionChanged != null)
                ControllerCollectionChanged.Invoke(this, new ControllerCollectionChangedEventArgs(Entity, newController, NotifyCollectionChangedAction.Add ));
        }

        /// <summary>
        /// Attach a list of <see cref="SoundEffect"/> to this emitter component.
        /// Once attached a <see cref="AudioEmitterSoundController"/> can be queried using <see cref="GetSoundEffectController"/> to control the attached SoundEffect.
        /// </summary>
        /// <param name="soundEffects">The SoundEffects to attach</param>
        /// <exception cref="ArgumentNullException">The provided <paramref name="soundEffects"/> list is null.</exception>
        /// <exception cref="InvalidOperationException">One or more of the provided SoundEffect can not be localized (contains more than one channel).</exception>
        /// <remarks>Attaching a soundEffect already attached has no effects.</remarks>
        public void AttachSoundEffects(IEnumerable<SoundEffect> soundEffects)
        {
            if (soundEffects == null)
            {
                throw new ArgumentNullException("soundEffects");
            }

            foreach (var soundEffect in soundEffects)
            {
                AttachSoundEffect(soundEffect);
            }
        }

        /// <summary>
        /// Detach a <see cref="SoundEffect"/> from this emitter component.
        /// Once detach the controller previously associated to the SoundEffect is invalid.
        /// </summary>
        /// <param name="soundEffect">The soundEffect to detach.</param>
        /// <exception cref="ArgumentNullException">The provided <paramref name="soundEffect"/> is null.</exception>
        /// <exception cref="ArgumentException">The provided <paramref name="soundEffect"/> is not currently attached to the emitter component.</exception>
        public void DetachSoundEffect(SoundEffect soundEffect)
        {
            if (soundEffect == null)
            {
                throw new ArgumentNullException("soundEffect");
            }
            if (!SoundEffectToController.ContainsKey(soundEffect))
            {
                throw new ArgumentException("The provided soundEffect is not currently attached to this emitter component.");
            }

            var oldController = SoundEffectToController[soundEffect];
            SoundEffectToController.Remove(soundEffect);
            if (ControllerCollectionChanged != null)
                ControllerCollectionChanged.Invoke(this, new ControllerCollectionChangedEventArgs(Entity, oldController, NotifyCollectionChangedAction.Remove));
        }

        /// <summary>
        /// Detach a list of <see cref="SoundEffect"/> from this emitter component.
        /// Once detach the controller previously associated to the SoundEffect is invalid.
        /// </summary>
        /// <param name="soundEffects">The soundEffects to detach.</param>
        /// <exception cref="ArgumentNullException">The provided <paramref name="soundEffects"/> is null.</exception>
        /// <exception cref="ArgumentException">One or more of the provided SoundEffect is not currently attached to the emitter component.</exception>
        public void DetachSoundEffects(IEnumerable<SoundEffect> soundEffects)
        {
            if (soundEffects == null)
            {
                throw new ArgumentNullException("soundEffects");
            }

            foreach (var soundEffect in soundEffects)
            {
                DetachSoundEffect(soundEffect);
            }
        }

        /// <summary>
        /// Distance scale used to calculate the signal attenuation with the listener
        /// </summary>
        /// <remarks>
        /// By default, this value is 1.0.
        /// This value represent the distance unit and determines how quickly the signal attenuates between this object and the AudioListener. 
        /// Values below 1.0 exaggerate the attenuation to make it more apparent. 
        /// Values above 1.0 scale down the attenuation. A value of 1.0 leaves the default attenuation unchanged.
        /// Note that this value modifies only the calculated attenuation between this object and a AudioListener. 
        /// The calculated attenuation is a product of the relationship between AudioEmitter.Position and AudioListener.Position. 
        /// If the calculation yields a result of no attenuation effect, this value has no effect.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The distance scale of an audio emitter must be greater than or equal to zero.</exception>
        public float DistanceScale
        {
            get { return distanceScale; }

            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException();

                distanceScale = value;
            }
        }
        private float distanceScale;

        /// <summary>
        /// The scalar applied to the level of Doppler effect calculated between this and the listener
        /// </summary>
        /// <remarks>
        /// By default, this value is 1.0.
        /// This value determines how much to modify the calculated Doppler effect between this object and a AudioListener. 
        /// Values below 1.0 scale down the Doppler effect to make it less apparent. 
        /// Values above 1.0 exaggerate the Doppler effect. A value of 1.0 leaves the effect unmodified.
        /// Note that this value modifies only the calculated Doppler between this object and a AudioListener. 
        /// The calculated Doppler is a product of the relationship between AudioEmitter.Velocity and AudioListener.Velocity. 
        /// If the calculation yields a result of no Doppler effect, this value has no effect.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The Doppler scale of an audio emitter must be greater than or equal to zero.</exception>
        public float DopplerScale
        {
            get { return dopplerScale; }

            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException();

                dopplerScale = value;
            }
        }
        private float dopplerScale;

        /// <summary>
        /// Boolean indicating to the <see cref="AudioEmitterProcessor"/> if the AudioEmitterComponent need to be processed or can be skipped.
        /// </summary>
        [DataMemberIgnore]
        internal bool ShouldBeProcessed { get; set; }
    }
}