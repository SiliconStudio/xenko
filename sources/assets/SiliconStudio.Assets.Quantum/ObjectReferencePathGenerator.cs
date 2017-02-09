using System.Collections.Generic;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Quantum
{
    public class ObjectReferencePathGenerator : AssetNodeMetadataCollector
    {
        public HashSet<YamlAssetPath> Result { get; } = new HashSet<YamlAssetPath>();

        public void Reset()
        {
            Result.Clear();
        }

        protected override void VisitMemberNode(IAssetMemberNode memberNode, YamlAssetPath currentPath)
        {
            if (((AssetMemberNode)memberNode).IsObjectReference)
            {
                Result.Add(currentPath);
            }
        }

        protected override void VisitObjectNode(IAssetObjectNode objectNode, YamlAssetPath currentPath)
        {
            foreach (var index in ((AssetObjectNode)objectNode).GetObjectReferenceIndices())
            {
                var id = objectNode.IndexToId(index);
                var itemPath = currentPath.Clone();
                itemPath.PushItemId(id);
                Result.Add(itemPath);
            }
        }
    }
}