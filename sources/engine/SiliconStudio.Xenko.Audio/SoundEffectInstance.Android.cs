// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using Android.Media;
using Android.Runtime;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Audio.Wave;

using Math = System.Math;

namespace SiliconStudio.Xenko.Audio
{
    partial class SoundEffectInstance
    {
        private const int MaximumNumberOfTracks = 8;
        private const int NumberOfSubBuffersInAudioTrack = 2;
        protected internal const int SoundEffectInstanceFrameRate = 44100;

        private static readonly Queue<TrackInfo> audioTrackPool = new Queue<TrackInfo>();
        
        private static readonly IntPtr audioTrackClassJavaHandle = JNIEnv.FindClass("android/media/AudioTrack");
        private static readonly IntPtr audioTrackWriteMethodID = JNIEnv.GetMethodID(audioTrackClassJavaHandle, "write", "([BII)I");
        
        private delegate void SetJavaByteArrayRegionDelegate(IntPtr handle, IntPtr destination, int offset, int length, IntPtr sourceByteArray);
        
        private static SetJavaByteArrayRegionDelegate setJavaByteArray;

        private static IntPtr blankJavaDataBuffer;
        
        private int readBufferPosition;
        private int writeBufferPosition;

        private TrackInfo currentTrack;
        private readonly object currentTrackLock = new object();

        private bool exitLoopRequested;

        private class TrackInfo : IDisposable
        {
            private readonly byte[] dataBuffer;

            private readonly int bufferSize;
            
            private readonly IntPtr javaDataBuffer;

            private readonly JValue[] javaWriteCallValues;

            public readonly AudioTrack Track;

            public int BuffersToWrite;

            public bool ShouldStop;

            public SoundEffectInstance CurrentInstance;

            public TrackInfo(AudioTrack track, IntPtr javaDataBuffer, byte[] dataBuffer, int bufferSize)
            {
                Track = track;
                this.javaDataBuffer = javaDataBuffer;
                this.dataBuffer = dataBuffer;
                this.bufferSize = bufferSize;

                javaWriteCallValues = new JValue[3];
                
                // add the callback feeding the audio track and updating play status
                Track.PeriodicNotification += (sender, args) => OnPeriodFinished();
                var status = Track.SetPositionNotificationPeriod(bufferSize / 4); // in frame number ( 2 channels * 2 byte data = 4)
                if (status != TrackStatus.Success)
                    throw new AudioSystemInternalException("AudioTrack.SetNotificationMarkerPosition failed and failure was not handled. [error=" + status + "].");
            }

            /// <summary>
            /// Should be called with the <see cref="TrackInfo"/> locked.
            /// </summary>
            public void WriteNextAudioBufferToTrack()
            {
                --BuffersToWrite;

                var instance = CurrentInstance;
                var soundEffect = instance.soundEffect;

                var sizeWriten = 0;
                while (sizeWriten < bufferSize)
                {
                    var sizeToWrite = bufferSize - sizeWriten;
                    var shouldWriteBlank = instance.writeBufferPosition >= soundEffect.WaveDataSize;

                    if (!shouldWriteBlank)
                    {
                        sizeToWrite = Math.Min(sizeToWrite, soundEffect.WaveDataSize - instance.writeBufferPosition);

                        if (setJavaByteArray == null)
                        {
                            Array.Copy(soundEffect.WaveDataArray, instance.writeBufferPosition, dataBuffer, 0, sizeToWrite);
                            JNIEnv.CopyArray(dataBuffer, javaDataBuffer);
                        }
                        else
                        {
                            setJavaByteArray(JNIEnv.Handle, javaDataBuffer, 0, sizeToWrite, soundEffect.WaveDataPtr + instance.writeBufferPosition);
                        }
                    }

                    javaWriteCallValues[0] = new JValue(shouldWriteBlank ? blankJavaDataBuffer : javaDataBuffer);
                    javaWriteCallValues[1] = new JValue(0);
                    javaWriteCallValues[2] = new JValue(sizeToWrite);

                    var writtenSize = JNIEnv.CallIntMethod(Track.Handle, audioTrackWriteMethodID, javaWriteCallValues);

                    sizeWriten += writtenSize;
                    instance.writeBufferPosition += writtenSize;

                    if (instance.writeBufferPosition >= soundEffect.WaveDataSize && instance.IsLooped && !instance.exitLoopRequested)
                        instance.writeBufferPosition = 0;

                    if (writtenSize != sizeToWrite) // impossible the write all the data due to a call to pause or stop
                        break; // all next call to WriteDataToAudioTrack will write 0 byte until next play.
                }
            }

