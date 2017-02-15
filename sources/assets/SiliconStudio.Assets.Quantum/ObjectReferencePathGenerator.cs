using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Yaml;
using SiliconStudio.Core;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Quantum
{
    public class ObjectReferencePathGenerator : AssetNodeMetadataCollector
    {
        public YamlAssetMetadata<Guid> Result { get; } = new YamlAssetMetadata<Guid>();

        protected override void VisitMemberNode(IAssetMemberNode memberNode, YamlAssetPath currentPath)
        {
            if (((AssetMemberNode)memberNode).IsObjectReference)
            {
                var id = ((IIdentifiable)memberNode.Retrieve()).Id;
                Result.Set(currentPath, id);
            }
        }

        protected override void VisitObjectNode(IAssetObjectNode objectNode, YamlAssetPath currentPath)
        {
            foreach (var index in ((IAssetObjectNodeInternal)objectNode).GetObjectReferenceIndices())
            {
                var itemId = objectNode.IndexToId(index);
                var itemPath = currentPath.Clone();
                itemPath.PushItemId(itemId);
                var id = ((IIdentifiable)objectNode.Retrieve(index)).Id;
                Result.Set(itemPath, id);
            }
        }
    }
}
