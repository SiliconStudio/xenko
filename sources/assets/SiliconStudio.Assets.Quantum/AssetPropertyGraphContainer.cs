using System;
using System.Collections.Generic;
using System.Linq;

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

        public void InitializeSession()
        {
            foreach (var asset in session.Packages.SelectMany(x => x.Assets).Where(x => !(x.Asset is SourceCodeAsset)))
            {
                InitializeAsset(asset);
            }
        }

        public AssetPropertyGraph InitializeAsset(AssetItem assetItem)
        {
            // SourceCodeAssets have no property
            if (assetItem.Asset is SourceCodeAsset)
                return null;

            var graph = AssetQuantumRegistry.ConstructPropertyGraph(this, assetItem);
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