            private void OnPeriodFinished()
            {
                int audioDataBufferSize;
                SoundEffectInstance instance;

                lock (this)
                {
                    ++BuffersToWrite;
                    if (BuffersToWrite == NumberOfSubBuffersInAudioTrack)
                        Track.Stop();

                    instance = CurrentInstance;
                    if(instance == null)
                        return;

                    instance.readBufferPosition += bufferSize;
                    audioDataBufferSize = instance.soundEffect.WaveDataSize;

                    while (instance.readBufferPosition >= audioDataBufferSize && instance.IsLooped && !instance.exitLoopRequested)
                        instance.readBufferPosition -= audioDataBufferSize;

                    if (instance.readBufferPosition < audioDataBufferSize && !ShouldStop)
                        WriteNextAudioBufferToTrack();
                }
                if ((instance.readBufferPosition >= audioDataBufferSize || ShouldStop) && instance.PlayState != SoundPlayState.Paused)
                    instance.Stop();
            }

            public void Dispose()
            {
                Track.Release();
                Track.Dispose();
                JNIEnv.DeleteGlobalRef(javaDataBuffer);
            }
        }

        internal static void CreateAudioTracks()
        {
            const int audioMemoryOS = 1024 * 1024; // the audio client have only this amount of memory available for streaming (see: AudioFlinger::Client constructor -> https://android.googlesource.com/platform/frameworks/av/+/126a630/services/audioflinger/AudioFlinger.cpp : line 1153)
            const int memoryDealerHeaderSize = 64; // size taken by the header of each memory section of the MemoryDealer.

            GetSetArrayRegionFunctionPointer();

            // the minimum size that can have an audio track in streaming mode (with that audio format)
            var minimumBufferSize = AudioTrack.GetMinBufferSize(SoundEffectInstanceFrameRate, ChannelOut.Stereo, Encoding.Pcm16bit);

            // the size that should be kept in order to be able to play sound music correctly (note: we need to be able to play 2 music simultaneously because destruction is asynchronous)
            var memoryNeededForSoundMusic = 2 * (GetUpperPowerOfTwo(minimumBufferSize) + memoryDealerHeaderSize);

            // the size taken by one of our sub-buffers => 2 bytes (16 bits sample) * 2 channels * 30 ms at 44100Hz
            var subBufferSize = Math.Max((int)Math.Ceiling(minimumBufferSize / (float)NumberOfSubBuffersInAudioTrack), 2 * 2 * 8000); 

            // the memory taken by one audio track creation for sound effects
            var memoryNeededAudioTrack = GetUpperPowerOfTwo(subBufferSize*NumberOfSubBuffersInAudioTrack);

            // the java buffer used to copy blank sound data
            blankJavaDataBuffer = JNIEnv.NewGlobalRef(JNIEnv.NewArray(new byte[subBufferSize]));

            // create the pool of audio tracks
            var trackNumber = 0;
            while (trackNumber < MaximumNumberOfTracks && audioMemoryOS - (trackNumber+1) * memoryNeededAudioTrack >= memoryNeededForSoundMusic)
            {
                // create the audio track
                var audioTrack = new AudioTrack(Stream.Music, SoundEffectInstanceFrameRate, ChannelOut.Stereo, Encoding.Pcm16bit, 
                                                NumberOfSubBuffersInAudioTrack * subBufferSize, AudioTrackMode.Stream);

                if (audioTrack.State == AudioTrackState.Uninitialized) // the maximum number of tracks is reached
                    break;

                // create the c# buffer for internal copy
                var dataBuffer = new byte[subBufferSize];

                // create the java buffer
                var javaDataBuffer = JNIEnv.NewGlobalRef(JNIEnv.NewArray(dataBuffer));

                // add the new track to the audio track pool
                var newTrackInfo = new TrackInfo(audioTrack, javaDataBuffer, dataBuffer, subBufferSize) { BuffersToWrite = NumberOfSubBuffersInAudioTrack };
                audioTrackPool.Enqueue(newTrackInfo);

                ++trackNumber;
            }
        }

        private static int GetUpperPowerOfTwo(int size)
        {
            var upperPowerOfTwo = 2;
            while (upperPowerOfTwo < size)
                upperPowerOfTwo = upperPowerOfTwo << 1;

            return upperPowerOfTwo;
        }

