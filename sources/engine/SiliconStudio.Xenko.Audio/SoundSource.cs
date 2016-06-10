using System;
using System.Threading.Tasks;

namespace SiliconStudio.Xenko.Audio
{ 

    public abstract class SoundSource : IDisposable
    {
        public const int SamplesPerBuffer = 32768;
        public const int MaxChannels = 2;
        internal const int NumberOfBuffers = 4;

        public TaskCompletionSource<bool> ReadyToPlay { get; } = new TaskCompletionSource<bool>(false);

        private readonly uint[] deviceBuffers = new uint[4];

        protected SoundInstance SoundInstance;

        protected SoundSource(SoundInstance soundInstance)
        {
            SoundInstance = soundInstance;
            for (var i = 0; i < NumberOfBuffers; i++)
            {
                deviceBuffers[i] = Native.OpenAl.AudioCreateBuffer();
            }
        }

        public virtual void Dispose()
        {
        }
    }
}
