// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Effect
{
    /// <summary>
    /// Describes a shader effect asset (xksl).
    /// </summary>
    [DataContract("EffectShader")]
    [AssetDescription(FileExtension, AlwaysMarkAsRoot = true)]
    [AssetCompiler(typeof(EffectShaderAssetCompiler))]
    [Display(90, "Effect Shader")]
    public sealed class EffectShaderAsset : ProjectSourceCodeAsset
    {
        /// <summary>
        /// The default file extension used by the <see cref="EffectLibraryAsset"/>.
        /// </summary>
        public const string FileExtension = ".xksl;.pdxsl";

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectLibraryAsset"/> class.
        /// </summary>
        public EffectShaderAsset()
        {
        }
    }
}