using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Audio
{ 

    public abstract class DynamicSoundSource : IDisposable
    {
        private bool readyToPlay;
        private int prebufferedCount;

        /// <summary>
        /// This will be fired internally once there are more then 1 buffer in the queue
        /// </summary>
        public TaskCompletionSource<bool> ReadyToPlay { get; } = new TaskCompletionSource<bool>(false);
        
        /// <summary>
        /// This must be filled by sub-classes implementation when there will be no more queueud data
        /// </summary>
        public TaskCompletionSource<bool> Ended { get; } = new TaskCompletionSource<bool>(false);

        private readonly List<uint> deviceBuffers = new List<uint>();
        private readonly Queue<uint> freeBuffers = new Queue<uint>(4);

        protected SoundInstance SoundInstance;

        protected DynamicSoundSource(SoundInstance soundInstance, int numberOfBuffers)
        {
            SoundInstance = soundInstance;
            for (var i = 0; i < numberOfBuffers; i++)
            {
                var buffer = OpenAl.BufferCreate();
                deviceBuffers.Add(buffer);
                freeBuffers.Enqueue(deviceBuffers[i]);
            }
        }

        public virtual void Dispose()
        {
            foreach (var deviceBuffer in deviceBuffers)
            {
                OpenAl.BufferDestroy(deviceBuffer);
            }
        }

        protected bool CanFill
        {
            get
            {
                if (freeBuffers.Count > 0) return true;
                var freeBuffer = OpenAl.SourceGetFreeBuffer(SoundInstance.Source);
                if (freeBuffer <= 0) return false;
                freeBuffers.Enqueue(freeBuffer);
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcm"></param>
        /// <param name="bufferSize"></param>
        /// <param name="sampleRate"></param>
        /// <param name="mono"></param>
        protected void FillBuffer(IntPtr pcm, int bufferSize, int sampleRate, bool mono)
        {
            var buffer = freeBuffers.Dequeue();
            OpenAl.SourceQueueBuffer(SoundInstance.Source, buffer, pcm, bufferSize, sampleRate, mono);
            if (readyToPlay) return;

            prebufferedCount++;
            if (prebufferedCount > 1) return;

            readyToPlay = true;
            ReadyToPlay.TrySetResult(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcm"></param>
        /// <param name="bufferSize"></param>
        /// <param name="sampleRate"></param>
        /// <param name="mono"></param>
        protected unsafe void FillBuffer(short[] pcm, int bufferSize, int sampleRate, bool mono)
        {
            var buffer = freeBuffers.Dequeue();
            fixed(short* pcmBuffer = pcm)
            OpenAl.SourceQueueBuffer(SoundInstance.Source, buffer, new IntPtr(pcmBuffer), bufferSize, sampleRate, mono);
            if (readyToPlay) return;

            prebufferedCount++;
            if (prebufferedCount > 1) return;

            readyToPlay = true;
            ReadyToPlay.TrySetResult(true);
        }

        public abstract void Restart();

        public abstract void SetLooped(bool looped);
    }
}
