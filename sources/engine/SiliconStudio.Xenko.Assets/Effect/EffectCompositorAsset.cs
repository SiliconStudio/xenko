// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Assets;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Effect
{
    /// <summary>
    /// Describes a shader effect asset (xksl).
    /// </summary>
    [DataContract("EffectCompositorAsset")]
    [AssetDescription(FileExtension, AlwaysMarkAsRoot = true, AllowArchetype = false)]
    public sealed partial class EffectCompositorAsset : ProjectSourceCodeWithFileGeneratorAsset
    {
        /// <summary>
        /// The default file extension used by the <see cref="EffectCompositorAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkfx";

        public override string Generator => "XenkoShaderKeyGenerator";

        public override void SaveGeneratedAsset(AssetItem assetItem)
        {
            // TODO: Implement this?
        }
    }
}
