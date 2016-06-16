using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Audio
{
    [DataContract("CompressedSoundPacket")]
    public class CompressedSoundPacket
    {
        public int Length;

        public byte[] Data;
    }
}
