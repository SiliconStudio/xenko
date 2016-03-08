// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Audio.Wave;

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
    [DataSerializerGlobal(typeof(ReferenceSerializer<SoundEffect>), Profile = "Content")]
    [DataSerializer(typeof(NullSerializer<SoundEffect>))]
    public sealed partial class SoundEffect : SoundBase, IPositionableSound
    {
        internal WaveFormat WaveFormat { get; private set; }

        private IntPtr nativeDataBuffer;

        internal IntPtr WaveDataPtr { get; private set; }

        internal int WaveDataSize { get; private set; }

        /// <summary>
        /// Create and Load a sound effect from an input wav stream.
        /// </summary>
        /// <param name="engine">Name of the audio engine in which to create the sound effect</param>
        /// <param name="stream">A stream corresponding to a wav file.</param>
        /// <returns>A new instance soundEffect ready to be played</returns>
        /// <exception cref="ArgumentNullException"><paramref name="engine"/> or <paramref name="stream"/> is null.</exception>
        /// <exception cref="NotSupportedException">The wave file or has more than 2 channels or is not encoded in 16bits.</exception>
        /// <exception cref="InvalidOperationException">The content of the stream does not correspond to a valid wave file.</exception>
        /// <exception cref="OutOfMemoryException">There is not enough memory anymore to load the specified file in memory. </exception>
        /// <exception cref="ObjectDisposedException">The audio engine has already been disposed</exception>
        /// <remarks>Supported WAV files' audio format is the 16bits PCM format.</remarks>
        public static SoundEffect Load(AudioEngine engine, Stream stream)
        {
            if(engine == null)
                throw new ArgumentNullException("engine");

            var newSdEff = new SoundEffect();
            newSdEff.AttachEngine(engine);
            newSdEff.Load(stream);

            return newSdEff;
        }

        internal void Load(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (AudioEngine.IsDisposed)
                throw new ObjectDisposedException("Audio Engine");

            // create a native memory stream to extract the lz4 audio stream.
            nativeDataBuffer = Utilities.AllocateMemory((int)stream.Length);

            var nativeStream = new NativeMemoryStream(nativeDataBuffer, stream.Length);
            stream.CopyTo(nativeStream);
            nativeStream.Position = 0;

            var waveStreamReader = new SoundStream(nativeStream);
            var waveFormat = waveStreamReader.Format;

            if (waveFormat.Channels > 2)
                throw new NotSupportedException("The wave file contains more than 2 data channels. Only mono and stereo formats are currently supported.");

            if (waveFormat.Encoding != WaveFormatEncoding.Pcm || waveFormat.BitsPerSample != 16)
                throw new NotSupportedException("The wave file audio format is not supported. Only 16bits PCM encoded formats are currently supported.");

            WaveFormat = waveFormat;
            WaveDataPtr = nativeDataBuffer + (int)nativeStream.Position;
            WaveDataSize = (int)waveStreamReader.Length;
            Name = "Sound Effect " + soundEffectCreationCount;

            AdaptAudioDataImpl();

            // register the sound to the AudioEngine so that it will be properly freed if AudioEngine is disposed before this.
            AudioEngine.RegisterSound(this);

            Interlocked.Increment(ref soundEffectCreationCount);
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
        private SoundEffectInstance DefaultInstance
        {
            get { return defaultInstance ?? (defaultInstance = CreateInstance()); }
        }
        private SoundEffectInstance defaultInstance;

        // for serialization
        internal SoundEffect()
        {
        }

        #region Interface Implementation using underlying SoundEffectInstance

        public float Pan
        {
            get { return DefaultInstance.Pan; }
            set { DefaultInstance.Pan = value; }
        }
        
        public float Volume
        {
            get { return DefaultInstance.Volume; }
            set { DefaultInstance.Volume = value; }
        }

        public void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            DefaultInstance.Apply3D(listener, emitter);
        }

        public SoundPlayState PlayState
        {
            get { return DefaultInstance.PlayState; }
        }

        public bool IsLooped
        {
            get { return DefaultInstance.IsLooped; }
            set { DefaultInstance.IsLooped = value; }
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

            // free the audio data.
            if (nativeDataBuffer != IntPtr.Zero)
            {
                Utilities.FreeMemory(nativeDataBuffer);
                nativeDataBuffer = IntPtr.Zero;
                WaveDataPtr = IntPtr.Zero;
                WaveDataSize = 0;
            }

            // Unregister this from the sound to dispose by the audio engine
            AudioEngine.UnregisterSound(this);
        }

        private unsafe void DuplicateTracks(IntPtr waveDataPtr, IntPtr newWaveDataPtr, int newWaveDataSize)
        {
            var pInputCurrent = (short*)waveDataPtr;
            var pOutputCurrent = (short*)newWaveDataPtr;
            var numberOfIters = newWaveDataSize / sizeof(short);

            var count = 0;
            while (count < numberOfIters)
            {
                *pOutputCurrent++ = *pInputCurrent;
                *pOutputCurrent++ = *pInputCurrent;

                count += 2;
                ++pInputCurrent;
            }
        }

        private unsafe void UpSampleByTwo(IntPtr waveDataPtr, IntPtr newWaveDataPtr, int newWaveDataSize, int nbOfChannels, bool convertToStereo)
        {
            var pInputCurrent = (short*)waveDataPtr;
            var pOutputCurrent = (short*)newWaveDataPtr;
            var numberOfIters = newWaveDataSize / sizeof(short);

            var count = 0;
            while (true)
            {
                for (int i = 0; i < nbOfChannels; i++)
                {
                    if (count >= numberOfIters)
                        return;

                    *pOutputCurrent = *pInputCurrent;

                    ++count;
                    ++pOutputCurrent;
                    ++pInputCurrent;
                }
                if (convertToStereo)
                {
                    *pOutputCurrent = pOutputCurrent[-1];
                    ++count;
                    ++pOutputCurrent;
                }
                for (int i = 0; i < nbOfChannels; i++)
                {
                    if (count >= numberOfIters)
                        return;

                    *pOutputCurrent = (short)((pInputCurrent[0] + pInputCurrent[-nbOfChannels]) >> 1);

                    ++count;
                    ++pOutputCurrent;
                    ++pInputCurrent;
                }
                if (convertToStereo)
                {
                    *pOutputCurrent = pOutputCurrent[-1];
                    ++count;
                    ++pOutputCurrent;
                }

                pInputCurrent -= nbOfChannels;
            }
        }

        private unsafe void UpSample(IntPtr waveDataPtr, IntPtr newWaveDataPtr, int newWaveDataSize, float sampleRateRatio, int nbOfChannels, bool convertToStereo)
        {
            var pInputCurrent = (short*)waveDataPtr;
            var pOutputCurrent = (short*)newWaveDataPtr;
            var numberOfIters = newWaveDataSize / sizeof(short);

            var count = 0;
            var currentPosition = 0f;
            while (true)
            {
                for (int i = 0; i < nbOfChannels; i++)
                {
                    if (count >= numberOfIters)
                        return;

                    *pOutputCurrent = (short)((1 - currentPosition) * pInputCurrent[i] + currentPosition * pInputCurrent[i + nbOfChannels]);

                    ++count;
                    ++pOutputCurrent;
                }
                if (convertToStereo)
                {
                    *pOutputCurrent = pOutputCurrent[-1];
                    ++count;
                    ++pOutputCurrent;
                }

                currentPosition += sampleRateRatio;

                if (currentPosition >= 1)
                {
                    pInputCurrent += nbOfChannels;
                    currentPosition -= 1;
                }
            }
        }
        
        #endregion
    }
}
