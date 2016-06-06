using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Audio
{
    public interface IAudioEngineProvider
    {
        AudioEngine AudioEngine { get; }
    }

    public class SoundBaseSerializer : DataSerializer<Sound>
    {
        public override void Serialize(ref Sound obj, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Deserialize)
            {
                var services = stream.Context.Tags.Get(ServiceRegistry.ServiceRegistryKey);
                var audioEngine = services.GetServiceAs<IAudioEngineProvider>()?.AudioEngine;

                obj.CompressedDataUrl = stream.ReadString();
                obj.SampleRate = stream.ReadInt32();
                obj.Channels = stream.ReadByte();
                obj.StreamFromDisk = stream.ReadBoolean();
                obj.Spatialized = stream.ReadBoolean();
                obj.NumberOfPackets = stream.ReadInt16();
                obj.MaxPacketLength = stream.ReadInt16();
                
                if (!obj.StreamFromDisk) //immediatelly preload all the data and decode
                {
                    using (var soundStream = ContentManager.FileProvider.OpenStream(obj.CompressedDataUrl, VirtualFileMode.Open, VirtualFileAccess.Read, VirtualFileShare.Read, StreamFlags.Seekable))
                    using (var decoder = new Celt(obj.SampleRate, CompressedSoundSource.SamplesPerFrame, obj.Channels, true))
                    {
                        var reader = new BinarySerializationReader(soundStream);
                        var samplesPerPacket = CompressedSoundSource.SamplesPerFrame*obj.Channels;

                        obj.PreloadedData = new UnmanagedArray<short>(samplesPerPacket*obj.NumberOfPackets);

                        var offset = 0;
                        var outputBuffer = new short[samplesPerPacket];
                        for (var i = 0; i < obj.NumberOfPackets; i++)
                        {
                            var len = reader.ReadInt16();
                            var compressedBuffer = reader.ReadBytes(len);
                            var samplesDecoded = decoder.Decode(compressedBuffer, len, outputBuffer);
                            obj.PreloadedData.Write(outputBuffer, offset, 0, samplesDecoded*obj.Channels);
                            offset += samplesDecoded*obj.Channels*sizeof(short);
                        }
                    }
                }

                if (audioEngine != null)
                {
                    obj.Attach(audioEngine);
                }
            }
            else
            {
                stream.Write(obj.CompressedDataUrl);
                stream.Write(obj.SampleRate);
                stream.Write((byte)obj.Channels);
                stream.Write(obj.StreamFromDisk);
                stream.Write(obj.Spatialized);
                stream.Write((short)obj.NumberOfPackets);
                stream.Write((short)obj.MaxPacketLength);
            }
        }
    }
}
