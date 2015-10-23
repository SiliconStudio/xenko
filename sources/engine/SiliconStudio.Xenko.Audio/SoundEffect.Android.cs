// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using System.Runtime.InteropServices;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Audio.Wave;

namespace SiliconStudio.Xenko.Audio
{
    public partial class SoundEffect
    {
        internal byte[] WaveDataArray;

        private GCHandle pinnedWaveData;

        private unsafe void AdaptAudioDataImpl()
        {
            var isStereo = WaveFormat.Channels == 2;
            
            // allocate the new audio buffer 
            var sampleRateRatio = Math.Min(1, WaveFormat.SampleRate / (float)SoundEffectInstance.SoundEffectInstanceFrameRate); // we don't down-sample in current version
            var newWaveDataSize = (int)Math.Floor(WaveDataSize / sampleRateRatio) * (isStereo ? 1 : 2);
            WaveDataArray = new byte[newWaveDataSize];

            fixed (byte* pNewWaveData = WaveDataArray)
            {
                // re-sample the audio data
                if (Math.Abs(sampleRateRatio - 1f) < MathUtil.ZeroTolerance && !isStereo)
                    DuplicateTracks(WaveDataPtr, (IntPtr)pNewWaveData, newWaveDataSize);
                else if (Math.Abs(sampleRateRatio - 0.5f) < MathUtil.ZeroTolerance)
                    UpSampleByTwo(WaveDataPtr, (IntPtr)pNewWaveData, newWaveDataSize, WaveFormat.Channels, !isStereo);
                else
                    UpSample(WaveDataPtr, (IntPtr)pNewWaveData, newWaveDataSize, sampleRateRatio, WaveFormat.Channels, !isStereo);
            }

            // update the wave data buffer
            pinnedWaveData = GCHandle.Alloc(WaveDataArray, GCHandleType.Pinned);
            WaveDataPtr = pinnedWaveData.AddrOfPinnedObject();
            WaveDataSize = newWaveDataSize;
            
            // Free unused anymore C# data buffer
            Utilities.FreeMemory(nativeDataBuffer);
            nativeDataBuffer = IntPtr.Zero;
        }

        private void DestroyImpl()
        {
            pinnedWaveData.Free();
        }
    }
}

#endif