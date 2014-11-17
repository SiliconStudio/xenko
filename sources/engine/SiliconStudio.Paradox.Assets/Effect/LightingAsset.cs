// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// Describes an lighting configuration asset. 
    /// </summary>
    [DataContract("LightingConfiguration")]
    [AssetFileExtension(FileExtension)]
    [AssetCompiler(typeof(LightingAssetCompiler))]
    [AssetDescription("Lighting Configuration", "A lighting configuration", false)]
    public sealed class LightingAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="EffectLibraryAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxlightconf";

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectLibraryAsset"/> class.
        /// </summary>
        public LightingAsset()
        {
            BuildOrder = 1000;
            Permutations = new EffectPermutation();
        }

        /// <summary>
        /// Gets the root definition for permutations
        /// </summary>
        /// <value>The the root definition for permutations.</value>
        /// <userdoc>
        /// The supported lighting configurations (number of lights and shadow maps) written as permutations. Non-lighting related keys will be ignored.
        /// </userdoc>
        [DataMember(10)]
        public EffectPermutation Permutations { get; set; }
    }
}
