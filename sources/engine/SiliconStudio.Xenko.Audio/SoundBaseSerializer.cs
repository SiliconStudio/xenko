using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Native;

namespace SiliconStudio.Xenko.Audio
{
    public class SoundBaseSerializer : DataSerializer<SoundBase>
    {
        public override void Serialize(ref SoundBase obj, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Deserialize)
            {
                obj.CompressedDataUrl = stream.ReadString();
                obj.SampleRate = stream.ReadInt32();
                obj.SamplesPerFrame = stream.ReadInt16();
                obj.Channels = stream.ReadByte();
                obj.StreamFromDisk = stream.ReadBoolean();
                obj.Spatialized = stream.ReadBoolean();
                obj.NumberOfPackets = stream.ReadInt16();
                obj.MaxPacketLength = stream.ReadInt16();

                obj.CompressedDataStream = ContentManager.FileProvider.OpenStream(obj.CompressedDataUrl, VirtualFileMode.Open, VirtualFileAccess.Read);
                if (!obj.StreamFromDisk) //immediatelly preload all the data and decode
                {
                    var reader = new BinarySerializationReader(obj.CompressedDataStream);
                    using (var decoder = new Celt(obj.SampleRate, obj.SamplesPerFrame, obj.Channels, true))
                    {
                        var samplesPerPacket = obj.SamplesPerFrame*obj.Channels;

                        obj.PreloadedData = new UnmanagedArray<short>(samplesPerPacket*obj.NumberOfPackets);

                        var offset = 0;
                        var inputBuffer = new byte[obj.MaxPacketLength];
                        var outputBuffer = new short[samplesPerPacket];
                        for (var i = 0; i < obj.NumberOfPackets; i++)
                        {
                            var len = reader.ReadInt16();
                            obj.CompressedDataStream.Read(inputBuffer, (int)obj.CompressedDataStream.Position, len);
                            decoder.Decode(inputBuffer, len, outputBuffer);
                            obj.PreloadedData.Write(outputBuffer, offset);
                            offset += samplesPerPacket;
                        }

                        obj.CompressedDataStream.Dispose();
                        obj.CompressedDataStream = null;
                    }
                }
            }
            else
            {
                stream.Write(obj.CompressedDataUrl);
                stream.Write(obj.SampleRate);
                stream.Write(obj.SamplesPerFrame);
                stream.Write(obj.Channels);
                stream.Write(obj.StreamFromDisk);
                stream.Write(obj.Spatialized);
                stream.Write(obj.NumberOfPackets);
                stream.Write(obj.MaxPacketLength);
            }
        }
    }
}
