using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Used internally in SoundAsset
    /// </summary>
    [DataContract("CompressedSoundPacket")]
    public class CompressedSoundPacket
    {
        public int Length;

        public byte[] Data;
    }
}
