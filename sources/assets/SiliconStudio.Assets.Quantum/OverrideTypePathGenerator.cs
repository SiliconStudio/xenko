using System;
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public class OverrideTypePathGenerator : GraphVisitorBase
    {
        public Dictionary<ObjectPath, OverrideType> Result { get; } = new Dictionary<ObjectPath, OverrideType>();

        public void Reset()
        {
            Result.Clear();
        }

        protected override void VisitNode(IGraphNode node, GraphNodePath currentPath)
        {
            var assetNode = (AssetNode)node;

            var path = ConvertPath(currentPath);
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
        }

        private static ObjectPath ConvertPath(GraphNodePath path)
        {
            var currentPath = new GraphNodePath(path.RootNode);
            var result = new ObjectPath();
            foreach (var item in path.Path)
            {
                switch (item.Type)
                {
                    case GraphNodePath.ElementType.Member:
                        var member = (string)item.Value;
                        result.PushMember(member);
                        currentPath = currentPath.PushMember(member);
                        break;
                    case GraphNodePath.ElementType.Target:
                        currentPath = currentPath.PushTarget();
                        break;
                    case GraphNodePath.ElementType.Index:
                        var index = (Index)item.Value;
                        var node = (AssetNode)currentPath.GetNode();
                        var id = node.IndexToId(index);
                        // Create a new id if we don't have any so far
                        if (id == ItemId.Empty)
                            id = ItemId.New();
                        result.PushItemId(id);
                        // TODO: support non-identifiable items collections.
                        currentPath = currentPath.PushIndex(index);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return result;
        }
    }
}