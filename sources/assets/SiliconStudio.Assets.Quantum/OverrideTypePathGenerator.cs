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

        protected override void VisitNode(IGraphNode node, GraphNodePath currentPath)
        {
            var assetNode = (AssetNode)node;

            bool localInNonIdentifiableType = false;
            if ((node.Content.Descriptor as ObjectDescriptor)?.Attributes.OfType<NonIdentifiableCollectionItemsAttribute>().Any() ?? false)
            {
                localInNonIdentifiableType = true;
                inNonIdentifiableType++;
            }

            var path = ConvertPath(currentPath, inNonIdentifiableType);
            if (assetNode.IsContentOverridden())
            {
                Result.Add(path, assetNode.GetContentOverride());
            }

            foreach (var index in assetNode.GetOverriddenItemIndices())
            {
                var id = assetNode.IndexToId(index);
                var itemPath = path.Clone();
                itemPath.PushItemId(id);
                Result.Add(itemPath, assetNode.GetItemOverride(index));
            }
            foreach (var index in assetNode.GetOverriddenKeyIndices())
            {
                var id = assetNode.IndexToId(index);
                var itemPath = path.Clone();
                itemPath.PushIndex(id);
                Result.Add(itemPath, assetNode.GetKeyOverride(index));
            }
            base.VisitNode(node, currentPath);

            if (localInNonIdentifiableType)
                inNonIdentifiableType--;
        }

        public static YamlAssetPath ConvertPath(GraphNodePath path, int inNonIdentifiableType)
        {
            var currentNode = (AssetNode)path.RootNode;
            var result = new YamlAssetPath();
            var i = 0;
            foreach (var item in path.Path)
            {
                switch (item.Type)
                {
                    case GraphNodePath.ElementType.Member:
                        var member = (string)item.Value;
                        result.PushMember(member);
                        currentNode = (AssetNode)((IGraphNode)currentNode).TryGetChild(member);
                        break;
                    case GraphNodePath.ElementType.Target:
                        if (i < path.Path.Count - 1)
                        {
                            currentNode = (AssetNode)((IGraphNode)currentNode).Target;
                        }
                        break;
                    case GraphNodePath.ElementType.Index:
                        var index = (Index)item.Value;
                        if (inNonIdentifiableType > 0 || currentNode.IsNonIdentifiableCollectionContent)
                        {
                            result.PushIndex(index.Value);
                        }
                        else
                        {
                            var id = currentNode.IndexToId(index);
                            // Create a new id if we don't have any so far
                            if (id == ItemId.Empty)
                                id = ItemId.New();
                            result.PushItemId(id);
                        }
                        if (i < path.Path.Count - 1)
                        {
                            currentNode = (AssetNode)((IGraphNode)currentNode).IndexedTarget(index);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                ++i;
            }
            return result;
        }
    }
}
