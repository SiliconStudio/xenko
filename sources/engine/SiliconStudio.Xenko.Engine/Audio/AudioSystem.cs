// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// The Audio System.
    /// It creates an underlying instance of <see cref="AudioEngine"/>.
    /// </summary>
    public class AudioSystem : GameSystemBase, IAudioEngineProvider
    {
        /// <summary>
        /// Create an new instance of AudioSystem
        /// </summary>
        /// <param name="registry">The service registry in which to register the <see cref="AudioSystem"/> services</param>
        public AudioSystem(IServiceRegistry registry)
            : base(registry)
        {
            Enabled = true;

            registry.AddService(typeof(AudioSystem), this);
            registry.AddService(typeof(IAudioEngineProvider), this);
        }

        /// <summary>
        /// The underlying <see cref="AudioEngine" />.
        /// </summary>
        /// <value>The audio engine.</value>
        public AudioEngine AudioEngine { get; private set; }

        /// <summary>
        /// A collection containing the <see cref="AudioListenerComponent"/>-<see cref="AudioListener"/> associations.
        /// The AudioListenerComponent keys are added/removed by the user by calls to <see cref="AddListener"/>/<see cref="RemoveListener"/>.
        /// The AudioListener values are created/updated by the <see cref="AudioListenerProcessor"/>. 
        /// </summary>
        /// <remarks>When a AudioListenerComponent is added to the AudioSystem but not present in the Entity System, 
        /// a valid AudioListener can not be computed. Thus we set its value to 'null'.</remarks>
        internal readonly TrackingDictionary<AudioListenerComponent, AudioListener> Listeners = new TrackingDictionary<AudioListenerComponent, AudioListener>();

        public override void Initialize()
        {
            base.Initialize();

            AudioEngine = AudioEngineFactory.NewAudioEngine();

            Game.Activated += OnActivated;
            Game.Deactivated += OnDeactivated;
        }

        /// <summary>
        /// Add and activate a <see cref="AudioListenerComponent" /> to the Audio System.
        /// After this call sounds played via <see cref="AudioEmitterSoundController" />s will be heard by this listener.
        /// </summary>
        /// <param name="listener">The listener to add to the audio system.</param>
        /// <remarks>Adding a listener already added as no effects.</remarks>
        internal void AddListener(AudioListenerComponent listener)
        {
            if(!Listeners.ContainsKey(listener))
                Listeners[listener] = null;
        }

        /// <summary>
        /// Remove a <see cref="AudioListenerComponent" /> from the Audio System.
        /// After this call sounds played via <see cref="AudioEmitterSoundController" />s will not be heard by this listener anymore.
        /// </summary>
        /// <param name="listener">The listener to remove from the audio system.</param>
        /// <exception cref="System.ArgumentException">The provided listener was not present in the Audio System.</exception>
        internal void RemoveListener(AudioListenerComponent listener)
        {
            if(!Listeners.ContainsKey(listener))
                throw new ArgumentException("The provided listener was not present in the Audio System.");

            Listeners.Remove(listener);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            AudioEngine.Update();
        }

        // called on dispose
        protected override void Destroy()
        {
            Game.Activated -= OnActivated;
            Game.Deactivated -= OnDeactivated;

            base.Destroy();

            AudioEngine.Dispose();
            AudioEngine = null;
        }

        private void OnActivated(object sender, EventArgs e)
        {
            // resume the audio
            AudioEngine.ResumeAudio();
        }

        private void OnDeactivated(object sender, EventArgs e)
        {
            // pause the audio
            AudioEngine.PauseAudio();
            AudioEngine.Update(); // force the update of the audio to pause the Musics
        }

    }
}
