// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Effect
{
    /// <summary>
    /// Describes an effect asset. 
    /// </summary>
    [DataContract("EffectLibrary")]
    [AssetDescription(FileExtension, AlwaysMarkAsRoot = true, AllowArchetype = false)]
    [AssetCompiler(typeof(EffectLogAssetCompiler))]
    [Display(98, "Effect Library")]
    public sealed class EffectLogAsset : SourceCodeAsset
    {
        /// <summary>
        /// The default file name used to store effect compile logs.
        /// </summary>
        public const string DefaultFile = "EffectCompileLog";

        /// <summary>
        /// The default file extension used by the <see cref="EffectLogAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkeffectlog;.pdxeffectlog";

        protected override int InternalBuildOrder => 100;
    }
}
