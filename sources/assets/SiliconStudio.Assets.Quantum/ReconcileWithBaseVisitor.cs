using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Assets.Quantum
{
    public class ReconcileWithBaseVisitor : GraphVisitorBase
    {
        protected override void VisitNode(IGraphNode node, GraphNodePath currentPath)
        {
            if (!(node.Content is ObjectContent))
            {
                Reconcile((AssetNode)node);
            }
            base.VisitNode(node, currentPath);
        }

        private static void Reconcile(AssetNode assetNode)
        {
            if (assetNode.BaseContent != null)
            {
                var baseNode = (AssetNode)assetNode.BaseContent.OwnerNode;
                var localValue = assetNode.Content.Retrieve();
                var baseValue = assetNode.BaseContent.Retrieve();

                // Reconcile occurs only when the node is not overridden.
                if (!assetNode.IsContentOverridden())
                {
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
                        return;
                    }

                    if (assetNode.Content.Descriptor is CollectionDescriptor || assetNode.Content.Descriptor is DictionaryDescriptor)
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
                }
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
