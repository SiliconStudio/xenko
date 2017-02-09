using System;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public abstract class AssetNodeMetadataCollector : GraphVisitorBase
    {
        private int inNonIdentifiableType;

        protected override void VisitNode(IGraphNode node, GraphNodePath currentPath)
        {
            var assetNode = (IAssetNode)node;

            var localInNonIdentifiableType = false;
            if ((node.Descriptor as ObjectDescriptor)?.Attributes.OfType<NonIdentifiableCollectionItemsAttribute>().Any() ?? false)
            {
                localInNonIdentifiableType = true;
                inNonIdentifiableType++;
            }

            var path = ConvertPath(currentPath, inNonIdentifiableType);
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
            base.VisitNode(node, currentPath);

            if (localInNonIdentifiableType)
                inNonIdentifiableType--;
        }

        protected abstract void VisitMemberNode(IAssetMemberNode memberNode, YamlAssetPath currentPath);

        protected abstract void VisitObjectNode(IAssetObjectNode objectNode, YamlAssetPath currentPath);

        public static YamlAssetPath ConvertPath(GraphNodePath path, int inNonIdentifiableType)
        {
            var currentNode = (IAssetNode)path.RootNode;
            var result = new YamlAssetPath();
            var i = 0;
            foreach (var item in path.Path)
            {
                switch (item.Type)
                {
                    case GraphNodePath.ElementType.Member:
                    {
                        var member = (string)item.Value;
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
                        var index = (Index)item.Value;
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