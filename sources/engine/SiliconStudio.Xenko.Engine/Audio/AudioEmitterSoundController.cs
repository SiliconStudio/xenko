// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// This class is used to control a <see cref="SiliconStudio.Xenko.Audio.Sound"/> associated to a <see cref="AudioEmitterComponent"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Instances of this class can not be directly created by the user, but need to queried from an <see cref="AudioEmitterComponent"/> 
    /// instance using the <see cref="AudioEmitterComponent.GetSoundController"/> function.
    /// </para>
    /// <para>
    /// An instance <see cref="AudioEmitterSoundController"/> is not valid anymore if any of those situations arrives: 
    /// <list type="bullet">
    ///  <item><description>The underlying <see cref="sound"/> is disposed.</description></item>
    ///  <item><description>The <see cref="AudioEmitterComponent"/> is detached from its entity.</description></item>
    ///  <item><description>The entity to which it is attached is removed from the Entity System.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [DebuggerDisplay("Controller for {sound.Name}")]
    public class AudioEmitterSoundController: IPlayableSound
    {
        /// <summary>
        /// The underlying <see cref="sound"/>
        /// </summary>
        private readonly Sound sound;

        /// <summary>
        /// The parent <see cref="AudioEmitterComponent"/> to which to controller is associated.
        /// </summary>
        private readonly AudioEmitterComponent parent;

        /// <summary>
        /// The instances of <see cref="sound"/> currently created by this controller (one for each listener).
        /// </summary>
        [DataMemberIgnore]
        internal readonly Dictionary<SoundInstance, AudioListenerComponent> InstanceToListener = new Dictionary<SoundInstance, AudioListenerComponent>();

        /// <summary>
        /// Created a new <see cref="AudioEmitterSoundController"/> instance.
        /// </summary>
        /// <param name="parent">The parent AudioEmitterComponent to which the controller is associated.</param>
        /// <param name="sound">The underlying Sound to be controlled</param>
        /// <remarks>A <see cref="sound"/> can be associated to several controllers.</remarks>
        internal AudioEmitterSoundController(AudioEmitterComponent parent, Sound sound)
        {
            if(sound == null)
                throw new ArgumentNullException(nameof(sound));

            this.sound = sound;
            this.parent = parent;

            Volume = 1;
        }

        /// <summary>
        /// Create an new instance of underlying sound, and register it in the controller's sound instance list.
        /// </summary>
        /// <returns>The new sound effect instance created</returns>
        internal SoundInstance CreateSoundInstance(AudioListenerComponent listener)
        {
            var newInstance = sound.CreateInstance(listener.Listener);

            InstanceToListener.Add(newInstance, listener);

            listener.AttachedInstances.Add(newInstance);

            return newInstance;
        }

        internal void DestroySoundInstances(AudioListenerComponent listener)
        {
            foreach (var instance in listener.AttachedInstances)
            {
                instance.Dispose();
                InstanceToListener.Remove(instance);
            }
        }

        /// <summary>
        /// Dispose and removes all the controller sound instances.
        /// </summary>
        internal void DestroyAllSoundInstances()
        {
            foreach (var instance in InstanceToListener)
            {
                instance.Key.Dispose();
            }
            InstanceToListener.Clear();
        }

        private SoundPlayState playState;

        public SoundPlayState PlayState 
        { 
            get 
            {
                // force the play status to 'stopped' if there is no listeners.
                if (!InstanceToListener.Any())
                    return SoundPlayState.Stopped;

                // return the controller playStatus if not started playing.
                if (playState != SoundPlayState.Playing || ShouldBePlayed)
                    return playState;

                // returns the playStatus of the underlying instances if controller is playing 

                // A desynchronization between instances' playState can appear due to asynchronous callbacks 
                // setting the state of the sound to Stopped when reaching the end of the track.
                // For coherency, we consider a controller as stopped only when all its instances are stopped.
                // (if not the case, a play call to a stopped controller would restart only some of the underlying instances)
                if(InstanceToListener.Any(x=>x.Key.PlayState == SoundPlayState.Playing))
                    return SoundPlayState.Playing;

                return playState = SoundPlayState.Stopped;
            } 
        }

        private bool isLooped;

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
                foreach (var instance in InstanceToListener)
                {
                    instance.Key.IsLooped = value;
                }
                isLooped = value;
            }
        }

        private float pitch = 1.0f;

        public float Pitch
        {
            get
            {
                return pitch;
            }
            set
            {
                foreach (var instance in InstanceToListener)
                {
                    instance.Key.Pitch = value;
                }
                pitch = value;
            }
        }

        /// <summary>
        /// Indicate the <see cref="AudioListenerProcessor"/> if the controller's sound instances need to be played.
        /// This variable is need because <see cref="Play"/> is asynchronous and actually starts playing only on next system update.
        /// </summary>
        internal bool ShouldBePlayed;

        public void Play()
        {
            playState = SoundPlayState.Playing;

            // Controller play function is asynchronous.
            // underlying sound instances actually start playing only after the next system update.
            // Such a asynchronous behavior is required in order to be able to update the associated AudioEmitter
            // and apply localization to the sound before starting to play.

            parent.ShouldBeProcessed = true; // tells the EmitterProcessor to update to AudioEmiter values.

            ShouldBePlayed = true;  // tells the EmitterProcessor to start playing the underlying instances.
        }

        public void Pause()
        {
            if (PlayState != SoundPlayState.Playing)
                return;

            playState = SoundPlayState.Paused;

            foreach (var instance in InstanceToListener)
            {
                instance.Key.Pause();
            }

            ShouldBePlayed = false;
        }

        public void Stop()
        {
            playState = SoundPlayState.Stopped;

            foreach (var instance in InstanceToListener)
            {
                instance.Key.Stop();
            }

            ShouldBePlayed = false;
        }

        private float volume;

        public float Volume 
        {
            get
            {
                return volume;
            }
            set
            {
                volume = MathUtil.Clamp(value, 0, 1);

                foreach (var instance in InstanceToListener)
                {
                    instance.Key.Volume = volume;
                }
            }
        }
    }
}
