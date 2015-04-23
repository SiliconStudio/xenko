// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Assets;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// Describes a shader effect asset (pdxsl).
    /// </summary>
    [DataContract("EffectCompositorAsset")]
    [AssetDescription(FileExtension, false)]
    [Display(95, "Effect Compositor", "An effect compositor")]
    public sealed class EffectCompositorAsset : SourceCodeAsset
    {
        /// <summary>
        /// The default file extension used by the <see cref="EffectLibraryAsset"/>.
        /// </summary>
        public const string FileExtension = ".pdxfx";

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectLibraryAsset"/> class.
        /// </summary>
        public EffectCompositorAsset()
        {
        }
    }
}