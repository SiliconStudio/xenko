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
using SiliconStudio.Quantum.References;

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
            // Unlink previously linked nodes
            foreach (var linkedNode in baseLinkedNodes.Where(x => x.Value != null))
            {
                linkedNode.Key.BaseContent.Changed -= linkedNode.Value;
            }
            baseLinkedNodes.Clear();

            // Link nodes to the new base.
            // Note: in case of composition (prefabs, etc.), even if baseAssetGraph is null, each part (entities, etc.) will discover
            // its own base by itself via the FindTarget method.
            LinkToBase(RootNode, baseAssetGraph?.RootNode);
        }

        public void ReconcileWithBase()
        {
            var visitor = CreateReconcilierVisitor();
            visitor.Visiting += (node, path) => Reconcile((AssetNode)node);
            visitor.Visit(RootNode);
        }

        // TODO: turn protected
        public virtual bool ShouldListenToTargetNode(MemberContent member, IGraphNode targetNode)
        {
            return true;
        }

        /// <summary>
        /// Creates an instance of <see cref="GraphVisitorBase"/> that is suited to reconcile properties with the base.
        /// </summary>
        /// <returns>A new instance of <see cref="GraphVisitorBase"/> for reconciliation.</returns>
        protected virtual GraphVisitorBase CreateReconcilierVisitor()
        {
            return new GraphVisitorBase();
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
            if (AssetItem.Overrides == null)
                return;

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

        private static void Reconcile(AssetNode assetNode)
        {
            if (assetNode.Content is ObjectContent || assetNode.BaseContent == null || !assetNode.CanOverride)
                return;

            var baseNode = (AssetNode)assetNode.BaseContent.OwnerNode;
            var localValue = assetNode.Content.Retrieve();
            var baseValue = assetNode.BaseContent.Retrieve();

            // Reconcile occurs only when the node is not overridden.
            if (!assetNode.IsContentOverridden())
            {
                assetNode.ResettingOverride = true;
                // Handle null cases first
                if (localValue == null || baseValue == null)
                {
                    if (localValue == null && baseValue != null)
                    {
                        var clonedValue = assetNode.Cloner(baseValue);
                        assetNode.Content.Update(clonedValue);
                    }
                    else if (localValue != null /*&& baseValue == null*/)
                    {
                        assetNode.Content.Update(null);
                    }
                }
                else if (assetNode.Content.Descriptor is CollectionDescriptor || assetNode.Content.Descriptor is DictionaryDescriptor)
                {
                    var itemsToRemove = new List<ItemId>();
                    var itemsToAdd = new SortedList<object, ItemId>(new DefaultKeyComparer());
                    foreach (var index in assetNode.Content.Indices)
                    {
                        // Skip overridden items
                        if (assetNode.IsItemOverridden(index))
                            continue;

                        var itemId = assetNode.IndexToId(index);
                        // TODO: What should we do if it's empty? It can happen only from corrupted data
                        if (itemId != ItemId.Empty)
                        {
                            if (baseNode.IdToIndex(itemId) == Index.Empty)
                            {
                                itemsToRemove.Add(itemId);
                            }
                        }
                    }

                    // Clean items marked as deleted that are absent from the base.
                    var ids = CollectionItemIdHelper.GetCollectionItemIds(localValue);
                    foreach (var deletedId in ids.DeletedItems.ToList())
                    {
                        if (assetNode.BaseContent.Indices.All(x => baseNode.IndexToId(x) != deletedId))
                        {
                            ids.UnmarkAsDeleted(deletedId);
                        }
                    }
                    foreach (var index in assetNode.BaseContent.Indices)
                    {
                        var itemId = baseNode.IndexToId(index);
                        // TODO: What should we do if it's empty? It can happen only from corrupted data
                        if (itemId != ItemId.Empty && !assetNode.IsItemDeleted(itemId))
                        {
                            var localIndex = assetNode.IdToIndex(itemId);
                            if (localIndex == Index.Empty)
                            {
                                itemsToAdd.Add(index.Value, itemId);
                            }
                            else
                            {
                                if (!assetNode.IsItemOverridden(localIndex))
                                {
                                    var localItemValue = assetNode.Content.Retrieve(localIndex);
                                    var baseItemValue = baseNode.Content.Retrieve(index);
                                    if (ShouldReconcileItem(localItemValue, baseItemValue, assetNode.Content.Reference is ReferenceEnumerable))
                                    {
                                        var clonedValue = assetNode.Cloner(baseItemValue);
                                        assetNode.Content.Update(clonedValue, localIndex);
                                    }
                                }
                                if (assetNode.Content.Descriptor is DictionaryDescriptor && !assetNode.IsKeyOverridden(localIndex))
                                {
                                    if (ShouldReconcileItem(localIndex.Value, index.Value, false))
                                    {
                                        var clonedIndex = new Index(assetNode.Cloner(index.Value));
                                        var localItemValue = assetNode.Content.Retrieve(localIndex);
                                        assetNode.Content.Remove(localItemValue, localIndex);
                                        assetNode.Content.Add(localItemValue, clonedIndex);
                                        ids[clonedIndex.Value] = itemId;
                                    }
                                }
                            }
                        }
                    }

                    foreach (var item in itemsToRemove)
                    {
                        var index = assetNode.IdToIndex(item);
                        var value = assetNode.Content.Retrieve(index);
                        assetNode.Content.Remove(value, index);
                        // We're reconciling, so let's hack the normal behavior of marking the removed item as deleted.
                        ids.UnmarkAsDeleted(item);
                    }

                    foreach (var item in itemsToAdd)
                    {
                        var baseIndex = baseNode.IdToIndex(item.Value);
                        var baseItemValue = baseNode.Content.Retrieve(baseIndex);
                        var clonedValue = assetNode.Cloner(baseItemValue);
                        if (assetNode.Content.Descriptor is CollectionDescriptor)
                        {
                            // In a collection, we need to find an index that matches the index on the base to maintain order.
                            // Let's start with the same index and remove missing elements
                            var localIndex = baseIndex;

                            // Let's iterate through base indices...
                            foreach (var index in assetNode.BaseContent.Indices)
                            {
                                // ...until we reach the base index
                                if (index == baseIndex)
                                    break;

                                if (assetNode.IdToIndex(baseNode.IndexToId(index)) == Index.Empty)
                                {
                                    // If no corresponding item exist in our node, decrease the target index by one.
                                    localIndex = new Index(localIndex.Int - 1);
                                }
                            }

                            assetNode.Content.Add(clonedValue, localIndex);
                        }
                        else
                        {
                            assetNode.Content.Add(clonedValue, baseIndex);
                        }
                    }
                }
                else
                {
                    if (ShouldReconcileItem(localValue, baseValue, assetNode.Content.Reference is ObjectReference))
                    {
                        var clonedValue = assetNode.Cloner(baseValue);
                        assetNode.Content.Update(clonedValue);
                    }
                }
                assetNode.ResettingOverride = false;
            }
        }

        private static bool ShouldReconcileItem(object localValue, object baseValue, bool isReference)
        {
            if (isReference)
            {
                // Reference type, we check matches by type
                return baseValue.GetType() != localValue.GetType();
            }
            // Value type, we check for equality
            return !Equals(localValue, baseValue);
        }
    }
}
