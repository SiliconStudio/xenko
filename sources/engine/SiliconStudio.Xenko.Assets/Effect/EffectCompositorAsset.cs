// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Assets;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Effect
{
    /// <summary>
    /// Describes a shader effect asset (xksl).
    /// </summary>
    [DataContract("EffectCompositorAsset")]
    [AssetDescription(FileExtension, AlwaysMarkAsRoot = true, AllowArchetype = false)]
    [Display(95, "Effect Compositor")]
    public sealed class EffectCompositorAsset : ProjectCodeGeneratorAsset
    {
        /// <summary>
        /// The default file extension used by the <see cref="EffectCompositorAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkfx;.pdxfx";

        public override string Generator { get; set; } = "XenkoShaderKeyGenerator";
    }
}
