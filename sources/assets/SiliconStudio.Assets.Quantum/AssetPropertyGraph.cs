// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    [AssetPropertyGraph(typeof(Asset))]
    public class AssetPropertyGraph : IDisposable
    {
        protected readonly AssetItem assetItem;
        public readonly AssetGraphNodeChangeListener NodeListener;
        protected AssetPropertyGraphContainer Container;
        private readonly AssetToBaseNodeLinker baseLinker;
        // TODO: this should be turn private once all quantum code has been split from view model
        public readonly Dictionary<AssetNode, EventHandler<ContentChangeEventArgs>> baseLinkedNodes = new Dictionary<AssetNode, EventHandler<ContentChangeEventArgs>>();

        public AssetPropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem)
        {
            if (assetItem == null)
                throw new ArgumentNullException(nameof(assetItem));
            this.assetItem = assetItem;
            Container = container;
            RootNode = (AssetNode)Container.NodeContainer.GetOrCreateNode(assetItem.Asset);
            ApplyOverrides();

            NodeListener = new AssetGraphNodeChangeListener(RootNode, ShouldListenToTargetNode);
            baseLinker = new AssetToBaseNodeLinker(this) { LinkAction = LinkBaseNode };
        }

        public void Dispose()
        {
            NodeListener.Dispose();
        }

        public AssetNode RootNode { get; }

        /// <summary>
        /// Gets or sets whether a property is currently being updated from a change in the base of this asset.
        /// </summary>
        public bool UpdatingPropertyFromBase { get; private set; }

        public Action<ContentChangeEventArgs, IContent> BaseContentChanged;

        public void RefreshBase(AssetPropertyGraph baseAssetGraph)
        {
            foreach (var linkedNode in baseLinkedNodes.Where(x => x.Value != null))
            {
                linkedNode.Key.BaseContent.Changed -= linkedNode.Value;
            }
            baseLinkedNodes.Clear();

            LinkToBase(RootNode, baseAssetGraph?.RootNode);
        }

        public void ReconcileWithBase()
        {
            if (assetItem.Asset.Base == null)
                return;

            var visitor = new ReconcileWithBaseVisitor();
            visitor.Visit(RootNode);
        }

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
                    bool overrideOnKey;
                    var node = RootNode.ResolveObjectPath(overrideInfo.Key, out index, out overrideOnKey);
                    if (index == Index.Empty)
                    {
                        node.SetContentOverride(overrideInfo.Value);
                    }
                    else if (!overrideOnKey)
                    {
                        node.SetItemOverride(overrideInfo.Value, index);
                    }
                    else
                    {
                        node.SetKeyOverride(overrideInfo.Value, index);
                    }
                }
            }
        }

        // TODO: turn private
        public void LinkToBase(AssetNode sourceRootNode, AssetNode targetRootNode)
        {
            baseLinker.ShouldVisit = (member, node) => (node == sourceRootNode || !baseLinkedNodes.ContainsKey((AssetNode)node)) && ShouldListenToTargetNode(member, node);
            baseLinker.LinkGraph(sourceRootNode, targetRootNode);
        }

        // TODO: turn protected
        public virtual object CloneValueFromBase(object value, AssetNode node)
        {
            return AssetNode.CloneFromBase(value);
        }

        private void LinkBaseNode(IGraphNode currentNode, IGraphNode baseNode)
        {
            var assetNode = (AssetNode)currentNode;
            assetNode.Cloner = x => CloneValueFromBase(x, assetNode);
            assetNode.SetBase(baseNode?.Content);
            if (!baseLinkedNodes.ContainsKey(assetNode))
            {
                EventHandler<ContentChangeEventArgs> action = null;
                if (baseNode != null)
                {
                    action = (s, e) => OnBaseContentChanged(e, currentNode.Content);
                    assetNode.BaseContent.Changed += action;
                }
                baseLinkedNodes.Add(assetNode, action);
            }
        }

        private void OnBaseContentChanged(ContentChangeEventArgs e, IContent assetContent)
        {
            // Ignore base change if propagation is disabled.
            if (!Container.PropagateChangesFromBase)
                return;

            var node = (AssetNode)assetContent.OwnerNode;
            var overrideType = OverrideType.Base;
            var index = node.RetrieveDerivedIndex(e.Index, e.OldValue);

            // This item does not exist anymore
            if (index.IsEmpty && !e.Index.IsEmpty && e.ChangeType != ContentChangeType.CollectionAdd)
                return;

            if (e.ChangeType == ContentChangeType.ValueChange)
            {
                overrideType = index == Index.Empty ? node.GetContentOverride() : node.GetItemOverride(index);
            }

            if (assetContent is MemberContent && !overrideType.HasFlag(OverrideType.New))
            {
                UpdatingPropertyFromBase = true;
                var newValue = node.Cloner(e.NewValue);
                switch (e.ChangeType)
                {
                    case ContentChangeType.ValueChange:
                        assetContent.Update(newValue, index);
                        break;
                    case ContentChangeType.CollectionAdd:
                        // Add at the same index than the base, if possible
                        if (assetContent.Descriptor is DictionaryDescriptor || assetContent.Indices.Any(x => x.Equals(e.Index)))
                            assetContent.Add(newValue, e.Index);
                        else
                            assetContent.Add(newValue);
                        break;
                    case ContentChangeType.CollectionRemove:
                        var item = assetContent.Retrieve(index);
                        assetContent.Remove(item, index);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                BaseContentChanged?.Invoke(e, assetContent);
                UpdatingPropertyFromBase = false;
            }
        }
    }
}
