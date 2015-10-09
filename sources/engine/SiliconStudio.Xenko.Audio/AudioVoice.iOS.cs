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
using SiliconStudio.Paradox.Audio.Wave;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Paradox.Audio
{
    internal unsafe class AudioVoice : ComponentBase
    {
        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        private static extern int SetInputRenderCallbackToChannelMixerDefault(IntPtr inUnit, uint element, IntPtr userData);

        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        private static extern int SetInputRenderCallbackTo3DMixerDefault(IntPtr inUnit, uint element, IntPtr userData);

        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        private static extern int SetInputRenderCallbackToNull(IntPtr inUnit, uint element);

        /// <summary>
        /// The frequency of the output of the audio unit graph.
        /// </summary>
        public const int AudioUnitOutputSampleRate = 44100;

        private readonly AudioEngine audioEngine;
        private readonly SoundEffectInstance soundEffectInstance;
        private readonly WaveFormat waveFormat;
        private const int MaxNumberOfTracks = 16;

        private static readonly Logger Log = GlobalLogger.GetLogger("AudioVoice");

        private static int nbOfInstances;
        private static readonly object StaticMembersLock = new object();

        private static AUGraph audioGraph;
        private static AudioUnit.AudioUnit unitChannelMixer;
        private static AudioUnit.AudioUnit unit3DMixer;

        private readonly AudioDataRendererInfo* pAudioDataRendererInfo;

        /// <summary>
        /// The list of UnitElement index that are available for use in the 3D and channel mixer.
        /// </summary>
        private static Queue<uint> availableMixerBusIndices;

        /// <summary>
        /// Boolean indicating if the sound is a 3D sound or not (input to 3DMixer or ChannelMixer)
        /// </summary>
        private bool is3D;
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
        
        /// <summary>
        /// The index of the mixer bus used by this instance.
        /// </summary>
        internal uint BusIndexMixer { get; private set; }

        public bool DidVoicePlaybackEnd()
        {
            return pAudioDataRendererInfo->PlaybackEnded;
        }

        private static void CheckUnitStatus(AudioUnitStatus status, string msg)
        {
            if (status != AudioUnitStatus.OK)
            {
                Log.Error("Audio Error [{0} / {1}]. Voices: {2}", msg, status, nbOfInstances);
                throw new AudioSystemInternalException(msg + " [Error=" + status + "].");
            }   
        }

        private void EnableMixerCurrentInput(bool shouldBeEnabled)
        {
            if(BusIndexMixer == uint.MaxValue)
                return;

            CheckUnitStatus(unitChannelMixer.SetParameter(AudioUnitParameterType.MultiChannelMixerEnable,
                !Is3D && shouldBeEnabled ? 1f : 0f, AudioUnitScopeType.Input, BusIndexMixer), "Failed to enable/disable the ChannelMixerInput.");

            if(waveFormat.Channels == 1) // no 3D mixer for stereo sounds
                CheckUnitStatus(unit3DMixer.SetParameter(AudioUnitParameterType.Mixer3DEnable,
                    Is3D && shouldBeEnabled ? 1f : 0f, AudioUnitScopeType.Input, BusIndexMixer), "Failed to enable/disable the 3DMixerInput.");

            pAudioDataRendererInfo->IsEnabled2D = shouldBeEnabled && !Is3D;
            pAudioDataRendererInfo->IsEnabled3D = shouldBeEnabled && Is3D;
        }
        
        private static void CheckGraphError(AUGraphError error, string msg)
        {
            if (error != AUGraphError.OK)
                throw new AudioSystemInternalException(msg + " [Error=" + error + "].");
        }

        /// <summary>
        /// Create the audio stream format for 16bits PCM data.
        /// </summary>
        /// <param name="numberOfChannels"></param>
        /// <param name="frameRate"></param>
        /// <param name="isInterleaved"></param>
        /// <returns></returns>
        private static AudioStreamBasicDescription CreateLinear16BitsPcm(int numberOfChannels, double frameRate, bool isInterleaved = true)
        {
            AudioStreamBasicDescription retFormat;
            const int wordSize = 2;

            retFormat.FramesPerPacket = 1;
            retFormat.Format = AudioFormatType.LinearPCM;
            retFormat.FormatFlags = AudioFormatFlags.IsPacked | AudioFormatFlags.IsSignedInteger;
            retFormat.SampleRate = frameRate; 
            retFormat.BitsPerChannel = 8 * wordSize;
            retFormat.ChannelsPerFrame = numberOfChannels;
            retFormat.BytesPerFrame = isInterleaved ? numberOfChannels * wordSize : wordSize;
            retFormat.BytesPerPacket = retFormat.FramesPerPacket * retFormat.BytesPerFrame;
            retFormat.Reserved = 0;

            if(!isInterleaved)
                retFormat.FormatFlags |= AudioFormatFlags.IsNonInterleaved;

            return retFormat;
        }

        public AudioVoice(AudioEngine engine, SoundEffectInstance effectInstance, WaveFormat desiredFormat)
        {
            if (engine == null) throw new ArgumentNullException("engine");
            if (desiredFormat == null) throw new ArgumentNullException("desiredFormat");

            audioEngine = engine;
            soundEffectInstance = effectInstance;
            waveFormat = desiredFormat;
            BusIndexMixer = uint.MaxValue;

            if (desiredFormat.BitsPerSample != 16)
                throw new AudioSystemInternalException("Invalid Audio Format. " + desiredFormat.BitsPerSample + " bits by sample is not supported.");

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
                    unit3DMixer.SetAudioFormat(CreateLinear16BitsPcm(2, desiredSampleRate), AudioUnitScopeType.Output);
                    unitChannelMixer.SetAudioFormat(CreateLinear16BitsPcm(2, desiredSampleRate), AudioUnitScopeType.Output);

                    // set the element count to the max number of possible tracks before initializing the audio graph
                    CheckUnitStatus(unitChannelMixer.SetElementCount(AudioUnitScopeType.Input, MaxNumberOfTracks+1), string.Format("Failed to set element count on ChannelMixer [{0}]", MaxNumberOfTracks+1)); // +1 for the 3DMixer output
                    CheckUnitStatus(unit3DMixer.SetElementCount(AudioUnitScopeType.Input, MaxNumberOfTracks), string.Format("Failed to set element count on 3DMixer [{0}]", MaxNumberOfTracks));

                    // set a null renderer callback to the channel and 3d mixer input bus
                    for (uint i = 0; i < MaxNumberOfTracks; i++)
                    {
                        CheckUnitStatus((AudioUnitStatus)SetInputRenderCallbackToNull(unit3DMixer.Handle, i), "Failed to set the render callback");
                        CheckUnitStatus((AudioUnitStatus)SetInputRenderCallbackToNull(unitChannelMixer.Handle, i), "Failed to set the render callback");
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

                // Create a AudioDataRendererInfo for the sounds.
                pAudioDataRendererInfo = (AudioDataRendererInfo*)Utilities.AllocateClearedMemory(sizeof(AudioDataRendererInfo));
                pAudioDataRendererInfo->HandleChannelMixer = unitChannelMixer.Handle;
                pAudioDataRendererInfo->Handle3DMixer = unit3DMixer.Handle;
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
            pAudioDataRendererInfo->LoopEndPoint = Math.Min(pAudioDataRendererInfo->TotalNumberOfFrames, endPoint);

            pAudioDataRendererInfo->IsInfiniteLoop = loopInfinite;
            pAudioDataRendererInfo->NumberOfLoops = Math.Max(0, loopsNumber);
        }

        protected override void Destroy()
        {
            base.Destroy();

            Utilities.FreeMemory((IntPtr)pAudioDataRendererInfo);

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

        public void Play()
        {
            pAudioDataRendererInfo->PlaybackEnded = false;

            EnableMixerCurrentInput(true);
        }

        public void Pause()
        {
            EnableMixerCurrentInput(false);
        }

        public void Stop()
        {
            EnableMixerCurrentInput(false);
            pAudioDataRendererInfo->CurrentFrame = 0;

            // free the input bus for other sound effects.
            if (BusIndexMixer != uint.MaxValue)
            {
                // reset the mixer callbacks to null renderers
                CheckUnitStatus((AudioUnitStatus)SetInputRenderCallbackToNull(unit3DMixer.Handle, BusIndexMixer), "Failed to set the render callback");
                CheckUnitStatus((AudioUnitStatus)SetInputRenderCallbackToNull(unitChannelMixer.Handle, BusIndexMixer), "Failed to set the render callback");

                availableMixerBusIndices.Enqueue(BusIndexMixer);
            }
        }
        
        public void SetAudioData(SoundEffect soundEffect)
        {
            BusIndexMixer = uint.MaxValue; // invalid index value (when no bus are free)

            // find a available bus indices for this instance.
            if (availableMixerBusIndices.Count > 0)
                BusIndexMixer = availableMixerBusIndices.Dequeue();
            else
            {
                // force the update of all sound effect instance to free bus indices
                audioEngine.ForceSoundEffectInstanceUpdate();

                // retry to get an free bus index
                if (availableMixerBusIndices.Count > 0)
                {
                    BusIndexMixer = availableMixerBusIndices.Dequeue();
                }
                else // try to stop another instance
                {
                    // try to get a sound effect to stop
                    var soundEffectToStop = audioEngine.GetLeastSignificativeSoundEffect();
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
            unitChannelMixer.SetAudioFormat(CreateLinear16BitsPcm(waveFormat.Channels, waveFormat.SampleRate), AudioUnitScopeType.Input, BusIndexMixer);

            // set the channel input bus callback
            CheckUnitStatus((AudioUnitStatus)SetInputRenderCallbackToChannelMixerDefault(unitChannelMixer.Handle, BusIndexMixer, (IntPtr)pAudioDataRendererInfo), "Failed to set the render callback");
                    
            ResetChannelMixerParameter();

            // initialize the 3D mixer input bus, if the sound can be used as 3D sound.
            if (waveFormat.Channels == 1)
            {
                // Set the audio stream format of the current mixer input bus.
                unit3DMixer.SetAudioFormat(CreateLinear16BitsPcm(waveFormat.Channels, waveFormat.SampleRate), AudioUnitScopeType.Input, BusIndexMixer);

                // set the 3D mixer input bus callback
                CheckUnitStatus((AudioUnitStatus)SetInputRenderCallbackTo3DMixerDefault(unit3DMixer.Handle, BusIndexMixer, (IntPtr)pAudioDataRendererInfo), "Failed to set the render callback");

                Reset3DMixerParameter();
            }

            // Disable the input by default so that it started in Stopped mode.
            EnableMixerCurrentInput(false);

            // set render info data 
            pAudioDataRendererInfo->AudioDataBuffer = soundEffect.WaveDataPtr;
            pAudioDataRendererInfo->TotalNumberOfFrames = (soundEffect.WaveDataSize / waveFormat.BlockAlign);
            pAudioDataRendererInfo->NumberOfChannels = waveFormat.Channels;

            // reset playback to the beginning of the track and set the looping status
            pAudioDataRendererInfo->CurrentFrame = 0;
            SetLoopingPoints(0, int.MaxValue, 0, pAudioDataRendererInfo->IsInfiniteLoop);
            SetVolume(soundEffectInstance.Volume);
            SetPan(soundEffectInstance.Pan);
        }

        public void SetVolume(float volume)
        {
            if (BusIndexMixer == uint.MaxValue)
                return;

            if (Is3D)
            {
                var gain = Math.Max(-120f, (float) (20*Math.Log10(volume)));
                CheckUnitStatus(unit3DMixer.SetParameter(AudioUnitParameterType.Mixer3DGain, gain, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the Gain parameter of the 3D mixer");
            }
            else
            {
                CheckUnitStatus(unitChannelMixer.SetParameter(AudioUnitParameterType.MultiChannelMixerVolume, volume, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the mixer bus Volume parameter.");
            }
        }

        public void SetPan(float pan)
        {
            if (BusIndexMixer == uint.MaxValue)
                return;

            Is3D = false;
            CheckUnitStatus(unitChannelMixer.SetParameter(AudioUnitParameterType.MultiChannelMixerPan, pan, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the mixer bus Pan parameter.");
        }

        private void Set3DParameters(float azimuth, float elevation, float distance, float playRate)
        {
            if (BusIndexMixer == uint.MaxValue)
                return;

            CheckUnitStatus(unit3DMixer.SetParameter(AudioUnitParameterType.Mixer3DAzimuth, azimuth, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the Azimuth parameter of the 3D mixer");
            CheckUnitStatus(unit3DMixer.SetParameter(AudioUnitParameterType.Mixer3DElevation, elevation, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the Elevation parameter of the 3D mixer");
            CheckUnitStatus(unit3DMixer.SetParameter(AudioUnitParameterType.Mixer3DDistance, distance, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the Distance parameter of the 3D mixer");
            CheckUnitStatus(unit3DMixer.SetParameter(AudioUnitParameterType.Mixer3DPlaybackRate, playRate, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the PlayRate parameter of the 3D mixer");
        }

        public void Apply3D(float azimut, float elevation, float distance, float playRate)
        {
            Set3DParameters(azimut, elevation, distance, playRate);
            Is3D = true;
        }

        public void Reset3D()
        {
            if (Is3D)
            {
                Set3DParameters(0, 0, 0, 1);
                Is3D = false;
            }
        }

        private void Reset3DMixerParameter()
        {
            if (BusIndexMixer == uint.MaxValue)
                return;

            Set3DParameters(0, 0, 0, 1);
            CheckUnitStatus(unit3DMixer.SetParameter(AudioUnitParameterType.Mixer3DGain, 0, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the Gain parameter of the 3D mixer");
        }

        private void ResetChannelMixerParameter()
        {
            if (BusIndexMixer == uint.MaxValue)
                return;

            CheckUnitStatus(unitChannelMixer.SetParameter(AudioUnitParameterType.MultiChannelMixerVolume, 1, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the mixer bus Volume parameter.");
            CheckUnitStatus(unitChannelMixer.SetParameter(AudioUnitParameterType.MultiChannelMixerPan, 0, AudioUnitScopeType.Input, BusIndexMixer), "Failed to set the mixer bus Pan parameter.");
        }
        
        [DebuggerDisplay("AudioDataMixer for input bus {parent.BusIndexChannelMixer}-{parent.BusIndex3DMixer}")]
        [StructLayout(LayoutKind.Sequential)]
        struct AudioDataRendererInfo
        {
            public int LoopStartPoint;
            public int LoopEndPoint;
            public int NumberOfLoops;
            public bool IsInfiniteLoop;

            public int CurrentFrame;
            public int TotalNumberOfFrames;

            public int NumberOfChannels;
            public IntPtr AudioDataBuffer;

            public bool IsEnabled2D;
            public bool IsEnabled3D;

            public bool PlaybackEnded;

            public IntPtr HandleChannelMixer;
            public IntPtr Handle3DMixer;
        }
    }
}

#endif