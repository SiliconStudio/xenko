using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Audio
{
    public class SoundSourceBuffer
    {
        public UnmanagedArray<short> Buffer;
        public bool EndOfStream;
        public int Length;
    }

    public abstract class SoundSource : IDisposable
    {
        public const int SamplesPerBuffer = 32768;
        internal const int NumberOfBuffers = 4;

        protected ConcurrentQueue<SoundSourceBuffer> FreeBuffers { get; } = new ConcurrentQueue<SoundSourceBuffer>();

        protected ConcurrentQueue<SoundSourceBuffer> DirtyBuffers { get; } = new ConcurrentQueue<SoundSourceBuffer>();

        private readonly List<UnmanagedArray<short>> buffersToDispose = new List<UnmanagedArray<short>>();

        public TaskCompletionSource<bool> ReadyToPlay { get; } = new TaskCompletionSource<bool>(false);

        protected SoundSource(int channels)
        {
            for (var i = 0; i < NumberOfBuffers; i++)
            {
                var buffer = new SoundSourceBuffer { Buffer = new UnmanagedArray<short>(channels*SamplesPerBuffer) };
                FreeBuffers.Enqueue(buffer);
                buffersToDispose.Add(buffer.Buffer);
            }
        }

        public bool ReadSamples(out SoundSourceBuffer buffer)
        {
            return DirtyBuffers.TryDequeue(out buffer);
        }

        public void ReleaseSamples(SoundSourceBuffer buffer)
        {
            FreeBuffers.Enqueue(buffer);
        }

        public virtual void Dispose()
        {
            foreach (var array in buffersToDispose)
            {
                array.Dispose();
            }
        }
    }
}
