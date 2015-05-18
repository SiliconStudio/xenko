using SharpYaml.Serialization;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets
{
    public abstract class AssetUpgraderBase : IAssetUpgrader
    {
        public void Upgrade(int currentVersion, int targetVersion, ILogger log, YamlMappingNode yamlAssetNode)
        {
            dynamic asset = new DynamicYamlMapping(yamlAssetNode);

            // upgrade the asset
            UpgradeAsset(currentVersion, targetVersion, log, asset);
            SetSerializableVersion(asset, targetVersion);
            // upgrade its base
            var baseBranch = asset["~Base"];
            if (baseBranch != null)
            {
                var baseAsset = baseBranch["Asset"];
                if (baseAsset != null)
                    UpgradeAsset(currentVersion, targetVersion, log, baseAsset);
                SetSerializableVersion(baseAsset, targetVersion);
            }
        }

        protected abstract void UpgradeAsset(int currentVersion, int targetVersion, ILogger log, dynamic asset);

        private static void SetSerializableVersion(dynamic asset, int value)
        {
            asset.SerializedVersion = value;
            // Ensure that it is stored right after the asset Id
            asset.MoveChild("SerializedVersion", asset.IndexOf("Id") + 1);
        }
    }
}