// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Assets.Editor.ViewModel.Quantum;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    public class AssetPropertyNodeGraph : IDisposable
    {
        private readonly AssetItem assetItem;
        public readonly AssetGraphNodeChangeListener NodeListener;

        public AssetPropertyNodeGraph(INodeContainer container, AssetItem assetItem)
        {
            if (assetItem == null)
                throw new ArgumentNullException(nameof(assetItem));
            this.assetItem = assetItem;
            RootNode = (AssetNode)container.GetOrCreateNode(assetItem.Asset);
            ApplyOverrides();

            NodeListener = new AssetGraphNodeChangeListener(RootNode, ShouldListenToTargetNode);
        }

        public AssetNode RootNode { get; }

        // TODO: turn protected
        public virtual bool ShouldListenToTargetNode(MemberContent member, IGraphNode targetNode)
        {
            return true;
        }

        public void UpdateOverridesForSerialization()
        {
            var visitor = new OverridePathVisitor();
            visitor.Visit(RootNode);
            assetItem.Overrides = visitor.Result;
        }

        private void ApplyOverrides()
        {
            if (RootNode == null)
                throw new InvalidOperationException($"{nameof(RootNode)} is not set.");
            if (assetItem.Overrides != null)
            {
                foreach (var overrideInfo in assetItem.Overrides)
                {
                    Index index;
                    var node = RootNode.ResolveObjectPath(overrideInfo.Key, out index);
                    node.SetOverride(overrideInfo.Value, index);
                }
            }
        }

        public void Dispose()
        {
            NodeListener.Dispose();
        }
    }
}
