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
            var currentNode = (AssetNode)path.RootNode;
            var result = new ObjectPath();
            var i = 0;
            foreach (var item in path.Path)
            {
                switch (item.Type)
                {
                    case GraphNodePath.ElementType.Member:
                        var member = (string)item.Value;
                        result.PushMember(member);
                        currentPath = currentPath.PushMember(member);
                        currentNode = (AssetNode)currentNode.GetChild(member);
                        break;
                    case GraphNodePath.ElementType.Target:
                        currentPath = currentPath.PushTarget();
                        if (i < path.Path.Count - 1)
                        {
                            currentNode = (AssetNode)currentNode.GetTarget();
                        }
                        break;
                    case GraphNodePath.ElementType.Index:
                        var index = (Index)item.Value;
                        if (AssetNode.IsNonIdentifiableCollectionContent(currentNode.Content))
                        {
                            result.PushIndex(index);
                        }
                        else
                        {
                            try
                            {
                                var id = currentNode.IndexToId(index);
                                // Create a new id if we don't have any so far
                                if (id == ItemId.Empty)
                                    id = ItemId.New();
                                result.PushItemId(id);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        currentPath = currentPath.PushIndex(index);
                        if (i < path.Path.Count - 1)
                        {
                            currentNode = (AssetNode)currentNode.GetTarget(index);
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