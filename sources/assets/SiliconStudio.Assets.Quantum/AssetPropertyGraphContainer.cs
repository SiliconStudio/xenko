using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets.Quantum
{
    public class AssetPropertyGraphContainer
    {
        private readonly PackageSession session;
        private readonly Dictionary<Guid, AssetPropertyGraph> registeredGraphs = new Dictionary<Guid, AssetPropertyGraph>();

        public AssetPropertyGraphContainer(PackageSession session, AssetNodeContainer nodeContainer)
        {
            this.session = session;
            NodeContainer = nodeContainer;
        }

        public AssetNodeContainer NodeContainer { get; }

        public bool PropagateChangesFromBase { get; set; } = true;

        public AssetPropertyGraph InitializeAsset(AssetItem assetItem, ILogger logger)
        {
            // SourceCodeAssets have no property
            if (assetItem.Asset is SourceCodeAsset)
                return null;

            var graph = AssetQuantumRegistry.ConstructPropertyGraph(this, assetItem, logger);
            RegisterGraph(assetItem.Id, graph);
            return graph;
        }

        public AssetPropertyGraph GetGraph(Guid assetId)
        {
            AssetPropertyGraph graph;
            registeredGraphs.TryGetValue(assetId, out graph);
            return graph;
        }

        public void RegisterGraph(Guid assetId, AssetPropertyGraph graph)
        {
            registeredGraphs.Add(assetId, graph);
        }

        public AssetItem GetAssetById(Guid assetId)
        {
            return session.FindAsset(assetId);
        }
    }
}
