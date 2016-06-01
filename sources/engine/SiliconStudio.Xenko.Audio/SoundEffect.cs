// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Audio.Wave;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// This class provides a loaded sound resource which is localizable in the 3D scene. 
    /// <para>SoundEffects are usually short sounds that are localized and need to be played with very low latency. 
    /// Classical examples are gun shots, foot steps, etc... Sound effects can be looped to seem longer.
    /// Its is not recommended to use SoundEffect to play long track as the memory required will be considerable.
    /// For that use please refer to the <see cref="SoundMusic"/> class.</para>
    /// <para>You can then create multiple instances of that sound resource by calling <see cref="CreateInstance"/>.</para>
    /// </summary>
    /// <remarks>
    /// <para>A SoundEffect contains the audio data and metadata (such as wave data and data format) loaded from a sound file. 
    /// You can create multiple <see cref="SoundEffectInstance"/> objects, and play and localize them independently.
    /// All these objects share the resources of that SoundEffect. 
    /// For convenience purposes a default <see cref="SoundEffectInstance"/> is automatically created and associated to each SoundEffect, 
    /// so that you can directly play them without having the need to create a first instance.</para>
    /// <para>
    /// You can create a SoundEffect by calling the static <see cref="Load"/> load function. 
    /// Currently only wav files are supported for soundEffects.
    ///  </para>
    /// <para>
    /// The only limit to the number of loaded SoundEffect objects is memory. 
    /// A loaded SoundEffect will continue to hold its memory resources throughout its lifetime. 
    /// All <see cref="SoundEffectInstance"/> objects created from a SoundEffect share memory resources. 
    /// When a SoundEffect object is destroyed, all <see cref="SoundEffectInstance"/> objects previously created by that SoundEffect stop playing and become invalid.
    /// </para>
    /// <para>
    /// Disposing a SoundEffect object, stops playing all the children <see cref="SoundEffectInstance"/> and then dispose them.
    /// </para>
    /// </remarks>
    /// <seealso cref="SoundEffectInstance"/>
    /// <seealso cref="SoundMusic"/>
    /// <seealso cref="IPositionableSound"/>    
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<SoundEffect>))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<SoundEffect>), Profile = "Content")]
    public sealed partial class SoundEffect : SoundBase, IPositionableSound
    {
        public List<CompressedSoundPacket> Packets;

        public int SampleRate { get; set; } = 44100;

        public int SamplesSize { get; set; } = 1024;

        public int Channels { get; set; } = 2;

        [DataMemberIgnore]
        internal UnmanagedArray<short> PreloadedData;

        public void Init()
        {
            Name = "Sound Effect " + Interlocked.Add(ref soundEffectCreationCount, 1);

            AdaptAudioDataImpl();

            // register the sound to the AudioEngine so that it will be properly freed if AudioEngine is disposed before this.
            AudioEngine.RegisterSound(this);

            //create a default instance only when actually we load, previously we were creating this on demand which would result sometimes in useless creations and bugs within the editor         
            DefaultInstance = CreateInstance();

            //copy back values we might have set before default instance creation
            DefaultInstance.Pan = defaultPan;
            DefaultInstance.Volume = defaultVolume;
            DefaultInstance.IsLooped = defaultIsLooped;
        }

        public void Preload()
        {
            var decoder = new Celt(SampleRate, SamplesSize, Channels, true);

            var samplesPerPacket = SamplesSize * Channels;

            PreloadedData = new UnmanagedArray<short>(samplesPerPacket * Packets.Count);

            var offset = 0;  
            var buffer = new short[samplesPerPacket];
            foreach (var compressedSoundPacket in Packets)
            {
                decoder.Decode(compressedSoundPacket.Data, buffer);
                PreloadedData.Write(buffer, offset);
                offset += samplesPerPacket;
            }
        }

        /// <summary>
        /// The number of SoundEffect Created so far. Used only to give a unique name to the SoundEffect.
        /// </summary>
        private static int soundEffectCreationCount;
        /// <summary>
        /// The number of Instances Created so far by this SoundEffect. Used only to give a unique name to the SoundEffectInstance.
        /// </summary>
        private int intancesCreationCount;

        /// <summary>
        /// Create a new sound effect instance of the sound effect. 
        /// The audio data are shared between the instances so that useless memory copies is avoided. 
        /// Each instance that can be played and localized independently from others.
        /// </summary>
        /// <returns>A new sound instance</returns>
        /// <exception cref="ObjectDisposedException">The sound has already been disposed</exception>
        public SoundEffectInstance CreateInstance()
        {
            CheckNotDisposed();

            var newInstance = new SoundEffectInstance(this) { Name = Name + " - Instance " + intancesCreationCount };

            RegisterInstance(newInstance);

            ++intancesCreationCount;

            return newInstance;
        }

        /// <summary>
        /// Current instances of the SoundEffect.
        /// We need to keep track of them to stop and dispose them when the soundEffect is disposed.
        /// </summary>
        internal readonly List<SoundEffectInstance> Instances = new List<SoundEffectInstance>(); 

        /// <summary>
        /// Register a new instance to the soundEffect.
        /// </summary>
        /// <param name="instance">new instance to register.</param>
        private void RegisterInstance(SoundEffectInstance instance)
        {
            Instances.Add(instance);
        }

        /// <summary>
        /// Stop all registered instances of the <see cref="SoundEffect"/>.
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
        internal void StopConcurrentInstances(SoundEffectInstance mainInstance)
        {
            foreach (var instance in Instances)
            {
                if(instance != mainInstance)
                    instance.Stop();
            }
        }

        /// <summary>
        /// Unregister a disposed Instance.
        /// </summary>
        /// <param name="instance"></param>
        internal void UnregisterInstance(SoundEffectInstance instance)
        {
            if(!Instances.Remove(instance))
                throw new AudioSystemInternalException("Tried to unregister soundEffectInstance while not contained in the instance list.");
        }

        // Create an underlying Instance to avoid re-writing Interface functions.
        private SoundEffectInstance DefaultInstance { get; set; }

        #region Interface Implementation using underlying SoundEffectInstance

        private float defaultPan; //0

        public float Pan
        {
            get
            {
                return DefaultInstance?.Pan ?? defaultPan;
            }
            set
            {
                defaultPan = value;

                if (DefaultInstance != null)
                {
                    DefaultInstance.Pan = defaultPan;
                }
            }
        }

        private float defaultVolume = 1.0f;
        
        public float Volume
        {
            get
            {
                return DefaultInstance?.Volume ?? defaultVolume;
            }
            set
            {
                defaultVolume = value;

                if (DefaultInstance != null)
                {
                    DefaultInstance.Volume = defaultVolume;
                }
            }
        }

        private bool defaultIsLooped; //false

        public bool IsLooped
        {
            get
            {
                return DefaultInstance?.IsLooped ?? defaultIsLooped;
            }
            set
            {
                defaultIsLooped = value;

                if (DefaultInstance != null)
                {
                    DefaultInstance.IsLooped = defaultIsLooped;
                }               
            }
        }

        public SoundPlayState PlayState => DefaultInstance?.PlayState ?? SoundPlayState.Stopped;

        public void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            DefaultInstance.Apply3D(listener, emitter);
        }

        public void Play()
        {
            DefaultInstance.Play();
        }

        public void Pause()
        {
            DefaultInstance.Pause();
        }

        public void Stop()
        {
            DefaultInstance.Stop();
        }

        public void ExitLoop()
        {
            DefaultInstance.ExitLoop();
        }

        public void Reset3D()
        {
            DefaultInstance.Reset3D();
        }

        protected override void Destroy()
        {
            base.Destroy();

            if (IsDisposed)
                return;

            // Stop and dispose all the instances
            foreach (var seInstance in Instances.ToArray())
                seInstance.Dispose();

            DestroyImpl();

            // Unregister this from the sound to dispose by the audio engine
            AudioEngine.UnregisterSound(this);

            PreloadedData?.Dispose();
        }
        
        #endregion
    }
}
