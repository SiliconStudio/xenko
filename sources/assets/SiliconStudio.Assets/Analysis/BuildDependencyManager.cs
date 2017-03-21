using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Compiler;

namespace SiliconStudio.Assets.Analysis
{
    public sealed class BuildDependencyManager
    {
        public static readonly AssetCompilerRegistry AssetCompilerRegistry = new AssetCompilerRegistry();

        private readonly ConcurrentDictionary<BuildNodeDesc, BuildAssetNode> nodes = new ConcurrentDictionary<BuildNodeDesc, BuildAssetNode>();

        public Type CompilationContext { get; set; } = typeof(AssetCompilationContext);

        public BuildAssetNode FindOrCreateNode(AssetItem item, BuildDependencyType dependencyType)
        {
            var nodeDesc = new BuildNodeDesc
            {
                AssetId = item.Id,
                BuildDependencyType = dependencyType
            };

            BuildAssetNode node;
            if (!nodes.TryGetValue(nodeDesc, out node))
            {
                node = new BuildAssetNode(item, dependencyType, this);
                nodes.TryAdd(nodeDesc, node);
            }
            
            return node;
        }

        public BuildAssetNode FindNode(AssetItem item, BuildDependencyType dependencyType)
        {
            var nodeDesc = new BuildNodeDesc
            {
                AssetId = item.Id,
                BuildDependencyType = dependencyType
            };

            BuildAssetNode node;
            if (!nodes.TryGetValue(nodeDesc, out node))
            {
                return null;
            }

            return node;
        }

        public IEnumerable<BuildAssetNode> FindNodes(AssetItem item)
        {
            return nodes.Where(x => x.Value.AssetItem == item).Select(x => x.Value);
        }

        public void RemoveNode(BuildAssetNode node)
        {
            var nodeDesc = new BuildNodeDesc
            {
                AssetId = node.AssetItem.Id,
                BuildDependencyType = node.DependencyType
            };

            nodes.TryRemove(nodeDesc, out node);
        }
    }
}