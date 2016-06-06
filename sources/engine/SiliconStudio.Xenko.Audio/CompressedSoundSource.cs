using System;
using System.IO;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Audio
{
    internal sealed class CompressedSoundSource : SoundSource
    {
        internal const int SamplesPerFrame = 512;

        private readonly Stream compressedSoundStream;      
        private readonly Celt decoder;
        private readonly int channels;

        public CompressedSoundSource(string soundStreamUrl, int sampleRate, int channels) : base(channels)
        {
            compressedSoundStream = ContentManager.FileProvider.OpenStream(soundStreamUrl, VirtualFileMode.Open, VirtualFileAccess.Read, VirtualFileShare.Read, StreamFlags.Seekable);
            decoder = new Celt(sampleRate, SamplesPerFrame, channels, true);
            this.channels = channels;
            Initialize();
        }

        protected override unsafe Task Reader()
        {
            return new Task(() =>
            {
                var decodedBuffer = new byte[SamplesPerFrame];
                var reader = new BinarySerializationReader(compressedSoundStream);
                while (!CancellationTokenSource.Token.IsCancellationRequested)
                {
                    UnmanagedArray<short> buffer;
                    while (FreeBuffers.TryTake(out buffer, 20, CancellationTokenSource.Token))
                    {
                        const int passes = SamplesPerBuffer/SamplesPerFrame;
                        var offset = 0;
                        var bufferPtr = (short*)buffer.Pointer.ToPointer();
                        for (var i = 0; i < passes; i++)
                        {
                            var len = reader.ReadInt16();
                            compressedSoundStream.Read(decodedBuffer, 0, len);
                            if (compressedSoundStream.Position == compressedSoundStream.Length) 
                            {
                                compressedSoundStream.Position = 0; //reset if we reach the end
                            }

                            var writePtr = bufferPtr + offset;
                            if (decoder.Decode(decodedBuffer, len, writePtr) != SamplesPerFrame)
                            {
                                throw new Exception("Celt decoder returned a wrong decoding buffer size.");
                            }

                            offset += SamplesPerFrame * channels;
                        }
                        DirtyBuffers.Enqueue(buffer);
                    }
                }
            }, CancellationTokenSource.Token);
        }

        public override void Dispose()
        {
            base.Dispose();
            compressedSoundStream.Dispose();
            decoder.Dispose();
        }
    }
}
