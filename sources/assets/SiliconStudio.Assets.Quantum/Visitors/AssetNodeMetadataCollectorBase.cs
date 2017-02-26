using System;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Visitors
{
    /// <summary>
    /// A visitor that collects metadata to pass to YAML serialization.
    /// </summary>
    public abstract class AssetNodeMetadataCollectorBase : GraphVisitorBase
    {
        private int inNonIdentifiableType;

        /// <inheritdoc/>
        protected override void VisitNode(IGraphNode node)
        {
            var assetNode = (IAssetNode)node;

            var localInNonIdentifiableType = false;
            if ((node.Descriptor as ObjectDescriptor)?.Attributes.OfType<NonIdentifiableCollectionItemsAttribute>().Any() ?? false)
            {
                localInNonIdentifiableType = true;
                inNonIdentifiableType++;
            }

            var path = ConvertPath(CurrentPath, inNonIdentifiableType);
            var memberNode = assetNode as IAssetMemberNode;
            if (memberNode != null)
            {
                VisitMemberNode(memberNode, path);
            }
            var objectNode = assetNode as IAssetObjectNode;
            if (objectNode != null)
            {
                VisitObjectNode(objectNode, path);
            }
            base.VisitNode(node);

            if (localInNonIdentifiableType)
                inNonIdentifiableType--;
        }

        /// <summary>
        /// Visits a node that is an <see cref="IAssetMemberNode"/>.
        /// </summary>
        /// <param name="memberNode">The node to visit.</param>
        /// <param name="currentPath">The current path in the visit.</param>
        protected abstract void VisitMemberNode(IAssetMemberNode memberNode, YamlAssetPath currentPath);

        /// <summary>
        /// Visits a node that is an <see cref="IAssetObjectNode"/>.
        /// </summary>
        /// <param name="objectNode">The node to visit.</param>
        /// <param name="currentPath">The current path in the visit.</param>
        protected abstract void VisitObjectNode(IAssetObjectNode objectNode, YamlAssetPath currentPath);

        /// <summary>
        /// Converts the given <see cref="GraphNodePath"/> to a <see cref="YamlAssetPath"/> that can be processed by YAML serialization.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <param name="inNonIdentifiableType">If greater than zero, will ignore collection item ids and write indices instead.</param>
        /// <returns>An instance of <see cref="YamlAssetPath"/> corresponding to the given <paramref name="path"/>.</returns>
        [NotNull]
        public static YamlAssetPath ConvertPath([NotNull] GraphNodePath path, int inNonIdentifiableType = 0)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            var currentNode = (IAssetNode)path.RootNode;
            var result = new YamlAssetPath();
            var i = 0;
            foreach (var item in path.Path)
            {
                switch (item.Type)
                {
                    case GraphNodePath.ElementType.Member:
                    {
                        var member = item.Name;
                        result.PushMember(member);
                        var objectNode = currentNode as IObjectNode;
                        if (objectNode == null) throw new InvalidOperationException($"An IObjectNode was expected when processing the path [{path}]");
                        currentNode = (IAssetNode)objectNode.TryGetChild(member);
                        break;
                    }
                    case GraphNodePath.ElementType.Target:
                    {
                        if (i < path.Path.Count - 1)
                        {
                            var targetingMemberNode = currentNode as IMemberNode;
                            if (targetingMemberNode == null) throw new InvalidOperationException($"An IMemberNode was expected when processing the path [{path}]");
                            currentNode = (IAssetNode)targetingMemberNode.Target;
                        }
                        break;
                    }
                    case GraphNodePath.ElementType.Index:
                    {
                        var index = item.Index;
                        var objectNode = currentNode as AssetObjectNode;
                        if (objectNode == null) throw new InvalidOperationException($"An IObjectNode was expected when processing the path [{path}]");
                        if (inNonIdentifiableType > 0 || !CollectionItemIdHelper.HasCollectionItemIds(objectNode.Retrieve()))
                        {
                            result.PushIndex(index.Value);
                        }
                        else
                        {
                            var id = objectNode.IndexToId(index);
                            // Create a new id if we don't have any so far
                            if (id == ItemId.Empty)
                                id = ItemId.New();
                            result.PushItemId(id);
                        }
                        if (i < path.Path.Count - 1)
                        {
                            currentNode = (IAssetNode)objectNode.IndexedTarget(index);
                        }
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                ++i;
            }
            return result;
        }
    }
}
