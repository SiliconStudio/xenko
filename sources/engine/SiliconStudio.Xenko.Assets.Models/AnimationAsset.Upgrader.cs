// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;

namespace SiliconStudio.Xenko.Assets.Models
{
    public class AnimationAssetUpgraderFramerate : AssetUpgraderBase
    {
        /// <inheritdoc/>
        protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
        {
            asset.StartAnimationFrame   = "0:00:00:00.0000000";
            asset.EndAnimationFrame     = "0:00:30:00.0000000";
            asset.AnimationFrameMinimum = "0:00:00:00.0000000";
            asset.AnimationFrameMaximum = "0:00:30:00.0000000";
        }
    }
}
