using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        private volatile bool looped;
        private volatile bool restart;
        private readonly int numberOfPackets;
        private int currentPacketIndex;
        private int startingPacketIndex;
        private int endPacketIndex;
        private PlayRange playRange;
        private int startPktSampleIndex;
        private int endPktSampleIndex;
        private bool begin;
        private readonly object rangeLock = new object();

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

        /// <summary>
        /// This type of DynamicSoundSource is streamed from Disk and reads compressed Celt encoded data, used internally.
        /// </summary>
        /// <param name="instance">The associated SoundInstance</param>
        /// <param name="soundStreamUrl">The compressed stream internal URL</param>
        /// <param name="numberOfPackets"></param>
        /// <param name="sampleRate">The sample rate of the compressed data</param>
        /// <param name="channels">The number of channels of the compressed data</param>
        /// <param name="maxCompressedSize">The maximum size of a compressed packet</param>
        public CompressedSoundSource(SoundInstance instance, string soundStreamUrl, int numberOfPackets, int sampleRate, int channels, int maxCompressedSize) : base(instance, NumberOfBuffers, SamplesPerBuffer * MaxChannels * sizeof(short))
        {
            looped = instance.IsLooped;
            this.channels = channels;
            this.maxCompressedSize = maxCompressedSize;
            this.soundStreamUrl = soundStreamUrl;
            this.sampleRate = sampleRate;
            this.numberOfPackets = numberOfPackets;
            playRange = new PlayRange(TimeSpan.Zero, TimeSpan.Zero);

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
                    source.restart = true;

                    Sources.Add(source);
                }

                foreach (var source in Sources)
                {
                    if (!source.dispose)
                    {
                        if (source.restart)
                        {
                            source.compressedSoundStream.Position = 0;
                            source.begin = true;
                            source.ended = false;
                            source.restart = false;                           
                            source.currentPacketIndex = 0;
                            source.startPktSampleIndex = 0;
                            source.endPktSampleIndex = 0;
                            source.endPacketIndex = source.numberOfPackets;                          

                            PlayRange range;
                            lock (source.rangeLock)
                            {
                                range = source.playRange;
                            }

                            if (range.Start != TimeSpan.Zero || range.Length != TimeSpan.Zero)
                            {
                                var frameSize = SamplesPerFrame*source.channels;
                                //ok we need to handle this case properly, this means that the user wants to use a different then full audio stream range...
                                var sampleStart = source.sampleRate * (double)source.channels * range.Start.TotalSeconds;
                                source.startPktSampleIndex = (int)Math.Floor(sampleStart) % (frameSize);

                                var sampleStop = source.sampleRate * (double)source.channels * range.End.TotalSeconds;
                                source.endPktSampleIndex = frameSize - (int)Math.Floor(sampleStart) % frameSize;

                                var skipCounter = source.startingPacketIndex = (int)Math.Floor(sampleStart / frameSize);
                                source.endPacketIndex = (int)Math.Floor(sampleStop / frameSize);
                                
                                // skip to the starting packet
                                if (source.startingPacketIndex < source.numberOfPackets && source.endPacketIndex < source.numberOfPackets && source.startingPacketIndex < source.endPacketIndex)
                                {
                                    //valid offsets.. process it
                                    while (skipCounter-- > 0)
                                    {
                                        //skip data to reach starting packet
                                        var len = source.reader.ReadInt16();
                                        source.compressedSoundStream.Position = source.compressedSoundStream.Position + len;
                                        source.currentPacketIndex++;
                                    }
                                }
                            }

                            source.restartCompletionSource?.TrySetResult(true);
                        }

                        if (source.ended || !source.CanFill) continue;

                        const int passes = SamplesPerBuffer / SamplesPerFrame;
                        var offset = 0;
                        var bufferPtr = (short*)utilityBuffer.Pointer;
                        var startingPacket = source.startingPacketIndex == source.currentPacketIndex;
                        var endingPacket = false;
                        for (var i = 0; i < passes; i++)
                        {
                            endingPacket = source.endPacketIndex == source.currentPacketIndex;

                            //read one packet, size first, then data
                            var len = source.reader.ReadInt16();
                            source.compressedSoundStream.Read(source.compressedBuffer, 0, len);
                            source.currentPacketIndex++;

                            var writePtr = bufferPtr + offset;
                            if (source.decoder.Decode(source.compressedBuffer, len, writePtr) != SamplesPerFrame)
                            {
                                throw new Exception("Celt decoder returned a wrong decoding buffer size.");
                            }

                            offset += SamplesPerFrame * source.channels;

                            if (source.compressedSoundStream.Position != source.compressedSoundStream.Length && source.currentPacketIndex < source.endPacketIndex) continue;

                            if (source.looped)
                            {
                                source.restart = true;
                            }
                            else
                            {
                                source.ended = true;
                                source.Ended.TrySetResult(true);
                            }

                            break;
                        }

                        if (source.restart) continue;

                        var finalPtr = new IntPtr(bufferPtr + (startingPacket ? source.startPktSampleIndex : 0));
                        var finalSize = (offset - (startingPacket ? source.startPktSampleIndex : 0) - (endingPacket ? source.endPktSampleIndex : 0))*sizeof(short);

                        var bufferType = AudioLayer.BufferType.None;
                        if (source.ended)
                        {
                            bufferType = AudioLayer.BufferType.EndOfStream;
                        }
                        else if (source.begin)
                        {
                            bufferType = AudioLayer.BufferType.BeginOfStream;
                            source.begin = false;
                        }
                        else if (source.looped && source.restart)
                        {
                            bufferType = AudioLayer.BufferType.EndOfLoop;
                        }

                        source.FillBuffer(finalPtr, finalSize, bufferType);
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

                NativeInvoke.Sleep(20);
            }
        }

        public override void Dispose()
        {
            dispose = true;
        }

        public override int MaxNumberOfBuffers => NumberOfBuffers;

        private TaskCompletionSource<bool> restartCompletionSource;

        public override async Task Restart()
        {
            restartCompletionSource = new TaskCompletionSource<bool>();
            restart = true;
            await restartCompletionSource.Task;
            await base.Restart();          
        }

        /// <summary>
        /// Sets if the stream should be played in loop
        /// </summary>
        /// <param name="loop">if looped or not</param>
        public override void SetLooped(bool loop)
        {
            looped = loop;
        }

        public override async Task SetRange(PlayRange range)
        {
            playRange = range;
            await Restart();
        }

        private void Destroy()
        {
            base.Dispose();

            compressedSoundStream.Dispose();

            decoder.Dispose();
        }
    }
}
