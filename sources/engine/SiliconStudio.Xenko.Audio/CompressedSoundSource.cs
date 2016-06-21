using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Audio
{
    internal sealed class CompressedSoundSource : DynamicSoundSource
    {
        private const int SamplesPerBuffer = 32768;
        private const int MaxChannels = 2;
        internal const int NumberOfBuffers = 4;
        internal const int SamplesPerFrame = 512;

        private Stream compressedSoundStream;
        private BinarySerializationReader reader;
        private bool ended;
        private bool looped;
        private bool restart;

        private Celt decoder;

        private readonly string soundStreamUrl;

        private readonly int channels;
        private readonly int sampleRate;

        private readonly int maxCompressedSize;
        private byte[] compressedBuffer;

        private bool dispose;       

        private static Task readFromDiskWorker;
        private static readonly ConcurrentBag<CompressedSoundSource> NewSources = new ConcurrentBag<CompressedSoundSource>();
        private static readonly List<CompressedSoundSource> Sources = new List<CompressedSoundSource>();
        
        public CompressedSoundSource(SoundInstance instance, string soundStreamUrl, int sampleRate, int channels, int maxCompressedSize) : base(instance, NumberOfBuffers, SamplesPerBuffer * MaxChannels * sizeof(short))
        {
            looped = instance.IsLooped;
            this.channels = channels;
            this.maxCompressedSize = maxCompressedSize;
            this.soundStreamUrl = soundStreamUrl;
            this.sampleRate = sampleRate;

            if (readFromDiskWorker == null)
            {
                readFromDiskWorker = Task.Factory.StartNew(Worker, TaskCreationOptions.LongRunning);
            }

            NewSources.Add(this);
        }

        private static unsafe void Worker()
        {
            var utilityBuffer = new UnmanagedArray<short>(SamplesPerBuffer * MaxChannels);

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
                        if (source.restart)
                        {
                            source.compressedSoundStream.Position = 0;
                            source.ended = false;
                            source.restart = false;
                        }

                        if (source.ended || !source.CanFill) continue;

                        const int passes = SamplesPerBuffer / SamplesPerFrame;
                        var offset = 0;
                        var bufferPtr = (short*)utilityBuffer.Pointer;
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
                            if (source.looped)
                            {
                                source.compressedSoundStream.Position = 0; //reset if we reach the end
                            }
                            else
                            {
                                source.ended = true;
                                source.Ended.TrySetResult(true);
                            }
                            break;
                        }
                        
                        source.FillBuffer(utilityBuffer.Pointer, offset * sizeof(short), source.ended);
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

                AudioLayer.xnSleep(20);
            }
        }

        public override void Dispose()
        {
            dispose = true;
        }

        public override int MaxNumberOfBuffers => MaxNumberOfBuffers;

        public override void Restart()
        {
            restart = true;
        }

        public override void SetLooped(bool loop)
        {
            looped = loop;
        }

        private void Destroy()
        {
            base.Dispose();

            compressedSoundStream.Dispose();

            decoder.Dispose();
        }
    }
}
