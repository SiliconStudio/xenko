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

        private readonly List<AudioLayer.Buffer> deviceBuffers = new List<AudioLayer.Buffer>();
        private readonly Queue<AudioLayer.Buffer> freeBuffers = new Queue<AudioLayer.Buffer>(4);

        protected SoundInstance SoundInstance;

        protected DynamicSoundSource(SoundInstance soundInstance, int numberOfBuffers, int maxBufferSizeBytes)
        {
            SoundInstance = soundInstance;
            for (var i = 0; i < numberOfBuffers; i++)
            {
                var buffer = AudioLayer.BufferCreate(maxBufferSizeBytes);
                deviceBuffers.Add(buffer);
                freeBuffers.Enqueue(deviceBuffers[i]);
            }
        }

        public virtual void Dispose()
        {
            foreach (var deviceBuffer in deviceBuffers)
            {
                AudioLayer.BufferDestroy(deviceBuffer);
            }
        }

        protected bool CanFill
        {
            get
            {
                if (freeBuffers.Count > 0) return true;
                var freeBuffer = AudioLayer.SourceGetFreeBuffer(SoundInstance.Source);
                if (freeBuffer.Ptr == IntPtr.Zero) return false;
                freeBuffers.Enqueue(freeBuffer);
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pcm"></param>
        /// <param name="bufferSize"></param>
        /// <param name="maxBufferSize"></param>
        /// <param name="endOfStream"></param>
        protected void FillBuffer(IntPtr pcm, int bufferSize, bool endOfStream)
        {
            var buffer = freeBuffers.Dequeue();
            AudioLayer.SourceQueueBuffer(SoundInstance.Source, buffer, pcm, bufferSize, endOfStream);
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
        /// <param name="maxBufferSize"></param>
        /// <param name="endOfStream"></param>
        protected unsafe void FillBuffer(short[] pcm, int bufferSize, bool endOfStream)
        {
            var buffer = freeBuffers.Dequeue();
            fixed(short* pcmBuffer = pcm)
            AudioLayer.SourceQueueBuffer(SoundInstance.Source, buffer, new IntPtr(pcmBuffer), bufferSize, endOfStream);
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
