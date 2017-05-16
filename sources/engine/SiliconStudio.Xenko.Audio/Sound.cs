// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Base class for all the sounds and sound instances.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    [ContentSerializer(typeof(DataContentSerializer<Sound>))]
    [ReferenceSerializer, DataSerializerGlobal(typeof(ReferenceSerializer<Sound>), Profile = "Content")]
    [DataSerializer(typeof(SoundSerializer))]
    public class Sound : ComponentBase
    {
        /// <summary>
        /// Create the audio engine to the sound base instance.
        /// </summary>
        /// <param name="engine">A valid AudioEngine.</param>
        /// <exception cref="ArgumentNullException">The engine argument is null.</exception>
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
        public SoundInstance CreateInstance(AudioListener listener = null, bool forceLoadInMemory = false, bool useHrtf = false, float directionalFactor = 0.0f, HrtfEnvironment environment = HrtfEnvironment.Small)
        {
            if (listener == null)
            {
                listener = AudioEngine.DefaultListener;
            }

            CheckNotDisposed();

            var newInstance = new SoundInstance(this, listener, forceLoadInMemory, useHrtf) { Name = Name + " - Instance " + intancesCreationCount };

            RegisterInstance(newInstance);

            ++intancesCreationCount;

            return newInstance;
        }

        /// <summary>
        /// Gets the total length in time of the Sound.
        /// </summary>
        public TimeSpan TotalLength => TimeSpan.FromSeconds(((double)NumberOfPackets * (double)CompressedSoundSource.SamplesPerFrame) / (double)SampleRate);

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

        /// <summary>
        /// Destroys the instance.
        /// </summary>
        protected override void Destroy()
        {
            if (AudioEngine == null || AudioEngine.State == AudioEngineState.Invalidated)
                return;

            if (!StreamFromDisk)
            {
                AudioLayer.BufferDestroy(PreloadedBuffer);
            }
        }

        internal void LoadSoundInMemory()
        {
            if (PreloadedBuffer.Ptr != IntPtr.Zero) return;

            using (var soundStream = ContentManager.FileProvider.OpenStream(CompressedDataUrl, VirtualFileMode.Open, VirtualFileAccess.Read, VirtualFileShare.Read, StreamFlags.Seekable))
            using (var decoder = new Celt(SampleRate, CompressedSoundSource.SamplesPerFrame, Channels, true))
            {
                var reader = new BinarySerializationReader(soundStream);
                var samplesPerPacket = CompressedSoundSource.SamplesPerFrame * Channels;

                PreloadedBuffer = AudioLayer.BufferCreate(samplesPerPacket * NumberOfPackets * sizeof(short));

                var memory = new UnmanagedArray<short>(samplesPerPacket * NumberOfPackets);

                var offset = 0;
                var outputBuffer = new short[samplesPerPacket];
                for (var i = 0; i < NumberOfPackets; i++)
                {
                    var len = reader.ReadInt16();
                    var compressedBuffer = reader.ReadBytes(len);
                    var samplesDecoded = decoder.Decode(compressedBuffer, len, outputBuffer);
                    memory.Write(outputBuffer, offset, 0, samplesDecoded * Channels);
                    offset += samplesDecoded * Channels * sizeof(short);
                }

                AudioLayer.BufferFill(PreloadedBuffer, memory.Pointer, memory.Length * sizeof(short), SampleRate, Channels == 1);
                memory.Dispose();
            }
        }
    }
}
