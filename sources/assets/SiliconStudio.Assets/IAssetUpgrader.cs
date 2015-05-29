using SharpYaml.Serialization;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets
{
    public interface IAssetUpgrader
    {
        void Upgrade(int currentVersion, int targetVersion, ILogger log, YamlMappingNode yamlAssetNode);
    }
}