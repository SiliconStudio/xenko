using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Audio
{ 

    public abstract class DynamicSoundSource : IDisposable
    {
        protected enum AsyncCommand
        {
            Play,
            Pause,
            Stop,
            SetRange,
            Dispose
        }

        protected ConcurrentQueue<AsyncCommand> Commands = new ConcurrentQueue<AsyncCommand>();

        private bool readyToPlay;
        private int prebufferedCount;
        private readonly int prebufferedTarget;

        /// <summary>
        /// This will be fired internally once there are more then 1 buffer in the queue
        /// </summary>
        public TaskCompletionSource<bool> ReadyToPlay { get; private set; } = new TaskCompletionSource<bool>(false);
        
        /// <summary>
        /// This must be filled by sub-classes implementation when there will be no more queueud data
        /// </summary>
        public TaskCompletionSource<bool> Ended { get; } = new TaskCompletionSource<bool>(false);

        private readonly List<AudioLayer.Buffer> deviceBuffers = new List<AudioLayer.Buffer>();
        private readonly Queue<AudioLayer.Buffer> freeBuffers = new Queue<AudioLayer.Buffer>(4);

        protected SoundInstance SoundInstance;

        protected bool Disposed = false;
        protected bool Playing = false;
        protected bool Paused = false;

        /// <summary>
        /// Sub classes can implement their own streaming sources
        /// </summary>
        /// <param name="soundInstance">the sound instance associated</param>
        /// <param name="numberOfBuffers">the size of the streaming ring-buffer</param>
        /// <param name="maxBufferSizeBytes">the maximum size of each buffer</param>
        protected DynamicSoundSource(SoundInstance soundInstance, int numberOfBuffers, int maxBufferSizeBytes)
        {
            prebufferedTarget = (int)Math.Ceiling(numberOfBuffers/(double)3);

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
            Commands.Enqueue(AsyncCommand.Dispose);
        }

        protected virtual void Destroy()
        {
            foreach (var deviceBuffer in deviceBuffers)
            {
                AudioLayer.BufferDestroy(deviceBuffer);
            }
        }

        /// <summary>
        /// Checks if a buffer can be filled, before calling FillBuffer this should be checked
        /// </summary>
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

        public abstract int MaxNumberOfBuffers { get; }

        /// <summary>
        /// If CanFillis true with this method you can fill the next free buffer
        /// </summary>
        /// <param name="pcm">The pointer to PCM data</param>
        /// <param name="bufferSize">The full size in bytes of PCM data</param>
        /// <param name="type">If this buffer is the last buffer of the stream set to true, if not false</param>
        protected void FillBuffer(IntPtr pcm, int bufferSize, AudioLayer.BufferType type)
        {
            var buffer = freeBuffers.Dequeue();
            AudioLayer.SourceQueueBuffer(SoundInstance.Source, buffer, pcm, bufferSize, type);
            if (readyToPlay) return;

            prebufferedCount++;
            if (prebufferedCount < prebufferedTarget) return;
            readyToPlay = true;
            ReadyToPlay.TrySetResult(true);
        }

        /// <summary>
        /// If CanFillis true with this method you can fill the next free buffer
        /// </summary>
        /// <param name="pcm">The array containing PCM data</param>
        /// <param name="bufferSize">The full size in bytes of PCM data</param>
        /// <param name="type">If this buffer is the last buffer of the stream set to true, if not false</param>
        protected unsafe void FillBuffer(short[] pcm, int bufferSize, AudioLayer.BufferType type)
        {
            var buffer = freeBuffers.Dequeue();
            fixed(short* pcmBuffer = pcm)
            AudioLayer.SourceQueueBuffer(SoundInstance.Source, buffer, new IntPtr(pcmBuffer), bufferSize, type);
            if (readyToPlay) return;

            prebufferedCount++;
            if (prebufferedCount < prebufferedTarget) return;
            readyToPlay = true;
            ReadyToPlay.TrySetResult(true);
        }

        public void Play()
        {
            Commands.Enqueue(AsyncCommand.Play);
        }

        public void Pause()
        {
            Commands.Enqueue(AsyncCommand.Pause);
        }

        public void Stop()
        {
            Commands.Enqueue(AsyncCommand.Stop);
        }

        /// <summary>
        /// Sets the region of time to play from the sample
        /// </summary>
        /// <param name="range"></param>
        public virtual void SetRange(PlayRange range)
        {
            Commands.Enqueue(AsyncCommand.SetRange);
        }

        /// <summary>
        /// Sets if the stream should be played in loop
        /// </summary>
        /// <param name="looped">if looped or not</param>
        public abstract void SetLooped(bool looped);

        /// <summary>
        /// Restarts streaming from the beginning.
        /// </summary>
        protected void Restart()
        {
            ReadyToPlay.TrySetResult(false);
            ReadyToPlay = new TaskCompletionSource<bool>();
            readyToPlay = false;
            prebufferedCount = 0;
        }
    }
}