        /// <summary>
        /// Hack using reflection to get a pointer to java jni SetByteArrayRegion function pointer
        /// </summary>
        private static void GetSetArrayRegionFunctionPointer()
        {
            // ReSharper disable PossibleNullReferenceException
            try
            {
                var jniEnvGetter = typeof(JNIEnv).GetMethod("get_Env", BindingFlags.Static | BindingFlags.NonPublic);
                if (jniEnvGetter == null)
                    return;

                var jniEnvInstanceField = jniEnvGetter.ReturnType.GetField("JniEnv", BindingFlags.NonPublic | BindingFlags.Instance);
                var setByteArrayFunctionField = jniEnvInstanceField.FieldType.GetField("SetByteArrayRegion", BindingFlags.Public | BindingFlags.Instance);

                var jniEnvInstance = jniEnvInstanceField.GetValue(jniEnvGetter.Invoke(null, null));
                var pointerToSetByteArrayFunction = (IntPtr)setByteArrayFunctionField.GetValue(jniEnvInstance);

                setJavaByteArray = Marshal.GetDelegateForFunctionPointer<SetJavaByteArrayRegionDelegate>(pointerToSetByteArrayFunction);
            }
            catch (Exception)
            {
            }
            // ReSharper restore PossibleNullReferenceException
        }

        internal static void StaticDestroy()
        {
            JNIEnv.DeleteGlobalRef(blankJavaDataBuffer);
            blankJavaDataBuffer = IntPtr.Zero;

            // release created audio tracks and java buffers.
            foreach (var trackInfo in audioTrackPool)
                trackInfo.Dispose();

            audioTrackPool.Clear();
        }

        internal void UpdateStereoVolumes()
        {
            lock (currentTrackLock)
            {
                if (currentTrack == null) // did not manage to obtain a track
                    return;

                // both Volume, panChannelVolumes, localizationChannelVolumes are in [0,1] so multiplication too, no clamp is needed.
                var status = currentTrack.Track.SetStereoVolume(Volume * panChannelVolumes[0] * localizationChannelVolumes[0], Volume * panChannelVolumes[1] * localizationChannelVolumes[1]);
                if (status != TrackStatus.Success)
                    throw new AudioSystemInternalException("AudioTrack.SetStereoVolume failed and failure was not handled. [error:" + status + "]");
            }
        }

        internal override void UpdateLooping()
        {
        }

        internal override void PlayImpl()
        {
            lock (currentTrackLock)
            {
                if (currentTrack != null && PlayState != SoundPlayState.Paused)
                    throw new AudioSystemInternalException("AudioTrack.PlayImpl was called with play state '" + PlayState + "' and currentTrackInfo not null.");

                if (currentTrack == null) // the audio instance is stopped.
                {
                    currentTrack = TryGetAudioTack();
                    if (currentTrack == null) // could not obtain a track -> give up and early return
                    {
                        AudioEngine.Logger.Info("Failed to obtain an audio track for SoundEffectInstance '{0}'. Play call will be ignored.",Name);
                        return;
                    }

                    // Update track state
                    UpdateLooping();
                    UpdatePitch();
                    UpdateStereoVolumes();
                }

                lock (currentTrack)
                {
                    currentTrack.CurrentInstance = this;
                    currentTrack.ShouldStop = false;

                    currentTrack.Track.Play();

                    while (currentTrack.BuffersToWrite > 0)
                        currentTrack.WriteNextAudioBufferToTrack();
                }
            }
        }

        internal override void PauseImpl()
        {
            lock (currentTrackLock)
            {
                if (currentTrack == null) // did not manage to obtain a track
                    return;

                currentTrack.ShouldStop = true;
            }
        }

        internal override void StopImpl()
        {
            exitLoopRequested = false;

            lock (currentTrackLock)
            {
                if (currentTrack == null) // did not manage to obtain a track
                    return;

                // update tack info
                lock (currentTrack)
                {
                    currentTrack.ShouldStop = true;
                    currentTrack.CurrentInstance = null;
                }

                // reset playback position
                readBufferPosition = 0;
                writeBufferPosition = 0;

                // add the track back to the tracks pool
                lock (audioTrackPool)
                    audioTrackPool.Enqueue(currentTrack);

                // avoid concurrency problems with EndOfTrack callback
                lock (currentTrackLock)
                    currentTrack = null;
            }
        }

        internal override void ExitLoopImpl()
        {
            exitLoopRequested = true;
        }

        internal virtual void CreateVoice(WaveFormat format)
        {
            // nothing to do here
        }

        private TrackInfo TryGetAudioTack()
        {
            // try to get a track from the pool
            lock (audioTrackPool)
            {
                if (audioTrackPool.Count > 0)
                    return audioTrackPool.Dequeue();
            }

            // pool was empty -> try to stop irrelevant instances to free a track
            var soundEffectToStop = AudioEngine.GetLeastSignificativeSoundEffect();
            if (soundEffectToStop == null) 
                return null;

            // stop the sound effect instances and retry to get a track
            soundEffectToStop.StopAllInstances();

            lock (audioTrackPool)
            {
                if (audioTrackPool.Count > 0)
                    return audioTrackPool.Dequeue();
            }

            return null;
        }

