using SharpYaml.Serialization;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets
{
    public abstract class AssetUpgraderBase : IAssetUpgrader
    {
        public void Upgrade(ILogger log, YamlMappingNode yamlAssetNode)
        {
            dynamic asset = new DynamicYamlMapping(yamlAssetNode);

            // upgrade the asset
            UpgradeAsset(log, asset);

            // upgrade its base
            var baseBranch = asset["~Base"];
            if (baseBranch != null)
            {
                var baseAsset = baseBranch["Asset"];
                if (baseAsset != null)
                    UpgradeAsset(log, baseAsset);
            }
        }

        protected abstract void UpgradeAsset(ILogger log, dynamic asset);

        protected void SetSerializableVersion(dynamic asset, int value)
        {
            asset.SerializedVersion = value;
            // Ensure that it is stored right after the asset Id
            asset.MoveChild("SerializedVersion", asset.IndexOf("Id") + 1);
        }
    }
}