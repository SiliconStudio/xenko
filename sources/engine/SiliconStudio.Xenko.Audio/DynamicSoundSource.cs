// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Audio
{ 

    public abstract class DynamicSoundSource : IDisposable
    {
        /// <summary>
        /// The possible async commands that can be queued and be handled by subclasses
        /// </summary>
        protected enum AsyncCommand
        {
            Play,
            Pause,
            Stop,
            SetRange,
            Dispose
        }

        /// <summary>
        /// The commands derived classes should execute.
        /// </summary>
        protected readonly ConcurrentQueue<AsyncCommand> Commands = new ConcurrentQueue<AsyncCommand>();

        private bool readyToPlay;
        private int prebufferedCount;
        private readonly int prebufferedTarget;

        /// <summary>
        /// Gets a task that will be fired once the source is ready to play.
        /// </summary>
        public TaskCompletionSource<bool> ReadyToPlay { get; private set; } = new TaskCompletionSource<bool>(false);

        /// <summary>
        /// Gets a task that will be fired once there will be no more queueud data.
        /// </summary>
        public TaskCompletionSource<bool> Ended { get; private set; } = new TaskCompletionSource<bool>(false);

        private readonly List<AudioLayer.Buffer> deviceBuffers = new List<AudioLayer.Buffer>();
        private readonly Queue<AudioLayer.Buffer> freeBuffers = new Queue<AudioLayer.Buffer>(4);

        /// <summary>
        /// The sound instance associated.
        /// </summary>
        protected SoundInstance SoundInstance;
        /// <summary>
        /// If we are in the disposed state.
        /// </summary>
        protected bool Disposed;
        /// <summary>
        /// If we are in the playing state.
        /// </summary>
        protected bool Playing;
        /// <summary>
        /// If we are in the paused state.
        /// </summary>
        protected bool Paused;
        /// <summary>
        /// If we are waiting to play.
        /// </summary>
        protected volatile bool PlayingQueued;
        /// <summary>
        /// If the source is actually playing sound
        /// this takes into account multiple factors: Playing, Ended task, and Audio layer playing state
        /// </summary>
        protected volatile bool PlayingState = false;

        /// <summary>
        /// Sub classes can implement their own streaming sources.
        /// </summary>
        /// <param name="soundInstance">the sound instance associated.</param>
        /// <param name="numberOfBuffers">the size of the streaming ring-buffer.</param>
        /// <param name="maxBufferSizeBytes">the maximum size of each buffer.</param>
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

        /// <summary>
        /// Enqueues a dispose command, to dispose this instance.
        /// </summary>
        public virtual void Dispose()
        {
            Commands.Enqueue(AsyncCommand.Dispose);
        }

        /// <summary>
        /// Destroys the instance.
        /// </summary>
        protected virtual void Destroy()
        {
            foreach (var deviceBuffer in deviceBuffers)
            {
                AudioLayer.BufferDestroy(deviceBuffer);
            }
        }

        /// <summary>
        /// Checks if a buffer can be filled, before calling FillBuffer this should be checked.
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

        /// <summary>
        /// Max number of buffers that are going to be queued.
        /// </summary>
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

        /// <summary>
        /// Enqueues a Play command, to Play this instance.
        /// </summary>
        public void Play()
        {
            PlayingQueued = true;
            Commands.Enqueue(AsyncCommand.Play);
        }

        /// <summary>
        /// Enqueues a Pause command, to Pause this instance.
        /// </summary>
        public void Pause()
        {
            Commands.Enqueue(AsyncCommand.Pause);
        }

        /// <summary>
        /// Enqueues a Stop command, to Stop this instance.
        /// </summary>
        public void Stop()
        {
            Commands.Enqueue(AsyncCommand.Stop);
        }

        /// <summary>
        /// Gets if this instance is in the playing state.
        /// </summary>
        public bool IsPlaying => PlayingQueued || PlayingState;

        /// <summary>
        /// Sets the region of time to play from the audio clip.
        /// </summary>
        /// <param name="range">a PlayRange structure that describes the starting offset and ending point of the sound to play in seconds.</param>
        public virtual void SetRange(PlayRange range)
        {
            Commands.Enqueue(AsyncCommand.SetRange);
        }

        /// <summary>
        /// Sets if the stream should be played in loop.
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
            Ended.TrySetResult(false);
            Ended = new TaskCompletionSource<bool>();
            readyToPlay = false;
            prebufferedCount = 0;
        }
    }
}
