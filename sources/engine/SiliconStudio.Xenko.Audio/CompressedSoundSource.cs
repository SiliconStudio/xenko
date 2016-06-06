using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly BinarySerializationReader reader;

        private readonly Celt decoder;

        private readonly int channels;

        private readonly int maxCompressedSize;
        private readonly byte[] compressedBuffer;

        private bool dispose;

        private static Thread readFromDiskWorker;
        private static ConcurrentBag<CompressedSoundSource> NewSources = new ConcurrentBag<CompressedSoundSource>();
        private static List<CompressedSoundSource> Sources = new List<CompressedSoundSource>();

        public CompressedSoundSource(string soundStreamUrl, int sampleRate, int channels, int maxCompressedSize) : base(channels)
        {
            compressedSoundStream = ContentManager.FileProvider.OpenStream(soundStreamUrl, VirtualFileMode.Open, VirtualFileAccess.Read, VirtualFileShare.Read, StreamFlags.Seekable);
            decoder = new Celt(sampleRate, SamplesPerFrame, channels, true);
            this.channels = channels;
            this.maxCompressedSize = maxCompressedSize;
            compressedBuffer = new byte[maxCompressedSize];
            reader = new BinarySerializationReader(compressedSoundStream);

            if (readFromDiskWorker == null)
            {
                readFromDiskWorker = new Thread(Worker) { IsBackground = true };
                readFromDiskWorker.Start();
            }

            NewSources.Add(this);
        }

        private static unsafe void Worker()
        {
            var toRemove = new List<CompressedSoundSource>();
            while (true)
            {
                toRemove.Clear();

                while (!NewSources.IsEmpty)
                {
                    CompressedSoundSource source;
                    if (NewSources.TryTake(out source))
                    {
                        Sources.Add(source);
                    }
                }

                foreach (var source in Sources)
                {
                    if (source.dispose)
                    {
                        source.Dispose2();
                        toRemove.Add(source);
                    }
                    else
                    {
                        SoundSourceBuffer buffer;
                        while (source.FreeBuffers.TryDequeue(out buffer))
                        {
                            buffer.EndOfStream = false;
                            buffer.Length = SamplesPerBuffer * source.channels;
                            const int passes = SamplesPerBuffer / SamplesPerFrame;
                            var offset = 0;
                            var bufferPtr = (short*)buffer.Buffer.Pointer.ToPointer();
                            for (var i = 0; i < passes; i++)
                            {
                                var len = source.reader.ReadInt16();
                                source.compressedSoundStream.Read(source.compressedBuffer, 0, len);

                                var writePtr = bufferPtr + offset;
                                if (source.decoder.Decode(source.compressedBuffer, len, writePtr) != SamplesPerFrame)
                                {
                                    throw new Exception("Celt decoder returned a wrong decoding buffer size.");
                                }

                                offset += SamplesPerFrame * source.channels;

                                if (source.compressedSoundStream.Position != source.compressedSoundStream.Length) continue;

                                buffer.EndOfStream = true;
                                buffer.Length = offset;
                                source.compressedSoundStream.Position = 0; //reset if we reach the end
                                break;
                            }
                            source.DirtyBuffers.Enqueue(buffer);
                        }
                    }
                }

                foreach (var source in toRemove)
                {
                    Sources.Remove(source);
                }

                Thread.Sleep(20);
            }
        }

        public override void Dispose()
        {
            dispose = true;
        }

        public void Dispose2()
        {
            base.Dispose();
            compressedSoundStream.Dispose();
            decoder.Dispose();
        }
    }
}
