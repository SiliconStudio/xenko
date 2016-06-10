// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_IOS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AudioToolbox;
using AudioUnit;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Audio
{
    internal unsafe class AudioVoice : ComponentBase
    {
        public const int AudioUnitOutputSampleRate = 44100;

        private const int MaxNumberOfTracks = 16;
        private static readonly Logger Log = GlobalLogger.GetLogger("AudioVoice");
        private static readonly object StaticMembersLock = new object();
        private static AUGraph audioGraph;

        private static Queue<uint> availableMixerBusIndices;

        private static int nbOfInstances;
        private static AudioUnit.AudioUnit unit3DMixer;
        private static AudioUnit.AudioUnit unitChannelMixer;
        private readonly AudioEngine audioEngine;
        private readonly int channels;
        private readonly AudioDataRendererInfo* pAudioDataRendererInfo;
        private readonly int sampleRate;
        private readonly SoundInstance soundInstance;

        private bool is3D;

        private float pazimuth, pelevation, pdistance, pplayRate;

        public AudioVoice(AudioEngine engine, SoundInstance instance, int sampleRate, int channels)
        {
            if (engine == null) throw new ArgumentNullException(nameof(engine));

            this.channels = channels;
            this.sampleRate = sampleRate;

            audioEngine = engine;
            soundInstance = instance;
            BusIndexMixer = uint.MaxValue;

            lock (StaticMembersLock)
            {
                if (nbOfInstances == 0)
                {
                    // Create the Audio Graph
                    audioGraph = new AUGraph();

                    // Open the graph (does not initialize it yet)
                    audioGraph.Open();

                    // Create the AudioComponentDescrition corresponding to the IO Remote output and MultiChannelMixer 
                    var remoteInOutComponentDesc = AudioComponentDescription.CreateOutput(AudioTypeOutput.Remote);
                    var mixerMultiChannelComponentDesc = AudioComponentDescription.CreateMixer(AudioTypeMixer.MultiChannel);
                    var mixer3DComponentDesc = AudioComponentDescription.CreateMixer(AudioTypeMixer.Spacial);

                    // Add the Audio Unit nodes to the AudioGraph
                    var outputUnitNodeId = audioGraph.AddNode(remoteInOutComponentDesc);
                    var idChannelMixerNode = audioGraph.AddNode(mixerMultiChannelComponentDesc);
                    var id3DMixerNode = audioGraph.AddNode(mixer3DComponentDesc);

                    // Connect the nodes together
                    CheckGraphError(audioGraph.ConnnectNodeInput(idChannelMixerNode, 0, outputUnitNodeId, 0), "Connection of the graph node failed.");
                    CheckGraphError(audioGraph.ConnnectNodeInput(id3DMixerNode, 0, idChannelMixerNode, MaxNumberOfTracks), "Connection of the graph node failed.");

                    // Get the MixerUnit objects
                    unitChannelMixer = audioGraph.GetNodeInfo(idChannelMixerNode);
                    unit3DMixer = audioGraph.GetNodeInfo(id3DMixerNode);

                    // Set the mixers' output formats (the stream format is propagated along the linked input during the graph initialization)
                    var desiredSampleRate = (engine.AudioSampleRate != 0) ? engine.AudioSampleRate : AudioUnitOutputSampleRate;
                    unit3DMixer.SetFormat(CreateLinear16BitsPcm(2, desiredSampleRate), AudioUnitScopeType.Output);
                    unitChannelMixer.SetFormat(CreateLinear16BitsPcm(2, desiredSampleRate), AudioUnitScopeType.Output);

                    // set the element count to the max number of possible tracks before initializing the audio graph
                    CheckUnitStatus(unitChannelMixer.SetElementCount(AudioUnitScopeType.Input, MaxNumberOfTracks + 1), $"Failed to set element count on ChannelMixer [{MaxNumberOfTracks + 1}]"); // +1 for the 3DMixer output
                    CheckUnitStatus(unit3DMixer.SetElementCount(AudioUnitScopeType.Input, MaxNumberOfTracks), $"Failed to set element count on 3DMixer [{MaxNumberOfTracks}]");

                    // set a null renderer callback to the channel and 3d mixer input bus
                    for (uint i = 0; i < MaxNumberOfTracks; i++)
                    {
                        CheckUnitStatus((AudioUnitStatus)Native.AudioUnitHelpers.SetInputRenderCallbackToNull(unit3DMixer.Handle, i), "Failed to set the render callback");
                        CheckUnitStatus((AudioUnitStatus)Native.AudioUnitHelpers.SetInputRenderCallbackToNull(unitChannelMixer.Handle, i), "Failed to set the render callback");
                    }

                    // Initialize the graph (validation of the topology)
                    CheckGraphError(audioGraph.Initialize(), "The audio graph initialization failed.");

                    // Start audio rendering
                    CheckGraphError(audioGraph.Start(), "Audio Graph could not start.");

                    // disable all the input bus at the beginning
                    for (uint i = 0; i < MaxNumberOfTracks; i++)
                    {
                        CheckUnitStatus(unitChannelMixer.SetParameter(AudioUnitParameterType.MultiChannelMixerEnable, 0f, AudioUnitScopeType.Input, i), "Failed to enable/disable the ChannelMixerInput.");
                        CheckUnitStatus(unit3DMixer.SetParameter(AudioUnitParameterType.Mixer3DEnable, 0f, AudioUnitScopeType.Input, i), "Failed to enable/disable the 3DMixerInput.");
                    }

                    // At initialization all UnitElement are available.
                    availableMixerBusIndices = new Queue<uint>();
                    for (uint i = 0; i < MaxNumberOfTracks; i++)
                        availableMixerBusIndices.Enqueue(i);
                }
                ++nbOfInstances;
            }

            // Create a AudioDataRendererInfo for the sounds.
            pAudioDataRendererInfo = (AudioDataRendererInfo*)Native.AudioUnitHelpers.CreateAudioDataRenderer();
            pAudioDataRendererInfo->HandleChannelMixer = unitChannelMixer.Handle;
            pAudioDataRendererInfo->Handle3DMixer = unit3DMixer.Handle;
        }

        /// <summary>
        /// The index of the mixer bus used by this instance.
        /// </summary>
        internal uint BusIndexMixer { get; private set; }

        private bool Is3D
        {
            get { return is3D; }
            set
            {
                if (value != is3D)
                {
                    is3D = value;
                    EnableMixerCurrentInput(pAudioDataRendererInfo->IsEnabled2D || pAudioDataRendererInfo->IsEnabled3D);
                }
            }
        }

        public void Apply3D(float azimut, float elevation, float distance, float playRate)
        {
            Set3DParameters(azimut, elevation, distance, playRate);
            Is3D = true;
        }

        public bool DidVoicePlaybackEnd()
        {
            return pAudioDataRendererInfo->PlaybackEnded;
        }

        public void LoadBuffer()
        {
            // set render info data 
            if (!soundInstance.Sound.StreamFromDisk) //this is ok for preloaded, but in case of stream we don't write that yet
            {
                //only add one buffer in this case
                Native.AudioUnitHelpers.AddAudioBuffer((IntPtr)pAudioDataRendererInfo, soundInstance.Sound.PreloadedData.Pointer, channels, soundInstance.Sound.PreloadedData.Length/channels);
            }
            else
            {
                for (var i = 0; i < SoundSource.NumberOfBuffers; i++)
                {
                    
                }
            }

            // reset playback to the beginning of the track and set the looping status            
            SetLoopingPoints(0, int.MaxValue, 0, pAudioDataRendererInfo->IsInfiniteLoop);
        }

        public void Pause()
        {
            EnableMixerCurrentInput(false);
        }

        public void Play()
        {
            pAudioDataRendererInfo->PlaybackEnded = false;

            EnableMixerCurrentInput(true);
        }

        public void PreparePlay()
        {
            BusIndexMixer = uint.MaxValue; // invalid index value (when no bus are free)

            // find a available bus indices for this instance.
            if (availableMixerBusIndices.Count > 0)
            {
                BusIndexMixer = availableMixerBusIndices.Dequeue();
            }
            else
            {
                // force the update of all sound effect instance to free bus indices
                audioEngine.ForceSoundInstanceUpdate();

                // retry to get an free bus index
                if (availableMixerBusIndices.Count > 0)
                {
                    BusIndexMixer = availableMixerBusIndices.Dequeue();
                }
                else // try to stop another instance
                {
                    // try to get a sound effect to stop
                    var soundEffectToStop = audioEngine.GetLeastSignificativeSound();
                    if (soundEffectToStop == null) // no available sound effect to stop -> give up the creation of the track
                        return;

                    // stop the sound effect instances and retry to get an available track
                    soundEffectToStop.StopAllInstances();

                    // retry to get an free bus index
                    if (availableMixerBusIndices.Count > 0)
                        BusIndexMixer = availableMixerBusIndices.Dequeue();
                    else
                        return;
                }
            }

            // Set the audio stream format of the current mixer input bus.
            unitChannelMixer.SetFormat(CreateLinear16BitsPcm(channels, sampleRate), AudioUnitScopeType.Input, BusIndexMixer);

            // set the channel input bus callback
            CheckUnitStatus((AudioUnitStatus)Native.AudioUnitHelpers.SetInputRenderCallbackToChannelMixerDefault(unitChannelMixer.Handle, BusIndexMixer, (IntPtr)pAudioDataRendererInfo), "Failed to set the render callback");

            SetVolume(soundInstance.Volume);
            if (!Is3D) SetPan(soundInstance.Pan);

            // initialize the 3D mixer input bus, if the sound can be used as 3D sound.
            if (channels == 1 && soundInstance.Sound.Spatialized)
            {
                // Set the audio stream format of the current mixer input bus.
                unit3DMixer.SetFormat(CreateLinear16BitsPcm(channels, sampleRate), AudioUnitScopeType.Input, BusIndexMixer);

                // set the 3D mixer input bus callback
                CheckUnitStatus((AudioUnitStatus)Native.AudioUnitHelpers.SetInputRenderCallbackTo3DMixerDefault(unit3DMixer.Handle, BusIndexMixer, (IntPtr)pAudioDataRendererInfo), "Failed to set the render callback");

                Set3DParameters(pazimuth, pelevation, pdistance, pplayRate);
            }

            // Disable the input by default so that it started in Stopped mode.
            EnableMixerCurrentInput(false);
        }

        public void Reset3D()
        {
            if (Is3D)
            {
                Set3DParameters(0, 0, 0, 1);
                Is3D = false;
            }
        }

        /// <summary>
        /// Set the loop points of the AudioVoice.
        /// </summary>
        /// <param name="startPoint">The beginning of the loop in frame number.</param>
        /// <param name="endPoint">The end of the loop in frame number.</param>
        /// <param name="loopsNumber">The number of times to loop.</param>
        /// <param name="loopInfinite">Should loop infinitely or not</param>
        public void SetLoopingPoints(int startPoint, int endPoint, int loopsNumber, bool loopInfinite)
        {
            pAudioDataRendererInfo->LoopStartPoint = Math.Max(0, startPoint);
            pAudioDataRendererInfo->LoopEndPoint = endPoint;

            pAudioDataRendererInfo->IsInfiniteLoop = loopInfinite;
            pAudioDataRendererInfo->NumberOfLoops = Math.Max(0, loopsNumber);
        }

        public void SetPan(float pan)
        {
            if (BusIndexMixer == uint.MaxValue)
                return;

            Is3D = false;
            CheckUnitStatus(unitChannelMixer.SetParameter(AudioUnitParameterType.MultiChannelMixerPan, pan, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the mixer bus Pan parameter.");
        }

        public void SetVolume(float volume)
        {
            if (BusIndexMixer == uint.MaxValue)
                return;

            if (Is3D)
            {
                var gain = Math.Max(-120f, (float)(20 * Math.Log10(volume)));
                CheckUnitStatus(unit3DMixer.SetParameter(AudioUnitParameterType.Mixer3DGain, gain, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the Gain parameter of the 3D mixer");
            }
            else
            {
                CheckUnitStatus(unitChannelMixer.SetParameter(AudioUnitParameterType.MultiChannelMixerVolume, volume, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the mixer bus Volume parameter.");
            }
        }

        public void Stop()
        {
            EnableMixerCurrentInput(false);
            Native.AudioUnitHelpers.SetAudioBufferFrame((IntPtr)pAudioDataRendererInfo, 0, 0);

            // free the input bus for other sound effects.
            if (BusIndexMixer != uint.MaxValue)
            {
                // reset the mixer callbacks to null renderers
                CheckUnitStatus((AudioUnitStatus)Native.AudioUnitHelpers.SetInputRenderCallbackToNull(unit3DMixer.Handle, BusIndexMixer), "Failed to set the render callback");
                CheckUnitStatus((AudioUnitStatus)Native.AudioUnitHelpers.SetInputRenderCallbackToNull(unitChannelMixer.Handle, BusIndexMixer), "Failed to set the render callback");

                availableMixerBusIndices.Enqueue(BusIndexMixer);
            }
        }

        protected override void Destroy()
        {
            base.Destroy();

            Native.AudioUnitHelpers.DestroyAudioDataRenderer((IntPtr)pAudioDataRendererInfo);

            lock (StaticMembersLock)
            {
                --nbOfInstances;
                if (nbOfInstances == 0)
                {
                    CheckGraphError(audioGraph.Stop(), "The audio Graph failed to stop.");
                    audioGraph.Dispose();
                }
            }
        }

        private static void CheckGraphError(AUGraphError error, string msg)
        {
            if (error != AUGraphError.OK)
                throw new AudioSystemInternalException(msg + " [Error=" + error + "].");
        }

        private static void CheckUnitStatus(AudioUnitStatus status, string msg)
        {
            if (status != AudioUnitStatus.OK)
            {
                Log.Error("Audio Error [{0} / {1}]. Voices: {2}", msg, status, nbOfInstances);
                throw new AudioSystemInternalException(msg + " [Error=" + status + "].");
            }   
        }

        /// <summary>
        /// Create the audio stream format for 16bits PCM data.
        /// </summary>
        /// <param name="numberOfChannels"></param>
        /// <param name="sampleRate"></param>
        /// <param name="isInterleaved"></param>
        /// <returns></returns>
        private static AudioStreamBasicDescription CreateLinear16BitsPcm(int numberOfChannels, double sampleRate, bool isInterleaved = true)
        {
            AudioStreamBasicDescription retFormat;
            const int wordSize = 2;

            retFormat.FramesPerPacket = 1;
            retFormat.Format = AudioFormatType.LinearPCM;
            retFormat.FormatFlags = AudioFormatFlags.IsPacked | AudioFormatFlags.IsSignedInteger;
            retFormat.SampleRate = sampleRate;
            retFormat.BitsPerChannel = 8 * wordSize;
            retFormat.ChannelsPerFrame = numberOfChannels;
            retFormat.BytesPerFrame = isInterleaved ? numberOfChannels * wordSize : wordSize;
            retFormat.BytesPerPacket = retFormat.FramesPerPacket * retFormat.BytesPerFrame;
            retFormat.Reserved = 0;

            if (!isInterleaved)
                retFormat.FormatFlags |= AudioFormatFlags.IsNonInterleaved;

            return retFormat;
        }

        private void EnableMixerCurrentInput(bool shouldBeEnabled)
        {
            Debugger.Break();

            if(BusIndexMixer == uint.MaxValue)
                return;

            CheckUnitStatus(unitChannelMixer.SetParameter(AudioUnitParameterType.MultiChannelMixerEnable, !Is3D && shouldBeEnabled ? 1f : 0f, AudioUnitScopeType.Input, BusIndexMixer), 
                "Failed to enable/disable the ChannelMixerInput.");

            if(channels == 1 && soundInstance.Sound.Spatialized) // no 3D mixer for stereo sounds
                CheckUnitStatus(unit3DMixer.SetParameter(AudioUnitParameterType.Mixer3DEnable, Is3D && shouldBeEnabled ? 1f : 0f, AudioUnitScopeType.Input, BusIndexMixer), 
                    "Failed to enable/disable the 3DMixerInput.");

            pAudioDataRendererInfo->IsEnabled2D = shouldBeEnabled && !Is3D;
            pAudioDataRendererInfo->IsEnabled3D = shouldBeEnabled && Is3D;
        }

        private void Set3DParameters(float azimuth, float elevation, float distance, float playRate)
        {
            pazimuth = azimuth;
            pelevation = elevation;
            pdistance = distance;
            pplayRate = playRate;

            if (BusIndexMixer == uint.MaxValue)
                return;

            CheckUnitStatus(unit3DMixer.SetParameter(AudioUnitParameterType.Mixer3DAzimuth, azimuth, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the Azimuth parameter of the 3D mixer");
            CheckUnitStatus(unit3DMixer.SetParameter(AudioUnitParameterType.Mixer3DElevation, elevation, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the Elevation parameter of the 3D mixer");
            CheckUnitStatus(unit3DMixer.SetParameter(AudioUnitParameterType.Mixer3DDistance, distance, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the Distance parameter of the 3D mixer");
            CheckUnitStatus(unit3DMixer.SetParameter(AudioUnitParameterType.Mixer3DPlaybackRate, playRate, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the PlayRate parameter of the 3D mixer");
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AudioDataRendererInfo
        {
            public int LoopStartPoint;
            public int LoopEndPoint;
            public int NumberOfLoops;
            public bool IsInfiniteLoop;

            public bool IsEnabled2D;
            public bool IsEnabled3D;

            public bool PlaybackEnded;

            public IntPtr HandleChannelMixer;
            public IntPtr Handle3DMixer;
        }
    }
}

#endif
