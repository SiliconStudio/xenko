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
    /// Use the functions <see cref="AttachSound"/> and <see cref="DetachSound"/> to associate or dissociate a <see cref="Sound"/> to the emitter component.
    /// Each Sound associated to the emitter component can be controlled (played, paused, stopped, ...) independently for the others.
    /// Once attached to the emitter component, a Sound is controlled using a <see cref="AudioEmitterSoundController"/>.
    /// To get the AudioEmitterSoundController associated to a Sound use the <see cref="GetSoundController"/> function.
    /// </para>
    /// </remarks>
    [Display("Audio Emitter", Expand = ExpandRule.Once)]
    [DataContract("AudioEmitterComponent")]
    [DefaultEntityComponentProcessor(typeof(AudioEmitterProcessor), ExecutionMode = ExecutionMode.Runtime)]
    [ComponentOrder(7000)]
    public sealed class AudioEmitterComponent : EntityComponent
    {
        /// <summary>
        /// Dictionary associating each Sound to a single soundController.
        /// The controller a valid as long as the corresponding Sound is present in the dictionary.
        /// </summary>
        internal readonly Dictionary<Sound, AudioEmitterSoundController> SoundToController = new Dictionary<Sound, AudioEmitterSoundController>();

        /// <summary>
        /// Create an instance of <see cref="AudioEmitterComponent"/> with a list default <see cref="Sound"/> associated.
        /// </summary>
        /// <param name="soundToAttach">The Sound to attach to the emitter by default.</param>
        public AudioEmitterComponent(IEnumerable<Sound> soundToAttach)
        {
            AttachSounds(soundToAttach);
        }

        /// <summary>
        /// Create an instance of <see cref="AudioEmitterComponent"/> with no default <see cref="Sound"/> associated.
        /// </summary>
        public AudioEmitterComponent()
            : this(new List<Sound>())
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
            /// The AudioEmitterComponent itself
            /// </summary>
            public AudioEmitterComponent EmitterComponent;

            /// <summary>
            /// Action indication if the controller has been added or removed.
            /// </summary>
            public NotifyCollectionChangedAction Action;

            public ControllerCollectionChangedEventArgs(Entity entity, AudioEmitterSoundController controller, AudioEmitterComponent component, NotifyCollectionChangedAction action)
            {
                Entity = entity;
                Controller = controller;
                EmitterComponent = component;
                Action = action;
            }
        }

        /// <summary>
        /// Event triggered when an <see cref="AudioEmitterSoundController"/> has be attached or detached to the component.
        /// </summary>
        internal event EventHandler<ControllerCollectionChangedEventArgs> ControllerCollectionChanged; 

        /// <summary>
        /// Return a <see cref="AudioEmitterSoundController"/> that can be used to control the provided <see cref="Sound"/>.
        /// </summary>
        /// <param name="sound">The Sound that the user want to control.</param>
        /// <returns>The controller that can control the <paramref name="sound"/></returns>
        /// <exception cref="ArgumentNullException">The provided <paramref name="sound"/> is null.</exception>
        /// <exception cref="ArgumentException">The provided <paramref name="sound"/> is not attached to this component.</exception>
        /// <remarks>The return AudioEmitterSoundController is valid as long as 
        /// (1) the associated Sound is attached to the emitter, 
        /// (2) the associated Sound is not disposed and,
        /// (3) the emitter component's entity is present into Entity system.</remarks>
        public AudioEmitterSoundController GetSoundController(Sound sound)
        {
            if (sound == null)
            {
                throw new ArgumentNullException(nameof(sound));
            }
            if (!SoundToController.ContainsKey(sound))
            {
                throw new ArgumentException("The provided Sound has not been attached to the EmitterComponent.");
            }

            return SoundToController[sound];
        }

        /// <summary>
        /// Attach a <see cref="Sound"/> to this emitter component.
        /// Once attached a <see cref="AudioEmitterSoundController"/> can be queried using <see cref="GetSoundController"/> to control the attached Sound.
        /// </summary>
        /// <param name="sound">The Sound to attach</param>
        /// <exception cref="ArgumentNullException">The provided <paramref name="sound"/> is null.</exception>
        /// <exception cref="InvalidOperationException">The provided <paramref name="sound"/> can not be localized (contains more than one channel).</exception>
        /// <remarks>Attaching a Sound already attached has no effects.</remarks>
        public void AttachSound(Sound sound)
        {
            if (sound == null)
            {
                throw new ArgumentNullException(nameof(sound));
            }
            if (sound.Channels > 1)
            {
                throw new InvalidOperationException("The provided Sound has more than one channel. It can not be localized in the 3D scene.");
            }

            if(SoundToController.ContainsKey(sound))
                return;

            var newController = new AudioEmitterSoundController(this, sound);
            SoundToController[sound] = newController;
            ControllerCollectionChanged?.Invoke(this, new ControllerCollectionChangedEventArgs(Entity, newController, this, NotifyCollectionChangedAction.Add ));
        }

        /// <summary>
        /// Attach a list of <see cref="Sound"/> to this emitter component.
        /// Once attached a <see cref="AudioEmitterSoundController"/> can be queried using <see cref="GetSoundController"/> to control the attached Sound.
        /// </summary>
        /// <param name="sounds">The Sounds to attach</param>
        /// <exception cref="ArgumentNullException">The provided <paramref name="sounds"/> list is null.</exception>
        /// <exception cref="InvalidOperationException">One or more of the provided Sound can not be localized (contains more than one channel).</exception>
        /// <remarks>Attaching a Sound already attached has no effects.</remarks>
        public void AttachSounds(IEnumerable<Sound> sounds)
        {
            if (sounds == null)
            {
                throw new ArgumentNullException(nameof(sounds));
            }

            foreach (var sound in sounds)
            {
                AttachSound(sound);
            }
        }

        /// <summary>
        /// Detach a <see cref="Sound"/> from this emitter component.
        /// Once detach the controller previously associated to the Sound is invalid.
        /// </summary>
        /// <param name="sound">The Sound to detach.</param>
        /// <exception cref="ArgumentNullException">The provided <paramref name="sound"/> is null.</exception>
        /// <exception cref="ArgumentException">The provided <paramref name="sound"/> is not currently attached to the emitter component.</exception>
        public void DetachSound(Sound sound)
        {
            if (sound == null)
            {
                throw new ArgumentNullException(nameof(sound));
            }
            if (!SoundToController.ContainsKey(sound))
            {
                throw new ArgumentException("The provided Sound is not currently attached to this emitter component.");
            }

            var oldController = SoundToController[sound];
            SoundToController.Remove(sound);
            ControllerCollectionChanged?.Invoke(this, new ControllerCollectionChangedEventArgs(Entity, oldController, this, NotifyCollectionChangedAction.Remove));
        }

        /// <summary>
        /// Detach a list of <see cref="Sound"/> from this emitter component.
        /// Once detach the controller previously associated to the Sound is invalid.
        /// </summary>
        /// <param name="sounds">The Sounds to detach.</param>
        /// <exception cref="ArgumentNullException">The provided <paramref name="sounds"/> is null.</exception>
        /// <exception cref="ArgumentException">One or more of the provided Sound is not currently attached to the emitter component.</exception>
        public void DetachSounds(IEnumerable<Sound> sounds)
        {
            if (sounds == null)
            {
                throw new ArgumentNullException(nameof(sounds));
            }

            foreach (var sound in sounds)
            {
                DetachSound(sound);
            }
        }

        /// <summary>
        /// Boolean indicating to the <see cref="AudioEmitterProcessor"/> if the AudioEmitterComponent need to be processed or can be skipped.
        /// </summary>
        [DataMemberIgnore]
        internal bool ShouldBeProcessed { get; set; }
    }
}
