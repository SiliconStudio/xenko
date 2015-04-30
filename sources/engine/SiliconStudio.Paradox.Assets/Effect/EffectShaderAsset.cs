// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// Describes a shader effect asset (pdxsl).
    /// </summary>
    [DataContract("EffectShader")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(EffectShaderAssetCompiler))]
    [Display(90, "Effect Shader", "An effect shader")]
    public sealed class EffectShaderAsset : SourceCodeAsset
    {
        /// <summary>
        /// The default file extension used by the <see cref="EffectLibraryAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxsl";

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectLibraryAsset"/> class.
        /// </summary>
        public EffectShaderAsset()
        {
        }
    }
}