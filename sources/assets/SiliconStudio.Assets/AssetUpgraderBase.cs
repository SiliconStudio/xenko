// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SharpYaml.Serialization;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets
{
    public abstract class AssetUpgraderBase : IAssetUpgrader
    {
        public void Upgrade(AssetMigrationContext context, int currentVersion, int targetVersion, YamlMappingNode yamlAssetNode, PackageLoadingAssetFile assetFile)
        {
            dynamic asset = new DynamicYamlMapping(yamlAssetNode);

            // upgrade the asset
            UpgradeAsset(context, currentVersion, targetVersion, asset, assetFile);
            SetSerializableVersion(asset, targetVersion);
            // upgrade its base
            var baseBranch = asset["~Base"];
            if (baseBranch != null)
            {
                var baseAsset = baseBranch["Asset"];
                if (baseAsset != null)
                    UpgradeAsset(context, currentVersion, targetVersion, baseAsset, assetFile);
                SetSerializableVersion(baseAsset, targetVersion);
            }
        }

        protected abstract void UpgradeAsset(AssetMigrationContext context, int currentVersion, int targetVersion, dynamic asset, PackageLoadingAssetFile assetFile);

        private static void SetSerializableVersion(dynamic asset, int value)
        {
            asset.SerializedVersion = value;
            // Ensure that it is stored right after the asset Id
            asset.MoveChild("SerializedVersion", asset.IndexOf("Id") + 1);
        }
    }
}