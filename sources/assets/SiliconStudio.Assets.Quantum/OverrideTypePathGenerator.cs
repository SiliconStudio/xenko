using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public class OverrideTypePathGenerator : GraphVisitorBase
    {
        public Dictionary<YamlAssetPath, OverrideType> Result { get; } = new Dictionary<YamlAssetPath, OverrideType>();
        private int inNonIdentifiableType;

        public void Reset()
        {
            Result.Clear();
        }

        protected override void VisitNode(IContentNode node, GraphNodePath currentPath)
        {
            var assetNode = (IAssetNode)node;

            bool localInNonIdentifiableType = false;
            if ((node.Descriptor as ObjectDescriptor)?.Attributes.OfType<NonIdentifiableCollectionItemsAttribute>().Any() ?? false)
            {
                localInNonIdentifiableType = true;
                inNonIdentifiableType++;
            }

            var path = ConvertPath(currentPath, inNonIdentifiableType);
            var memberNode = assetNode as AssetMemberNode;
            if (memberNode?.IsContentOverridden() == true)
            {
                Result.Add(path, memberNode.GetContentOverride());
            }
            var objectNode = assetNode as IAssetObjectNode;
            if (objectNode != null)
            {
                foreach (var index in objectNode.GetOverriddenItemIndices())
                {
                    var id = objectNode.IndexToId(index);
                    var itemPath = path.Clone();
                    itemPath.PushItemId(id);
                    Result.Add(itemPath, objectNode.GetItemOverride(index));
                }
                foreach (var index in objectNode.GetOverriddenKeyIndices())
                {
                    var id = objectNode.IndexToId(index);
                    var itemPath = path.Clone();
                    itemPath.PushIndex(id);
                    Result.Add(itemPath, objectNode.GetKeyOverride(index));
                }
            }
            base.VisitNode(node, currentPath);

            if (localInNonIdentifiableType)
                inNonIdentifiableType--;
        }

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
