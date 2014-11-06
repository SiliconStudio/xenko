using SharpYaml.Serialization;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets
{
    public interface IAssetUpgrader
    {
        void Upgrade(ILogger log, YamlMappingNode yamlAssetNode);
    }
}