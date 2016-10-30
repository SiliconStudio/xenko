// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    [AssetPropertyNodeGraphAttribute(typeof(Asset))]
    public class AssetPropertyNodeGraph : IDisposable
    {
        protected readonly AssetItem assetItem;
        public readonly AssetGraphNodeChangeListener NodeListener;
        protected AssetPropertyNodeGraphContainer Container;
        // TODO: this should be turn private once all quantum code has been split from view model
        public readonly AssetToBaseNodeLinker baseLinker;
        public readonly Dictionary<AssetNode, EventHandler<ContentChangeEventArgs>> baseLinkedNodes = new Dictionary<AssetNode, EventHandler<ContentChangeEventArgs>>();

        public AssetPropertyNodeGraph(AssetPropertyNodeGraphContainer container, AssetItem assetItem)
        {
            if (assetItem == null)
                throw new ArgumentNullException(nameof(assetItem));
            this.assetItem = assetItem;
            Container = container;
            RootNode = (AssetNode)Container.NodeContainer.GetOrCreateNode(assetItem.Asset);
            ApplyOverrides();

            NodeListener = new AssetGraphNodeChangeListener(RootNode, ShouldListenToTargetNode);
            baseLinker = new AssetToBaseNodeLinker(this);
        }

        public void Dispose()
        {
            NodeListener.Dispose();
        }

        public AssetNode RootNode { get; }

        // TODO: turn protected
        public virtual bool ShouldListenToTargetNode(MemberContent member, IGraphNode targetNode)
        {
            return true;
        }

        public virtual IGraphNode FindTarget(IGraphNode sourceNode, IGraphNode target)
        {
            return target;
        }

        public void UpdateOverridesForSerialization()
        {
            var visitor = new OverrideTypePathGenerator();
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

        // TODO: turn private
        public void LinkToBase(AssetNode sourceRootNode, AssetNode targetRootNode)
        {
            if (baseLinker != null)
            {
                baseLinker.ShouldVisit = (member, node) => (node == sourceRootNode || !baseLinkedNodes.ContainsKey((AssetNode)node)) && ShouldListenToTargetNode(member, node);
                baseLinker.LinkGraph(sourceRootNode, targetRootNode);
            }
        }
    }
}
