// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Audio;

namespace SiliconStudio.Xenko.Assets.Audio
{
    [DataContract("Sound")]
    [AssetDescription(FileExtension)]
    [AssetContentType(typeof(Sound))]
    [Display(1200, "Sound")]
    public class SoundAsset : AssetWithSource
    {
        /// <summary>
        /// The default file extension used by the <see cref="SoundAsset"/>.
        /// </summary>
        public const string FileExtension = ".xksnd;.pdxsnd";

        [DefaultValue(44100)]
        public int SampleRate { get; set; } = 44100;

        [DefaultValue(10)]
        [DataMemberRange(1, 40, 1, 5, 0)]
        public int CompressionRatio { get; set; } = 10;

        public bool StreamFromDisk { get; set; }

        public bool Spatialized { get; set; }  
    }
}
