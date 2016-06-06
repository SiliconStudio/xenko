using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Audio
{
    public abstract class SoundSource : IDisposable
    {
        public const int SamplesPerBuffer = 65536;
        internal const int NumberOfBuffers = 4;

        protected readonly BlockingCollection<UnmanagedArray<short>> FreeBuffers = new BlockingCollection<UnmanagedArray<short>>(NumberOfBuffers);
        protected readonly ConcurrentQueue<UnmanagedArray<short>> DirtyBuffers = new ConcurrentQueue<UnmanagedArray<short>>();
        protected readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        protected readonly List<UnmanagedArray<short>> BuffersToDispose = new List<UnmanagedArray<short>>();

        protected static readonly TaskScheduler DecoderScheduler = new LimitedConcurrencyLevelTaskScheduler(2);
        protected Task FillerTask;

        protected SoundSource(int channels)
        {
            for (var i = 0; i < NumberOfBuffers; i++)
            {
                var buffer = new UnmanagedArray<short>(channels* SamplesPerBuffer);
                FreeBuffers.Add(buffer);
                BuffersToDispose.Add(buffer);
            }
        }

        protected virtual void Initialize()
        {
            FillerTask = Reader();
            FillerTask.Start(DecoderScheduler);
        }

        protected abstract Task Reader();

        public bool ReadSamples(out UnmanagedArray<short> outSamples)
        {
            return DirtyBuffers.TryDequeue(out outSamples);
        }

        public void ReleaseSamples(UnmanagedArray<short> samples)
        {
            FreeBuffers.Add(samples);
        }

        public virtual void Dispose()
        {
            CancellationTokenSource.Cancel();
            FillerTask.Wait();
            foreach (var array in BuffersToDispose)
            {
                array.Dispose();
            }
        }
    }
}
