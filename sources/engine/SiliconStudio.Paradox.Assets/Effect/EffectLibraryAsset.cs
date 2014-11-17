// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// Describes an effect asset. 
    /// </summary>
    [DataContract("EffectLibrary")]
    [AssetFileExtension(FileExtension)]
    [AssetCompiler(typeof(EffectLibraryAssetCompiler))]
    [AssetDescription("Effect Library", "An effect library", false)]
    public sealed class EffectLibraryAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="EffectLibraryAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxfxlib";

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectLibraryAsset"/> class.
        /// </summary>
        public EffectLibraryAsset()
        {
            BuildOrder = 1000;
            Permutations = new EffectPermutation();
        }

        /// <summary>
        /// Gets the root definition for permutations
        /// </summary>
        /// <value>The the root definition for permutations.</value>
        /// <userdoc>
        /// The permutations of parameters used to generate the effects. Behind each ParameterKey, you can assign a value, a list of values or a range of values (if numeric).
        /// </userdoc>
        [DataMember(10)]
        public EffectPermutation Permutations { get; set; }
    }
}