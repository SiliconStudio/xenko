using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Quantum
{
    public class OverrideTypePathGenerator : AssetNodeMetadataCollector
    {
        public Dictionary<YamlAssetPath, OverrideType> Result { get; } = new Dictionary<YamlAssetPath, OverrideType>();

        public void Reset()
        {
            Result.Clear();
        }

        protected override void VisitMemberNode(IAssetMemberNode memberNode, YamlAssetPath currentPath)
        {
            if (memberNode?.IsContentOverridden() == true)
            {
                Result.Add(currentPath, memberNode.GetContentOverride());
            }
        }

        protected override void VisitObjectNode(IAssetObjectNode objectNode, YamlAssetPath currentPath)
        {
            foreach (var index in objectNode.GetOverriddenItemIndices())
            {
                var id = objectNode.IndexToId(index);
                var itemPath = currentPath.Clone();
                itemPath.PushItemId(id);
                Result.Add(itemPath, objectNode.GetItemOverride(index));
            }
            foreach (var index in objectNode.GetOverriddenKeyIndices())
            {
                var id = objectNode.IndexToId(index);
                var itemPath = currentPath.Clone();
                itemPath.PushIndex(id);
                Result.Add(itemPath, objectNode.GetKeyOverride(index));
            }
        }
    }
}
