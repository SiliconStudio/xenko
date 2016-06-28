// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Audio
{
    [DataContract("Sound")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(SoundAssetCompiler))]
    [Display(120, "Sound")]
    public class SoundAsset : AssetWithSource
    {
        /// <summary>
        /// The default file extension used by the <see cref="SoundAsset"/>.
        /// </summary>
        public const string FileExtension = ".xksnd;.pdxsnd";

        [DefaultValue(44100)]
        public int SampleRate { get; set; } = 44100;

        [DefaultValue(10)]
        public int CompressionRatio { get; set; } = 10;

        public bool StreamFromDisk { get; set; }

        public bool Spatialized { get; set; }  
    }
}
