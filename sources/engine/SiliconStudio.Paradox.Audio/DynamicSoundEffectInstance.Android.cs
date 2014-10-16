// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using System.Collections.Generic;
using System.Threading;
using Android.Media;

using SiliconStudio.Paradox.Audio.Wave;

using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Paradox.Audio
{
    partial class DynamicSoundEffectInstance
    {
        /// <summary>
        /// The number of sub-division of the audioTrack buffer.
        /// </summary>
        private const int NbOfSubBuffers = 3;
        
        /// <summary>
        /// The handles to the data buffers in use in the current subBuffer.
        /// </summary>
        private SubBufferDataHandles currentSubBufferHandles;

        /// <summary>
        /// The empty space remainning in the current subBuffer
        /// </summary>
        private int dataNeededToFillCurSubBuffer ;

        /// <summary>
        /// The size in byte of a subBuffer of the audio track (size of a sub-part of the audioTrack buffer).
        /// </summary>
        internal int SubBufferSize;

        /// <summary>
        /// The total number of audio frames submitted for playback.
        /// </summary>
        private int nbOfAudioFrameAvailable;

        /// <summary>
        /// Lock to protect all internal data from concurrency introduced by audio callbacks.
        /// </summary>
        private readonly object internalLock = new object();

        /// <summary>
        /// A buffer provided by the user that is partially written on the AudioTrack buffer.
        /// </summary>
        private class UserBuffer
        {
            public int Offset;
            public int ByteCount;
            public readonly byte[] Buffer;

            public UserBuffer(int offset, int byteCount, byte[] buffer)
            {
                Offset = offset;
                ByteCount = byteCount;
                Buffer = buffer;
            }
        }

        /// <summary>
        /// The list of the user provided buffers that are waiting to be submitted to the audioTrack.
        /// </summary>
        private readonly Queue<UserBuffer> pendingUserBuffers = new Queue<UserBuffer>();

        private void InitializeDynamicSound()
        {
            // nothing to do here.
        }

        /// <summary>
        /// Method called when a subbuffer of the audio track has finished to be played.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSubBufferConsumed(object sender, EventArgs e)
        {
            //OnBufferEndCommon();
            //
            //// write next data on the audio track if it exist
            //lock (internalLock)
            //{
            //    if (AudioTrack == null)
            //        return;
            //
            //    WriteUserBuffersToAudioTrack();
            //}
        }

        /// <summary>
        /// Method called when all the data for playback has been consumed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAllDataConsumed(object sender, EventArgs e)
        {
            //lock (internalLock)
            //{
            //    pendingBufferCount -= currentSubBufferHandles.HandleCount;
            //    currentSubBufferHandles.FreeHandles();
            //
            //    // bug of the library: if playback arrives at the end of audio data,
            //    // future submitted buffers are never played unless all the buffer gets full.
            //    // the solve this problem we destroy and recreate the voice
            //    RebuildVoice();
            //    AudioTrack.Play();
            //}
        }

        internal void RebuildVoice()
        {
            DestroyVoice();
            CreateVoice(WaveFormat);

            UpdateLooping();
            UpdatePitch();
            UpdateStereoVolumes();
        }

        private void SubmitBufferImpl(byte[] buffer, int offset, int byteCount)
        {
            //lock (internalLock)
            //{
            //    if(AudioTrack == null)
            //        return;
            //
            //    // set the marker to the end of the audioTrack to trigger OnAllDataConsumed
            //    nbOfAudioFrameAvailable += byteCount / WaveFormat.BlockAlign;
            //    var status = AudioTrack.SetNotificationMarkerPosition(nbOfAudioFrameAvailable);
            //    if (status != TrackStatus.Success)
            //        throw new AudioSystemInternalException("AudioTrack.SetNotificationMarkerPosition failed and failure was not handled. [error=" + status + "].");
            //
            //    Interlocked.Increment(ref pendingBufferCount);
            //
            //    // add the user buffer to the pending list waiting to be processed
            //    var userBuffer = new UserBuffer(offset, byteCount, buffer);
            //    pendingUserBuffers.Enqueue(userBuffer);
            //
            //    // write next data on the audio track if it possible
            //    WriteUserBuffersToAudioTrack();
            //}
        }

        private void AudioTrackWrite(byte[] buffer, int offset, int byteCount)
        {
            //if (AudioTrack == null) // the voice has been destroyed or creation failed
            //    return;
            //
            //// monodroid make a copy of the ENTIRE buffer when passing it to the java API
            //// to avoid useless copy of big buffer written in slices (and consequent garbage collection)
            //// we create a subbuffer of just the required size (then copy happens only on the needed data)
            //
            //int writtenLength;
            //if (buffer.Length > NbOfSubBuffers*SubBufferSize)
            //{
            //    writtenLength = AudioTrack.Write(buffer.SubArray(offset, byteCount), 0, byteCount);
            //}
            //else
            //{
            //    writtenLength = AudioTrack.Write(buffer, offset, byteCount);
            //}
            //
            ////if (writtenLength != byteCount)  -> sometimes failure happens without reason but does not disturb next write calls, so we skip the check
            ////    throw new AudioSystemInternalException("AudioTrack.Write failed to write the provided data and failure was not handled. [error=" + writtenLength + "]");
        }

        private void WriteUserBuffersToAudioTrack()
        {
            // Here we need to fill the subBuffers of size 'subBufferSize' with the buffers provided by the user.
            // There are three cases:
            //  (1) the provided buffer is smaller than our subBuffer size. In this case we need to concat the buffers.
            //  (2) the provided buffer is bigger than our subBuffer size. In this case we need to cut the buffer.
            //  (3) the buffer has exaclty the size of our subBuffer size. In this case we submit the buffer as is.

            while (internalPendingBufferCount < NbOfSubBuffers)
            {
                if (pendingUserBuffers.Count == 0) // no user buffers available anymore
                    return;

                var userBuffer = pendingUserBuffers.Peek();

                if (userBuffer.ByteCount > dataNeededToFillCurSubBuffer) // (case 2: cut the buffers)
                {
                    AudioTrackWrite(userBuffer.Buffer, userBuffer.Offset, dataNeededToFillCurSubBuffer);

                    Interlocked.Increment(ref internalPendingBufferCount);
                    submittedBufferHandles.Enqueue(currentSubBufferHandles); // submit the handles (note: current buffer handle is added only in case (1))

                    userBuffer.Offset += dataNeededToFillCurSubBuffer;
                    userBuffer.ByteCount -= dataNeededToFillCurSubBuffer;
                    dataNeededToFillCurSubBuffer = SubBufferSize;
                    currentSubBufferHandles = new SubBufferDataHandles();
                }
                else
                {
                    // case 1: concat the buffers
                    AudioTrackWrite(userBuffer.Buffer, userBuffer.Offset, userBuffer.ByteCount);

                    currentSubBufferHandles.AddHandle();
                    dataNeededToFillCurSubBuffer -= userBuffer.ByteCount;

                    if (dataNeededToFillCurSubBuffer == 0) // case 3: the user buffer fit perfectly the size of our subbuffer
                    {
                        Interlocked.Increment(ref internalPendingBufferCount);
                        submittedBufferHandles.Enqueue(currentSubBufferHandles); // submit the handles (note: current buffer handle is added only in case (1))

                        dataNeededToFillCurSubBuffer = SubBufferSize;
                        currentSubBufferHandles = new SubBufferDataHandles();
                    }

                    // remove this buffer from the pending list because consumed
                    pendingUserBuffers.Dequeue();
                }
            }
        }

        internal override void StopImpl()
        {
            // For the DynamicSoundEffectInstance stop does not need to destroy/reconstruct the voice
            // It is done in above ClearBuffersImpl function. Moreover destruction/contruction need to be lock protected.
        }

        private void ClearBuffersImpl()
        {
            lock (internalLock)
            {
                // we need to destroy/reconstruct the voice here to be sure to clean all written data in the voice (flush is not working).
                // destroy/construct the voice need to be lock protected in order to avoid the callback to work with invalid voice.
                RebuildVoice();

                // remove all the user pending buffers.
                pendingUserBuffers.Clear();

                // free current buffer handles
                currentSubBufferHandles.FreeHandles();

                ResetBufferInfo();
            }
        }

        internal override void CreateVoice(WaveFormat format)
        {
        }

        private void ResetBufferInfo()
        {
            currentSubBufferHandles = new SubBufferDataHandles();
            dataNeededToFillCurSubBuffer = SubBufferSize;
            nbOfAudioFrameAvailable = 0;
        }

        internal override void PlatformSpecificDisposeImpl()
        {
            lock (internalLock)
            {
                DestroyVoice();
            }
        }


        /// <summary>
        /// A list of all the data handles lock for one subBuffer.
        /// </summary>
        private class SubBufferDataHandles
        {
            public void FreeHandles()
            {
                HandleCount = 0;
            }

            public int HandleCount { get; private set; }

            public void AddHandle()
            {
                HandleCount = HandleCount + 1;
            }
        };
    }
}

#endif
