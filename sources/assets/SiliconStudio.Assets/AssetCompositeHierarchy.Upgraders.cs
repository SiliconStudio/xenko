// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets
{
    partial class AssetCompositeHierarchy<TAssetPartDesign, TAssetPart>
    {
        protected class RootPartIdsToRootPartsUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                var rootPartIds = asset.Hierarchy.RootPartIds;
                int i = 0;
                foreach (dynamic rootPartId in rootPartIds)
                {
                    rootPartIds[i++] = "ref!! " + rootPartId.ToString();
                }
                asset.Hierarchy.RootParts = rootPartIds;
                asset.Hierarchy.RootPartIds = DynamicYamlEmpty.Default;
            }
        }
    }
}
