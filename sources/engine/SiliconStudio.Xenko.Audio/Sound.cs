// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Base class for all the sounds and sound instances.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    [ContentSerializer(typeof(DataContentSerializer<Sound>))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<Sound>), Profile = "Content")]
    [DataSerializer(typeof(SoundSerializer))]
    public class Sound : ComponentBase
    {
        /// <summary>
        /// Create the audio engine to the sound base instance.
        /// </summary>
        /// <param name="engine">A valid AudioEngine</param>
        /// <exception cref="ArgumentNullException">The engine argument is null</exception>
        internal void AttachEngine(AudioEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));

            AudioEngine = engine;
        }

        [DataMemberIgnore]
        internal AudioEngine AudioEngine { get; private set; }

        /// <summary>
        /// Current instances of the SoundEffect.
        /// We need to keep track of them to stop and dispose them when the soundEffect is disposed.
        /// </summary>
        [DataMemberIgnore]
        internal readonly List<SoundInstance> Instances = new List<SoundInstance>();

        [DataMemberIgnore]
        internal AudioLayer.Buffer PreloadedBuffer;

        /// <summary>
        /// The number of SoundEffect Created so far. Used only to give a unique name to the SoundEffect.
        /// </summary>
        private static int soundEffectCreationCount;

        /// <summary>
        /// The number of Instances Created so far by this SoundEffect. Used only to give a unique name to the SoundEffectInstance.
        /// </summary>
        private int intancesCreationCount;

        internal int Channels { get; set; } = 2;

        internal string CompressedDataUrl { get; set; }

        [DataMemberIgnore]
        internal AudioEngineState EngineState => AudioEngine.State;

        internal int MaxPacketLength { get; set; }

        internal int NumberOfPackets { get; set; }

        internal int SampleRate { get; set; } = 44100;

        internal bool Spatialized { get; set; }

        internal bool StreamFromDisk { get; set; }

        /// <summary>
        /// Create a new sound effect instance of the sound effect. 
        /// The audio data are shared between the instances so that useless memory copies is avoided. 
        /// Each instance that can be played and localized independently from others.
        /// </summary>
        /// <returns>A new sound instance</returns>
        /// <exception cref="ObjectDisposedException">The sound has already been disposed</exception>
        public SoundInstance CreateInstance(AudioListener listener = null)
        {
            if (listener == null)
            {
                listener = AudioEngine.DefaultListener;
            }

            CheckNotDisposed();

            var newInstance = new SoundInstance(this, listener) { Name = Name + " - Instance " + intancesCreationCount };

            RegisterInstance(newInstance);

            ++intancesCreationCount;

            return newInstance;
        }

        internal void Attach(AudioEngine engine)
        {
            AttachEngine(engine);

            Name = "Sound Effect " + Interlocked.Add(ref soundEffectCreationCount, 1);

            // register the sound to the AudioEngine so that it will be properly freed if AudioEngine is disposed before this.
            AudioEngine.RegisterSound(this);
        }

        internal void CheckNotDisposed()
        {
            if(IsDisposed)
                throw new ObjectDisposedException("this");
        }

        /// <summary>
        /// Stop all registered instances of the <see cref="Sound"/>.
        /// </summary>
        internal void StopAllInstances()
        {
            foreach (var instance in Instances)
                instance.Stop();
        }

        /// <summary>
        /// Stop all registered instances different from the provided main instance
        /// </summary>
        /// <param name="mainInstance">The main instance of the sound effect</param>
        internal void StopConcurrentInstances(SoundInstance mainInstance)
        {
            foreach (var instance in Instances)
            {
                if (instance != mainInstance)
                    instance.Stop();
            }
        }

        /// <summary>
        /// Unregister a disposed Instance.
        /// </summary>
        /// <param name="instance"></param>
        internal void UnregisterInstance(SoundInstance instance)
        {
            if (!Instances.Remove(instance))
                throw new AudioSystemInternalException("Tried to unregister soundEffectInstance while not contained in the instance list.");
        }

        /// <summary>
        /// Register a new instance to the soundEffect.
        /// </summary>
        /// <param name="instance">new instance to register.</param>
        private void RegisterInstance(SoundInstance instance)
        {
            Instances.Add(instance);
        }

        protected override void Destroy()
        {
            if (AudioEngine.State == AudioEngineState.Invalidated)
                return;

            if (!StreamFromDisk)
            {
                AudioLayer.BufferDestroy(PreloadedBuffer);
            }
        }
    }
}
