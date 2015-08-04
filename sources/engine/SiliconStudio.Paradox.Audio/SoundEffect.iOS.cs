// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_IOS

using System;
using UIKit;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Audio.Wave;

namespace SiliconStudio.Paradox.Audio
{
    public partial class SoundEffect
    {
        // need to convert the audio data to 44100Hz because having several input of different sampling rate in not working on iOS < 7.0
        private void AdaptAudioDataImpl()
        {
            if(UIDevice.CurrentDevice.CheckSystemVersion(7, 0)) // input of different sampling rate work properly on iOS >= 7.0
                return;

            if(WaveFormat.SampleRate >= AudioVoice.AudioUnitOutputSampleRate) // down sampling is not supported
                return;

            // allocate the new audio buffer 
            var sampleRateRatio = WaveFormat.SampleRate / (float)AudioVoice.AudioUnitOutputSampleRate;
            var newWaveDataSize = (int)Math.Floor(WaveDataSize / sampleRateRatio);
            var newWaveDataPtr = Utilities.AllocateMemory(newWaveDataSize);

            // up-sample the audio data
            if (Math.Abs(sampleRateRatio - 0.5f) < MathUtil.ZeroTolerance)
                UpSampleByTwo(WaveDataPtr, newWaveDataPtr, newWaveDataSize, WaveFormat.Channels, false);
            else
                UpSample(WaveDataPtr, newWaveDataPtr, newWaveDataSize, sampleRateRatio, WaveFormat.Channels, false);

            // update the wave data buffer
            Utilities.FreeMemory(nativeDataBuffer);
            nativeDataBuffer = newWaveDataPtr;
            WaveDataPtr = newWaveDataPtr;
            WaveDataSize = newWaveDataSize;
            WaveFormat = new WaveFormat(AudioVoice.AudioUnitOutputSampleRate, WaveFormat.Channels);
        }

        private void DestroyImpl()
        {
        }
    }
}

#endif