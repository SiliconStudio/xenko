using System;
using SiliconStudio.Assets.Yaml;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Visitors
{
    /// <summary>
    /// An implementation of <see cref="AssetNodeMetadataCollectorBase"/> that generates the path to all object references in the given asset.
    /// </summary>
    public class ObjectReferencePathGenerator : AssetNodeMetadataCollectorBase
    {
        private readonly AssetPropertyGraph propertyGraph;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectReferencePathGenerator"/> class.
        /// </summary>
        /// <param name="propertyGraph">The <see cref="AssetPropertyGraph"/> used to analyze object references.</param>
        public ObjectReferencePathGenerator(AssetPropertyGraph propertyGraph)
        {
            this.propertyGraph = propertyGraph;
        }

        /// <summary>
        /// Gets the resulting metadata that can be passed to YAML serialization.
        /// </summary>
        public YamlAssetMetadata<Guid> Result { get; } = new YamlAssetMetadata<Guid>();

        /// <summary>
        /// Gets or sets a method that indicates if a given identifier should be output to the list of object references.
        /// </summary>
        public Func<Guid, bool> ShouldOutputReference { get; set; }

        /// <inheritdoc/>
        protected override void VisitMemberNode(IAssetMemberNode memberNode, YamlAssetPath currentPath)
        {
            if (propertyGraph.IsObjectReference(memberNode, Index.Empty))
            {
                var value = memberNode.Retrieve();
                if (value == null)
                    return;

                var identifiable = value as IIdentifiable;
                if (identifiable == null)
                    throw new InvalidOperationException("IsObjectReference returned true for an object that is not IIdentifiable");

                var id = identifiable.Id;
                if (ShouldOutputReference?.Invoke(id) ?? true)
                    Result.Set(currentPath, id);
            }
        }

        /// <inheritdoc/>
        protected override void VisitObjectNode(IAssetObjectNode objectNode, YamlAssetPath currentPath)
        {
            if (!objectNode.IsReference)
                return;

            foreach (var index in ((IAssetObjectNodeInternal)objectNode).Indices)
            {
                if (!propertyGraph.IsObjectReference(objectNode, index))
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
                if (ShouldOutputReference?.Invoke(id) ?? true)
                    Result.Set(itemPath, id);
            }
        }
    }
}
