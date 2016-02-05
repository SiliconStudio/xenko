// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SharpYaml.Serialization;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets
{
    public abstract class AssetUpgraderBase : IAssetUpgrader
    {
        public void Upgrade(AssetMigrationContext context, string dependencyName, PackageVersion currentVersion, PackageVersion targetVersion, YamlMappingNode yamlAssetNode, PackageLoadingAssetFile assetFile)
        {
            dynamic asset = new DynamicYamlMapping(yamlAssetNode);

            // upgrade the asset
            UpgradeAsset(context, currentVersion, targetVersion, asset, assetFile);
            SetSerializableVersion(asset, dependencyName, targetVersion);

            // upgrade its base
            var baseBranch = asset[Asset.BaseProperty];
            if (baseBranch != null)
            {
                UpgradeBase(context, dependencyName, currentVersion, targetVersion, baseBranch, assetFile);
            }

            // upgrade base parts
            var basePartsBranch = asset[Asset.BasePartsProperty] as DynamicYamlArray;
            if (basePartsBranch != null)
            {
                foreach (dynamic assetBase in basePartsBranch)
                {
                    UpgradeBase(context, dependencyName, currentVersion, targetVersion, assetBase, assetFile);
                }
            }
        }

        private void UpgradeBase(AssetMigrationContext context, string dependencyName, PackageVersion currentVersion, PackageVersion targetVersion, dynamic assetBase, PackageLoadingAssetFile assetFile)
        {
            var baseAsset = assetBase[nameof(AssetBase.Asset)];
            if (baseAsset != null)
            {
                UpgradeAsset(context, currentVersion, targetVersion, baseAsset, assetFile);
                SetSerializableVersion(baseAsset, dependencyName, targetVersion);
            }
        }

        protected abstract void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile);

        public static void SetSerializableVersion(dynamic asset, string dependencyName, PackageVersion value)
        {
            if (asset.IndexOf(nameof(Asset.SerializedVersion)) == -1)
            {
                asset.SerializedVersion = new YamlMappingNode();

                // Ensure that it is stored right after the asset Id
                asset.MoveChild(nameof(Asset.SerializedVersion), asset.IndexOf(nameof(Asset.Id)) + 1);
            }

            asset.SerializedVersion[dependencyName] = value;
        }
    }
}