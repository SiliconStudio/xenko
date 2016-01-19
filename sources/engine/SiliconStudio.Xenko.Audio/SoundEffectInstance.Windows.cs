// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS && !SILICONSTUDIO_XENKO_SOUND_SDL

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Audio.Wave;

using SharpDX.XAudio2;
using SharpDX.X3DAudio;

namespace SiliconStudio.Xenko.Audio
{
    public partial class SoundEffectInstance
    {
        #region Implementation of the ILocalizable Interface

        internal void Apply3DImpl(AudioListener listener, AudioEmitter emitter)
        {
            //////////////////////////////////////////////////////////////
            // 1. First let's calculate the parameters to set to the voice
            var inputChannels = soundEffect.WaveFormat.Channels;
            var outputChannels = MasterVoice.VoiceDetails.InputChannelCount;

            if (inputChannels != 1 || outputChannels != 2)
                throw new AudioSystemInternalException("Error in Apply3DImpl only mono sounds are supposed to be localizable");

            var list = new Listener
                {
                    Position = listener.Position.ToSharpDX(), 
                    Velocity = listener.Velocity.ToSharpDX(),
                    OrientFront = listener.Forward.ToSharpDX(),
                    OrientTop = listener.Up.ToSharpDX()
                };
            var emit = new Emitter
                {
                    Position = emitter.Position.ToSharpDX(),
                    Velocity = emitter.Velocity.ToSharpDX(),
                    DopplerScaler = emitter.DopplerScale,
                    CurveDistanceScaler = emitter.DistanceScale,
                    ChannelRadius = 0f, // Multi-channels localizable sound are considered as source of multiple sounds coming from the same location.
                    ChannelCount = inputChannels
                };
            
            var dspSettings = new DspSettings(inputChannels, outputChannels);

            AudioEngine.X3DAudio.Calculate(list, emit, CalculateFlags.Matrix | CalculateFlags.LpfDirect, dspSettings);

            /////////////////////////////////////////////////////////////
            // 2. Now let's set the voice parameters to simulate a 3D voice.

            // 2.1 The Doppler effect due to the difference of speed between the emitter and listener
            ComputeDopplerFactor(listener, emitter);
            UpdatePitch();

            // 2.2 The channel attenuations due to the source localization.
            localizationChannelVolumes = new[] { dspSettings.MatrixCoefficients[0], dspSettings.MatrixCoefficients[1] };    // only mono sound can be localized so matrix should be 2*1
            UpdateStereoVolumes();
        }

        internal override void UpdateLooping()
        {
            // Nothing to do here for windows version.
            // All the work is done in LoadBuffer.
        }

        internal override void PauseImpl()
        {
            SourceVoice.Stop();
        }

        internal override void ExitLoopImpl()
        {
            SourceVoice.ExitLoop();
        }

        internal override void PlayImpl()
        {
            SourceVoice.Start();
        }

        internal override void StopImpl()
        {
            SourceVoice.Stop();
            SourceVoice.FlushSourceBuffers();
        }

        private void UpdateStereoVolumes()
        {
            var sourceChannelCount = WaveFormat.Channels;

            // then update the volume of each channel
            Single[] matrix;
            if (sourceChannelCount == 1)
            {   // panChannelVolumes and localizationChannelVolumes are both in [0,1] so multiplication too, no clamp is needed
                matrix = new[] { panChannelVolumes[0] * localizationChannelVolumes[0], panChannelVolumes[1] * localizationChannelVolumes[1] };
            }
            else if (sourceChannelCount == 2)
            {
                matrix = new[] { panChannelVolumes[0], 0, 0, panChannelVolumes[1] }; // no localization on stereo sounds.
            }
            else
            {
                throw new AudioSystemInternalException("The sound is not supposed to contain more than 2 channels");
            }

            SourceVoice.SetOutputMatrix(sourceChannelCount, MasterVoice.VoiceDetails.InputChannelCount, matrix);
        }

        private void UpdatePitch()
        {
            SourceVoice.SetFrequencyRatio(MathUtil.Clamp((float)Math.Pow(2, Pitch) * dopplerPitchFactor, 0.5f, 2f)); // conversion octave to frequencyRatio
        }

        #endregion

        #region Implementation of the IDisposable Interface

        internal void PlatformSpecificDisposeImpl()
        {
            if(SourceVoice == null)
                return;

            SourceVoice.DestroyVoice();
            SourceVoice.Dispose();
        }

        #endregion

        internal SourceVoice SourceVoice;

        internal void CreateVoice(WaveFormat format)
        {
            SourceVoice = new SourceVoice(AudioEngine.XAudio2, format.ToSharpDX(), VoiceFlags.None, 2f, true); // '2f' -> allow to modify pitch up to one octave, 'true' -> enable callback
            SourceVoice.StreamEnd += Stop;
        }

        internal override void LoadBuffer()
        {
            var buffer = new AudioBuffer(new SharpDX.DataPointer(soundEffect.WaveDataPtr, soundEffect.WaveDataSize));
            
            if (IsLooped)
                buffer.LoopCount = AudioBuffer.LoopInfinite;

            SourceVoice.SubmitSourceBuffer(buffer, null);
        }

        private void Reset3DImpl()
        {
            // nothing to do here.
        }

        internal override void UpdateVolume()
        {
            SourceVoice.SetVolume(Volume);
        }

        private void UpdatePan()
        {
            UpdateStereoVolumes();
        }
    }
}

#endif