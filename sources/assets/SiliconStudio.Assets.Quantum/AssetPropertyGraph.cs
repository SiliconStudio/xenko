// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    [AssetPropertyGraph(typeof(Asset))]
    public class AssetPropertyGraph : IDisposable
    {
        protected readonly AssetItem AssetItem;
        public readonly AssetGraphNodeChangeListener NodeListener;
        protected AssetPropertyGraphContainer Container;
        private readonly AssetToBaseNodeLinker baseLinker;
        // TODO: this should be turn private once all quantum code has been split from view model
        public readonly Dictionary<AssetNode, EventHandler<ContentChangeEventArgs>> baseLinkedNodes = new Dictionary<AssetNode, EventHandler<ContentChangeEventArgs>>();

        public AssetPropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
        {
            if (assetItem == null)
                throw new ArgumentNullException(nameof(assetItem));
            AssetItem = assetItem;
            Container = container;
            AssetCollectionItemIdHelper.GenerateMissingItemIds(assetItem.Asset);
            CollectionItemIdsAnalysis.FixupItemIds(assetItem, logger);
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
            if (assetItem.Asset.Archetype == null)
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

        public void PrepareSave(ILogger logger)
        {
            AssetCollectionItemIdHelper.GenerateMissingItemIds(AssetItem.Asset);
            CollectionItemIdsAnalysis.FixupItemIds(AssetItem, logger);
            UpdateOverridesForSerialization();
        }

        public void UpdateOverridesForSerialization()
        {
            var visitor = new OverrideTypePathGenerator();
            visitor.Visit(RootNode);
            AssetItem.Overrides = visitor.Result;
        }

        private void ApplyOverrides()
        {
            if (RootNode == null)
                throw new InvalidOperationException($"{nameof(RootNode)} is not set.");
            if (AssetItem.Overrides != null)
            {
                foreach (var overrideInfo in AssetItem.Overrides)
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
            var index = Index.Empty;

            if (e.ChangeType == ContentChangeType.ValueChange)
            {
                // Find the index of the item in this instance corresponding to the modified item in the base.
                index = node.RetrieveDerivedIndex(e.Index);

                // If this item does not exist anymore in the instance, stop here.
                if (index.IsEmpty && !e.Index.IsEmpty)
                    return;

                // Otherwise, retrieve the current override (before the change).
                overrideType = index == Index.Empty ? node.GetContentOverride() : node.GetItemOverride(index);
            }
            else if (e.ChangeType == ContentChangeType.CollectionRemove)
            {
                // If we're removing, we need to find the item id that still exists in our instance but not in the base anymore.
                // TODO: at some point it would be better to merge this algorithm and RetrieveDerivedIndex in a single method, private to this class (and remove RetrieveDerivedIndex)
                var baseIds = CollectionItemIdHelper.GetCollectionItemIds(e.Content.Retrieve());
                var instanceIds = CollectionItemIdHelper.GetCollectionItemIds(node.Content.Retrieve());
                var missingIds = baseIds.FindMissingIds(instanceIds);
                bool foundUnique = false;
                foreach (var id in missingIds)
                {
                    if (node.TryIdToIndex(id, out index))
                    {
                        if (foundUnique) throw new InvalidOperationException("Couldn't find a unique item id in the instance collection corresponding to the item removed in the base collection");
                        foundUnique = true;
                    }
                }
                if (!foundUnique) throw new InvalidOperationException("Couldn't find a single item id in the instance collection corresponding to the item removed in the base collection");
            }

            // Then we update the value of this instance according to the value from the base, but only if it's not overridden.
            // Remark: if it's an Add/Remove, we always propagate the action which is why overrideType is always not New in this case.
            if (assetContent is MemberContent && !overrideType.HasFlag(OverrideType.New))
            {
                UpdatingPropertyFromBase = true;
                // Clone the value from the base
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