        internal override void LoadBuffer()
        {
        }
        
        internal virtual void DestroyVoice()
        {
            lock (currentTrackLock)
            {
                if (currentTrack != null) // the voice has not been destroyed in the previous stop or the instance has not been stopped
                    throw new AudioSystemInternalException("The AudioTrackInfo was not null when destroying the SoundEffectInstance.");
            }
        }

        internal void UpdatePitch()
        {
            lock (currentTrackLock)
            {
                if (currentTrack == null) // did not manage to obtain a track
                    return;

                var status = currentTrack.Track.SetPlaybackRate((int)(MathUtil.Clamp((float)Math.Pow(2, Pitch) * dopplerPitchFactor, 0.5f, 2f) * SoundEffectInstanceFrameRate)); // conversion octave to frequency
                if (status != (int)TrackStatus.Success)
                    throw new AudioSystemInternalException("AudioTrack.SetPlaybackRate failed and failure was not handled. [error:" + status + "]");
            }
        }
        
        internal virtual void PlatformSpecificDisposeImpl()
        {
            DestroyVoice();
        }

        private void Apply3DImpl(AudioListener listener, AudioEmitter emitter)
        {
            // Since android has no function available to perform sound 3D localization by default, here we try to mimic the behaviour of XAudio2

            // After an analysis of the XAudio2 left/right stereo balance with respect to 3D world position, 
            // it could be found the volume repartition is symmetric to the Up/Down and Front/Back planes.
            // Moreover the left/right repartition seems to follow a third degree polynomial function:
            // Volume_left(a) = 2(c-1)*a^3 - 3(c-1)*a^2 + c*a , where c is a constant close to c = 1.45f and a is the angle normalized bwt [0,1]
            // Volume_right(a) = 1-Volume_left(a)

            // As for signal attenuation wrt distance the model follows a simple inverse square law function as explained in XAudio2 documentation 
            // ( http://msdn.microsoft.com/en-us/library/windows/desktop/microsoft.directx_sdk.x3daudio.x3daudio_emitter(v=vs.85).aspx )
            // Volume(d) = 1                    , if d <= ScaleDistance where d is the distance to the listener
            // Volume(d) = ScaleDistance / d    , if d >= ScaleDistance where d is the distance to the listener

            // 1. Attenuation due to distance.
            var vecListEmit = emitter.Position - listener.Position;
            var distListEmit = vecListEmit.Length();
            var attenuationFactor = distListEmit <= emitter.DistanceScale ? 1f : emitter.DistanceScale / distListEmit;

            // 2. Left/Right balance.
            var repartRight = 0.5f;
            var worldToList = Matrix.Identity;
            var rightVec = Vector3.Cross(listener.Forward, listener.Up);
            worldToList.Column1 = new Vector4(rightVec, 0);
            worldToList.Column2 = new Vector4(listener.Forward, 0);
            worldToList.Column3 = new Vector4(listener.Up, 0);
            var vecListEmitListBase = Vector3.TransformNormal(vecListEmit, worldToList);
            var vecListEmitListBase2 = (Vector2)vecListEmitListBase;
            if (vecListEmitListBase2.Length() > 0)
            {
                const float c = 1.45f;
                var absAlpha = Math.Abs(Math.Atan2(vecListEmitListBase2.Y, vecListEmitListBase2.X));
                var normAlpha = (float)(absAlpha / (Math.PI / 2));
                if (absAlpha > Math.PI / 2) normAlpha = 2 - normAlpha;
                repartRight = 0.5f * (2 * (c - 1) * normAlpha * normAlpha * normAlpha - 3 * (c - 1) * normAlpha * normAlpha * normAlpha + c * normAlpha);
                if (absAlpha > Math.PI / 2) repartRight = 1 - repartRight;
            }

            // Set the volumes.
            localizationChannelVolumes = new[] { attenuationFactor * (1f - repartRight), attenuationFactor * repartRight };
            UpdateStereoVolumes();

            // 3. Calculation of the Doppler effect
            ComputeDopplerFactor(listener, emitter);
            UpdatePitch();
        }

        private void Reset3DImpl()
        {
            // nothing to do here.
        }

        internal override void UpdateVolume()
        {
            UpdateStereoVolumes();
        }

        private void UpdatePan()
        {
            UpdateStereoVolumes();
        }
    }
}

#endif