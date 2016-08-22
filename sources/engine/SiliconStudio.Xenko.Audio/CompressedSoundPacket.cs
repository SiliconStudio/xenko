using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Used internally in SoundAsset
    /// </summary>
    [DataContract("CompressedSoundPacket")]
    public class CompressedSoundPacket
    {
        /// <summary>
        /// The length of the Data.
        /// </summary>
        public int Length;

        /// <summary>
        /// The Data.
        /// </summary>
        public byte[] Data;
    }
}
