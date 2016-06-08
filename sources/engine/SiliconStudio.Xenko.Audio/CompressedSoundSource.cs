using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Audio
{
    internal sealed class CompressedSoundSource : SoundSource
    {
        internal const int SamplesPerFrame = 512;

        private Stream compressedSoundStream;
        private BinarySerializationReader reader;

        private Celt decoder;

        private readonly string soundStreamUrl;

        private readonly int channels;
        private readonly int sampleRate;

        private readonly int maxCompressedSize;
        private byte[] compressedBuffer;

        private bool dispose;
        private bool readyToPlay;

        private static Thread readFromDiskWorker;
        private static readonly ConcurrentBag<CompressedSoundSource> NewSources = new ConcurrentBag<CompressedSoundSource>();
        private static readonly List<CompressedSoundSource> Sources = new List<CompressedSoundSource>();
        

        public CompressedSoundSource(string soundStreamUrl, int sampleRate, int channels, int maxCompressedSize) : base(channels)
        {
            this.channels = channels;
            this.maxCompressedSize = maxCompressedSize;
            this.soundStreamUrl = soundStreamUrl;
            this.sampleRate = sampleRate;

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
                    if (!NewSources.TryTake(out source)) continue;

                    source.compressedSoundStream = ContentManager.FileProvider.OpenStream(source.soundStreamUrl, VirtualFileMode.Open, VirtualFileAccess.Read, VirtualFileShare.Read, StreamFlags.Seekable);
                    source.decoder = new Celt(source.sampleRate, SamplesPerFrame, source.channels, true);
                    source.compressedBuffer = new byte[source.maxCompressedSize];
                    source.reader = new BinarySerializationReader(source.compressedSoundStream);

                    Sources.Add(source);
                }

                foreach (var source in Sources)
                {
                    if (!source.dispose)
                    {
                        SoundSourceBuffer buffer;
                        while (source.FreeBuffers.TryDequeue(out buffer))
                        {
                            buffer.EndOfStream = false;
                            buffer.Length = SamplesPerBuffer*source.channels;
                            const int passes = SamplesPerBuffer/SamplesPerFrame;
                            var offset = 0;
                            var bufferPtr = (short*)buffer.Buffer.Pointer;
                            for (var i = 0; i < passes; i++)
                            {
                                var len = source.reader.ReadInt16();
                                source.compressedSoundStream.Read(source.compressedBuffer, 0, len);

                                var writePtr = bufferPtr + offset;
                                if (source.decoder.Decode(source.compressedBuffer, len, writePtr) != SamplesPerFrame)
                                {
                                    throw new Exception("Celt decoder returned a wrong decoding buffer size.");
                                }

                                offset += SamplesPerFrame*source.channels;

                                if (source.compressedSoundStream.Position != source.compressedSoundStream.Length) continue;

                                buffer.EndOfStream = true;
                                buffer.Length = offset;
                                source.compressedSoundStream.Position = 0; //reset if we reach the end
                                break;
                            }
                            source.DirtyBuffers.Enqueue(buffer);
                        }

                        if (!source.readyToPlay)
                        {
                            source.readyToPlay = true;
                            source.ReadyToPlay.TrySetResult(true);
                        }
                    }
                    else
                    {
                        toRemove.Add(source);
                    }
                }

                foreach (var source in toRemove)
                {
                    source.Destroy();
                    Sources.Remove(source);
                }

                Thread.Sleep(20);
            }
        }

        public override void Dispose()
        {
            dispose = true;
        }

        private void Destroy()
        {
            base.Dispose();
            compressedSoundStream.Dispose();
            decoder.Dispose();
        }
    }
}
