using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Compiler;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Build dependency manager
    /// Basically is a container of BuildAssetNode
    /// </summary>
    public sealed class BuildDependencyManager
    {
        public struct BuildNodeDesc
        {
            public AssetId AssetId;
            public BuildDependencyType BuildDependencyType;

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                var other = (BuildNodeDesc)obj;
                return AssetId == other.AssetId && BuildDependencyType == other.BuildDependencyType;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = (int)2166136261;
                    hash = (hash * 16777619) ^ AssetId.GetHashCode();
                    hash = (hash * 16777619) ^ BuildDependencyType.GetHashCode();
                    return hash;
                }
            }

            public static bool operator ==(BuildNodeDesc x, BuildNodeDesc y)
            {
                return x.AssetId == y.AssetId && x.BuildDependencyType == y.BuildDependencyType;
            }

            public static bool operator !=(BuildNodeDesc x, BuildNodeDesc y)
            {
                return x.AssetId != y.AssetId || x.BuildDependencyType != y.BuildDependencyType;
            }
        }

        /// <summary>
        /// The AssetCompilerRegistry, here mostly for ease of access
        /// </summary>
        public static readonly AssetCompilerRegistry AssetCompilerRegistry = new AssetCompilerRegistry();

        private readonly ConcurrentDictionary<BuildNodeDesc, BuildAssetNode> nodes = new ConcurrentDictionary<BuildNodeDesc, BuildAssetNode>();

        /// <summary>
        /// The context of the build itself, nodes might differ between contexts
        /// </summary>
        public Type CompilationContext { get; }

        public BuildDependencyManager(Type compilationContext)
        {
            CompilationContext = compilationContext;
        }

        /// <summary>
        /// Finds or creates a node, notice that this will not perform an analysis on the node, which must be explicitly called on the node
        /// </summary>
        /// <param name="item">The asset item to find or create</param>
        /// <param name="dependencyType">The type of dependency</param>
        /// <returns>The build node associated with item</returns>
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
            else if (!ReferenceEquals(node.AssetItem, item))
            {               
                node = new BuildAssetNode(item, dependencyType, this);
                nodes[nodeDesc] = node;
            }

            return node;
        }

        /// <summary>
        /// Finds a node, notice that this will not perform an analysis on the node, which must be explicitly called on the node
        /// </summary>
        /// <param name="item">The asset item to find</param>
        /// <param name="dependencyType">The type of dependency</param>
        /// <returns>The build node associated with item or null if it was not found</returns>
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

            if (!ReferenceEquals(node.AssetItem, item))
            {
                nodes.TryRemove(nodeDesc, out node);
                return null;
            }

            return node;
        }

        /// <summary>
        /// Finds all the nodes associated with the asset
        /// </summary>
        /// <param name="item">The asset item to find</param>
        /// <returns>The build nodes associated with item or null if it was not found</returns>
        public IEnumerable<BuildAssetNode> FindNodes(AssetItem item)
        {
            return nodes.Where(x => x.Value.AssetItem == item).Select(x => x.Value);
        }

        /// <summary>
        /// Removes the node from the build graph
        /// </summary>
        /// <param name="node">The node to remove</param>
        public void RemoveNode(BuildAssetNode node)
        {
            var nodeDesc = new BuildNodeDesc
            {
                AssetId = node.AssetItem.Id,
                BuildDependencyType = node.DependencyType
            };

            nodes.TryRemove(nodeDesc, out node);
        }

        /// <summary>
        /// Removes the nodes associated with item from the build graph
        /// </summary>
        /// <param name="item">The item to use to find nodes to remove</param>
        public void RemoveNode(AssetItem item)
        {
            var assetNodes = FindNodes(item).ToList();
            foreach (var buildAssetNode in assetNodes)
            {
                BuildAssetNode node;
                nodes.TryRemove(new BuildNodeDesc { AssetId = item.Id, BuildDependencyType = buildAssetNode.DependencyType } , out node);
            }
        }
    }
}