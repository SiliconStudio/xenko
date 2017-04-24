// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
