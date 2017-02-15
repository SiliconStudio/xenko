using System.Collections.Generic;
using SiliconStudio.Assets.Yaml;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Quantum
{
    public class OverrideTypePathGenerator : AssetNodeMetadataCollector
    {
        public YamlAssetMetadata<OverrideType> Result { get; } = new YamlAssetMetadata<OverrideType>();

        protected override void VisitMemberNode(IAssetMemberNode memberNode, YamlAssetPath currentPath)
        {
            if (memberNode?.IsContentOverridden() == true)
            {
                Result.Set(currentPath, memberNode.GetContentOverride());
            }
        }

        protected override void VisitObjectNode(IAssetObjectNode objectNode, YamlAssetPath currentPath)
        {
            foreach (var index in objectNode.GetOverriddenItemIndices())
            {
                var id = objectNode.IndexToId(index);
                var itemPath = currentPath.Clone();
                itemPath.PushItemId(id);
                Result.Set(itemPath, objectNode.GetItemOverride(index));
            }
            foreach (var index in objectNode.GetOverriddenKeyIndices())
            {
                var id = objectNode.IndexToId(index);
                var itemPath = currentPath.Clone();
                itemPath.PushIndex(id);
                Result.Set(itemPath, objectNode.GetKeyOverride(index));
            }
        }
    }
}
