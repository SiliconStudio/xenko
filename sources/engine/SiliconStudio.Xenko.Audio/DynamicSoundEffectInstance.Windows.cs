// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS && !SILICONSTUDIO_XENKO_SOUND_SDL

using System;
using System.Runtime.InteropServices;
using System.Threading;

using SharpDX.XAudio2;

namespace SiliconStudio.Xenko.Audio
{
    public partial class DynamicSoundEffectInstance
    {
        
        private void SubmitBufferImpl(byte[] buffer, int offset, int byteCount)
        {
            var gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            submittedBufferHandles.Enqueue(new SubBufferDataHandles(gcHandle));

            var audioBuffer = new AudioBuffer
                {
                    AudioDataPointer = gcHandle.AddrOfPinnedObject()+offset,
                    AudioBytes = byteCount,
                };

            SourceVoice.SubmitSourceBuffer(audioBuffer, null);

            Interlocked.Increment(ref pendingBufferCount);
            Interlocked.Increment(ref internalPendingBufferCount);
        }

        private void InitializeDynamicSound()
        {
            SourceVoice.BufferEnd += OnBufferEnd;
        }

        private void OnBufferEnd(IntPtr context)
        {
            OnBufferEndCommon();
        }

        private void ClearBuffersImpl()
        {
            SourceVoice.FlushSourceBuffers();
        }


        /// <summary>
        /// A list of all the data handles lock for one subBuffer.
        /// </summary>
        private class SubBufferDataHandles
        {
            private GCHandle handle;

            public SubBufferDataHandles(GCHandle handle) 
            {
                this.handle = handle;
            }

            public void FreeHandles()
            {
                handle.Free();
            }

            public int HandleCount { get { return 1; } }
        };
    }
}

#endif