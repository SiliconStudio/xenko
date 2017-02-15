using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Yaml;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public class ObjectReferencePathGenerator : AssetNodeMetadataCollector
    {
        private readonly AssetPropertyGraph propertyGraph;

        public ObjectReferencePathGenerator(AssetPropertyGraph propertyGraph)
        {
            this.propertyGraph = propertyGraph;
        }

        public YamlAssetMetadata<Guid> Result { get; } = new YamlAssetMetadata<Guid>();

        protected override void VisitMemberNode(IAssetMemberNode memberNode, YamlAssetPath currentPath)
        {
            if (propertyGraph.IsObjectReference(memberNode, Index.Empty, memberNode.Retrieve()))
            {
                var value = memberNode.Retrieve() as IIdentifiable;
                if (value == null)
                    throw new InvalidOperationException("IsObjectReference returned true for an object that is not IIdentifiable");
                var id = value.Id;
                Result.Set(currentPath, id);
            }
        }

        protected override void VisitObjectNode(IAssetObjectNode objectNode, YamlAssetPath currentPath)
        {
            if (!objectNode.IsReference)
                return;

            foreach (var index in ((IAssetObjectNodeInternal)objectNode).Indices)
            {
                if (!propertyGraph.IsObjectReference(objectNode, index, objectNode.Retrieve(index)))
                    continue;

                var itemPath = currentPath.Clone();
                if (CollectionItemIdHelper.HasCollectionItemIds(objectNode.Retrieve()))
                {
                    var itemId = objectNode.IndexToId(index);
                    itemPath.PushItemId(itemId);
                }
                else
                {
                    itemPath.PushIndex(index);
                }
                var value = objectNode.Retrieve(index) as IIdentifiable;
                if (value == null)
                    throw new InvalidOperationException("IsObjectReference returned true for an object that is not IIdentifiable");
                var id = value.Id;
                Result.Set(itemPath, id);
            }
        }
    }
}
