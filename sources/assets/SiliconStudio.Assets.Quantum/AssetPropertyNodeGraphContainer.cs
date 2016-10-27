using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Assets.Quantum
{
    public class AssetPropertyNodeGraphContainer
    {
        private readonly PackageSession session;
        private readonly AssetNodeContainer nodeContainer;
        private readonly Dictionary<Guid, AssetPropertyNodeGraph> registeredGraphs = new Dictionary<Guid, AssetPropertyNodeGraph>();

        public AssetPropertyNodeGraphContainer(PackageSession session, AssetNodeContainer nodeContainer)
        {
            this.session = session;
            this.nodeContainer = nodeContainer;
        }

        public void InitializeSession()
        {
            foreach (var asset in session.Packages.SelectMany(x => x.Assets).Where(x => !(x.Asset is SourceCodeAsset)))
            {
                InitializeAsset(asset);
            }
        }

        public AssetPropertyNodeGraph InitializeAsset(AssetItem assetItem)
        {
            // SourceCodeAssets have no property
            if (assetItem.Asset is SourceCodeAsset)
                return null;

            var graph = AssetQuantumRegistry.ConstructPropertyGraph(nodeContainer, assetItem);
            RegisterGraph(assetItem.Id, graph);
            return graph;
        }

        public AssetPropertyNodeGraph GetGraph(Guid assetId)
        {
            AssetPropertyNodeGraph graph;
            registeredGraphs.TryGetValue(assetId, out graph);
            return graph;
        }

        public void RegisterGraph(Guid assetId, AssetPropertyNodeGraph graph)
        {
            registeredGraphs.Add(assetId, graph);
        }
    }
}
