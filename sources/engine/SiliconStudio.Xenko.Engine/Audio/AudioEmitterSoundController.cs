// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    ///  <item><description>The underlying <see cref="Sound"/> is disposed.</description></item>
    ///  <item><description>The <see cref="AudioEmitterComponent"/> is detached from its entity.</description></item>
    ///  <item><description>The entity to which it is attached is removed from the Entity System.</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [DebuggerDisplay("Controller for {Sound.Name}")]
    public class AudioEmitterSoundController: IPlayableSound
    {
        /// <summary>
        /// The underlying <see cref="Sound"/>
        /// </summary>
        private readonly Sound Sound;

        /// <summary>
        /// The parent <see cref="AudioEmitterComponent"/> to which to controller is associated.
        /// </summary>
        private readonly AudioEmitterComponent parent;

        /// <summary>
        /// The instances of <see cref="Sound"/> currently created by this controller (one for each listener).
        /// </summary>
        private readonly HashSet<SoundInstance> associatedSoundInstances = new HashSet<SoundInstance>();

        /// <summary>
        /// Created a new <see cref="AudioEmitterSoundController"/> instance.
        /// </summary>
        /// <param name="parent">The parent AudioEmitterComponent to which the controller is associated.</param>
        /// <param name="Sound">The underlying Sound to be controlled</param>
        /// <remarks>A <see cref="Sound"/> can be associated to several controllers.</remarks>
        internal AudioEmitterSoundController(AudioEmitterComponent parent, Sound Sound)
        {
            if(Sound == null)
                throw new ArgumentNullException("Sound");

            this.Sound = Sound;
            this.parent = parent;

            Volume = 1;
        }

        /// <summary>
        /// Create an new instance of underlying sound, and register it in the controller's sound instance list.
        /// </summary>
        /// <returns>The new sound effect instance created</returns>
        internal SoundInstance CreateSoundInstance()
        {
            var newInstance = Sound.CreateInstance();

            associatedSoundInstances.Add(newInstance);

            return newInstance;
        }

        /// <summary>
        /// Dispose and sound instance and removes it from the controller sound instance list.
        /// </summary>
        /// <param name="soundInstance">Sound instance to destroy</param>
        internal void DestroySoundInstance(SoundInstance soundInstance)
        {
            soundInstance.Dispose();
            associatedSoundInstances.Remove(soundInstance);
        }

        /// <summary>
        /// Dispose and removes all the controller sound instances.
        /// </summary>
        internal void DestroyAllSoundInstances()
        {
            foreach (var instance in associatedSoundInstances)
            {
                instance.Dispose();
            }
            associatedSoundInstances.Clear();
        }

        private SoundPlayState playState;
        public SoundPlayState PlayState 
        { 
            get 
            {
                // force the play status to 'stopped' if there is no listeners.
                if (!associatedSoundInstances.Any())
                    return SoundPlayState.Stopped;

                // return the controller playStatus if not started playing.
                if (playState != SoundPlayState.Playing || ShouldBePlayed)
                    return playState;

                // returns the playStatus of the underlying instances if controller is playing 

                // A desynchronization between instances' playState can appear due to asynchronous callbacks 
                // setting the state of the sound to Stopped when reaching the end of the track.
                // For coherency, we consider a controller as stopped only when all its instances are stopped.
                // (if not the case, a play call to a stopped controller would restart only some of the underlying instances)
                if(associatedSoundInstances.Any(x=>x.PlayState == SoundPlayState.Playing))
                    return SoundPlayState.Playing;

                return playState = SoundPlayState.Stopped;
            } 
        }

        private bool isLooped;
        public bool IsLooped
        {
            get
            {
                return isLooped;
            }
            set
            {
                foreach (var instance in associatedSoundInstances)
                {
                    instance.IsLooped = value;
                }
                isLooped = value;
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

            foreach (var instance in associatedSoundInstances)
            {
                instance.Pause();
            }
            ShouldBePlayed = false;
        }

        public void Stop()
        {
            playState = SoundPlayState.Stopped;

            foreach (var instance in associatedSoundInstances)
            {
                instance.Stop();
            }
            ShouldBePlayed = false;
        }

        internal bool ShouldExitLoop;
        public void ExitLoop()
        {
            if (ShouldBePlayed)
                ShouldExitLoop = true;

            foreach (var instance in associatedSoundInstances)
            {
                instance.ExitLoop();
            }
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

                foreach (var instance in associatedSoundInstances)
                {
                    instance.Volume = volume;
                }
            }
        }
    }
}
