// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Assets;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Models
{
    public class AnimationAssetUpgraderFramerate : AssetUpgraderBase
    {
        /// <inheritdoc/>
        protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
        {
            asset.AnimationTimeMinimum = "0:00:00:00.0000000";
            asset.AnimationTimeMaximum = "0:00:30:00.0000000";  // Tentatively set this to 30 minutes
        }
    }
}
