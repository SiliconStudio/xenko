using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Assets.Quantum
{
    public interface IAssetNode : IContentNode
    {
        AssetPropertyGraph PropertyGraph { get; }

        IContentNode BaseContent { get; }

        void SetContent(string key, IContentNode content);

        IContentNode GetContent(string key);

        /// <summary>
        /// Resets the overrides attached to this node and its descendants, recursively.
        /// </summary>
        /// <param name="indexToReset">The index of the override to reset in this node, if relevant.</param>
        void ResetOverride(Index indexToReset);
    }

    internal interface IAssetNodeInternal : IAssetNode
    {
        void SetPropertyGraph([NotNull] AssetPropertyGraph assetPropertyGraph);

        void SetBaseContent(IContentNode content);
    }

    public class AssetObjectNode : ObjectContent, IAssetNode, IAssetNodeInternal
    {
        private readonly Dictionary<string, IContentNode> contents = new Dictionary<string, IContentNode>();

        public AssetObjectNode(object value, Guid guid, ITypeDescriptor descriptor, bool isPrimitive, IReference reference)
            : base(value, guid, descriptor, isPrimitive, reference)
        {
        }

        public AssetPropertyGraph PropertyGraph { get; private set; }

        public IContentNode BaseContent { get; private set; }

        public void SetContent(string key, IContentNode content)
        {
            contents[key] = content;
        }

        public IContentNode GetContent(string key)
        {
            IContentNode content;
            contents.TryGetValue(key, out content);
            return content;
        }

        /// <inheritdoc/>
        public void ResetOverride(Index indexToReset)
        {
            PropertyGraph.ResetOverride(this, indexToReset);
        }

        void IAssetNodeInternal.SetPropertyGraph(AssetPropertyGraph assetPropertyGraph)
        {
            if (assetPropertyGraph == null) throw new ArgumentNullException(nameof(assetPropertyGraph));
            PropertyGraph = assetPropertyGraph;
        }

        void IAssetNodeInternal.SetBaseContent(IContentNode content)
        {
            BaseContent = content;
        }
    }

    public class AssetBoxedNode : BoxedContent, IAssetNode, IAssetNodeInternal
    {
        private readonly Dictionary<string, IContentNode> contents = new Dictionary<string, IContentNode>();

        public AssetBoxedNode(object value, Guid guid, ITypeDescriptor descriptor, bool isPrimitive)
            : base(value, guid, descriptor, isPrimitive)
        {
        }

        public AssetPropertyGraph PropertyGraph { get; private set; }

        public IContentNode BaseContent { get; private set; }

        public void SetContent(string key, IContentNode content)
        {
            contents[key] = content;
        }

        public IContentNode GetContent(string key)
        {
            IContentNode content;
            contents.TryGetValue(key, out content);
            return content;
        }

        /// <inheritdoc/>
        public void ResetOverride(Index indexToReset)
        {
            PropertyGraph.ResetOverride(this, indexToReset);
        }

        void IAssetNodeInternal.SetPropertyGraph(AssetPropertyGraph assetPropertyGraph)
        {
            if (assetPropertyGraph == null) throw new ArgumentNullException(nameof(assetPropertyGraph));
            PropertyGraph = assetPropertyGraph;
        }

        void IAssetNodeInternal.SetBaseContent(IContentNode content)
        {
            BaseContent = content;
        }
    }

    public class AssetMemberNode : MemberContent, IAssetNode, IAssetNodeInternal
    {
        private AssetPropertyGraph propertyGraph;
        private readonly Dictionary<string, IContentNode> contents = new Dictionary<string, IContentNode>();

        private OverrideType contentOverride;
        private readonly Dictionary<ItemId, OverrideType> itemOverrides = new Dictionary<ItemId, OverrideType>();
        private readonly Dictionary<ItemId, OverrideType> keyOverrides = new Dictionary<ItemId, OverrideType>();
        private CollectionItemIdentifiers collectionItemIdentifiers;
        private ItemId restoringId;

        public AssetMemberNode(INodeBuilder nodeBuilder, Guid guid, IObjectNode parent, IMemberDescriptor memberDescriptor, bool isPrimitive, IReference reference)
            : base(nodeBuilder, guid, parent, memberDescriptor, isPrimitive, reference)
        {
            Changed += ContentChanged;
            IsNonIdentifiableCollectionContent = MemberDescriptor.GetCustomAttributes<NonIdentifiableCollectionItemsAttribute>(true)?.Any() ?? false;
            CanOverride =MemberDescriptor.GetCustomAttributes<NonOverridableAttribute>(true)?.Any() != true;
        }

        public bool IsNonIdentifiableCollectionContent { get; }

        public bool CanOverride { get; }

        internal bool ResettingOverride { get; set; }

        public event EventHandler<EventArgs> OverrideChanging;

        public event EventHandler<EventArgs> OverrideChanged;

        public AssetPropertyGraph PropertyGraph { get { return propertyGraph; } internal set { if (value == null) throw new ArgumentNullException(nameof(value)); propertyGraph = value; } }

        public IContentNode BaseContent { get; private set; }

        public void SetContent(string key, IContentNode content)
        {
            contents[key] = content;
        }

        public IContentNode GetContent(string key)
        {
            IContentNode content;
            contents.TryGetValue(key, out content);
            return content;
        }

        void IAssetNodeInternal.SetPropertyGraph(AssetPropertyGraph assetPropertyGraph)
        {
            if (assetPropertyGraph == null) throw new ArgumentNullException(nameof(assetPropertyGraph));
            PropertyGraph = assetPropertyGraph;
        }

        void IAssetNodeInternal.SetBaseContent(IContentNode content)
        {
            BaseContent = content;
        }

        public void OverrideContent(bool isOverridden)
        {
            if (CanOverride)
            {
                OverrideChanging?.Invoke(this, EventArgs.Empty);
                contentOverride = isOverridden ? OverrideType.New : OverrideType.Base;
                OverrideChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void OverrideItem(bool isOverridden, Index index)
        {
            if (CanOverride)
            {
                OverrideChanging?.Invoke(this, EventArgs.Empty);
                SetItemOverride(isOverridden ? OverrideType.New : OverrideType.Base, index);
                OverrideChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void OverrideKey(bool isOverridden, Index index)
        {
            if (CanOverride)
            {
                OverrideChanging?.Invoke(this, EventArgs.Empty);
                SetKeyOverride(isOverridden ? OverrideType.New : OverrideType.Base, index);
                OverrideChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void OverrideDeletedItem(bool isOverridden, ItemId deletedId)
        {
            CollectionItemIdentifiers ids;
            if (CanOverride && TryGetCollectionItemIds(Retrieve(), out ids))
            {
                OverrideChanging?.Invoke(this, EventArgs.Empty);
                SetOverride(isOverridden ? OverrideType.New : OverrideType.Base, deletedId, itemOverrides);
                if (isOverridden)
                {
                    ids.MarkAsDeleted(deletedId);
                }
                else
                {
                    ids.UnmarkAsDeleted(deletedId);
                }
                OverrideChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsItemDeleted(ItemId itemId)
        {
            var collection = Retrieve();
            CollectionItemIdentifiers ids;
            if (!TryGetCollectionItemIds(collection, out ids))
                throw new InvalidOperationException("No Collection item identifier associated to the given collection.");
            return ids.IsDeleted(itemId);
        }

        public bool TryGetCollectionItemIds(object instance, out CollectionItemIdentifiers itemIds)
        {
            if (collectionItemIdentifiers != null)
            {
                itemIds = collectionItemIdentifiers;
                return true;
            }

            var result = CollectionItemIdHelper.TryGetCollectionItemIds(instance, out collectionItemIdentifiers);
            itemIds = collectionItemIdentifiers;
            return result;
        }

        public void Restore(object restoredItem, ItemId id)
        {
            CollectionItemIdentifiers oldIds = null;
            CollectionItemIdentifiers ids;
            if (!IsNonIdentifiableCollectionContent && TryGetCollectionItemIds(Retrieve(), out ids))
            {
                // Remove the item from deleted ids if it was here.
                ids.UnmarkAsDeleted(id);
                // Get a clone of the CollectionItemIdentifiers before we add back the item.
                oldIds = new CollectionItemIdentifiers();
                ids.CloneInto(oldIds, null);
            }
            // Actually restore the item.
            Add(restoredItem);

            if (TryGetCollectionItemIds(Retrieve(), out ids) && oldIds != null)
            {
                // Find the new id that has been generated by the Add
                var idToReplace = oldIds.FindMissingId(ids);
                if (idToReplace == ItemId.Empty)
                    throw new InvalidOperationException("No ItemId to replace has been generated.");
            }
        }

        public void Restore(object restoredItem, Index index, ItemId id)
        {
            restoringId = id;
            Add(restoredItem, index);
            restoringId = ItemId.Empty;
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(Retrieve(), out ids))
            {
                // Remove the item from deleted ids if it was here.
                ids.UnmarkAsDeleted(id);
            }
        }

        public void RemoveAndDiscard(object item, Index itemIndex, ItemId id)
        {
            Remove(item, itemIndex);
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(Retrieve(), out ids))
            {
                // Remove the item from deleted ids if it was here.
                ids.UnmarkAsDeleted(id);
            }
        }

        internal bool HasId(ItemId id)
        {
            Index index;
            return TryIdToIndex(id, out index);
        }

        internal Index IdToIndex(ItemId id)
        {
            Index index;
            if (!TryIdToIndex(id, out index)) throw new InvalidOperationException("No Collection item identifier associated to the given collection.");
            return index;
        }

        internal bool TryIdToIndex(ItemId id, out Index index)
        {
            if (id == ItemId.Empty)
            {
                index = Index.Empty;
                return true;
            }

            var collection = Retrieve();
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(collection, out ids))
            {
                index = new Index(ids.GetKey(id));
                return !index.IsEmpty;
            }
            index = Index.Empty;
            return false;

        }

        internal ItemId IndexToId(Index index)
        {
            ItemId id;
            if (!TryIndexToId(index, out id)) throw new InvalidOperationException("No Collection item identifier associated to the given collection.");
            return id;
        }

        public bool TryIndexToId(Index index, out ItemId id)
        {
            if (index == Index.Empty)
            {
                id = ItemId.Empty;
                return true;
            }

            var collection = Retrieve();
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(collection, out ids))
            {
                return ids.TryGet(index.Value, out id);
            }
            id = ItemId.Empty;
            return false;
        }

        /// <inheritdoc/>
        public void ResetOverride(Index indexToReset)
        {
            if (indexToReset.IsEmpty)
            {
                OverrideContent(false);
            }
            else
            {
                OverrideItem(false, indexToReset);
            }
            PropertyGraph.ResetOverride(this, indexToReset);
        }

        private void ContentChanged(object sender, MemberNodeChangeEventArgs e)
        {
            // Make sure that we have item ids everywhere we're supposed to.
            AssetCollectionItemIdHelper.GenerateMissingItemIds(e.Member.Retrieve());

            // Clear the cached item identifier collection.
            collectionItemIdentifiers = null;

            var node = (AssetMemberNode)e.Member;
            if (node.IsNonIdentifiableCollectionContent)
                return;

            // Create new ids for collection items
            var baseNode = (AssetMemberNode)BaseContent;
            var isOverriding = baseNode != null && !PropertyGraph.UpdatingPropertyFromBase;
            var removedId = ItemId.Empty;
            switch (e.ChangeType)
            {
                case ContentChangeType.ValueChange:
                case ContentChangeType.CollectionUpdate:
                    break;
                case ContentChangeType.CollectionAdd:
                    {
                        var collectionDescriptor = e.Member.Descriptor as CollectionDescriptor;
                        var itemIds = CollectionItemIdHelper.GetCollectionItemIds(e.Member.Retrieve());
                        // Compute the id we will add for this item
                        ItemId itemId;
                        if (baseNode != null && PropertyGraph.UpdatingPropertyFromBase)
                        {
                            var baseCollection = baseNode.Retrieve();
                            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(baseCollection);
                            itemId = itemIds.FindMissingId(baseIds);
                        }
                        else
                        {
                            itemId = restoringId != ItemId.Empty ? restoringId : ItemId.New();
                        }
                        // Add the id to the proper location (insert or add)
                        if (collectionDescriptor != null)
                        {
                            if (e.Index != Index.Empty)
                            {
                                itemIds.Insert(e.Index.Int, itemId);
                            }
                            else
                            {
                                throw new InvalidOperationException("An item has been added to a collection that does not have a predictable Add. Consider using NonIdentifiableCollectionItemsAttribute on this collection.");
                            }
                        }
                        else
                        {
                            itemIds[e.Index.Value] = itemId;
                        }
                    }
                    break;
                case ContentChangeType.CollectionRemove:
                    {
                        var collectionDescriptor = e.Member.Descriptor as CollectionDescriptor;
                        if (collectionDescriptor != null)
                        {
                            var itemIds = CollectionItemIdHelper.GetCollectionItemIds(e.Member.Retrieve());
                            removedId = itemIds.DeleteAndShift(e.Index.Int, isOverriding);
                        }
                        else
                        {
                            var itemIds = CollectionItemIdHelper.GetCollectionItemIds(e.Member.Retrieve());
                            removedId = itemIds.Delete(e.Index.Value, isOverriding);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            // Don't update override if propagation from base is disabled.
            if (PropertyGraph?.Container?.PropagateChangesFromBase == false)
                return;

            // Mark it as New if it does not come from the base
            if (baseNode != null && !PropertyGraph.UpdatingPropertyFromBase && !ResettingOverride)
            {
                if (e.ChangeType != ContentChangeType.CollectionRemove)
                {
                    if (e.Index == Index.Empty)
                    {
                        OverrideContent(!ResettingOverride);
                    }
                    else
                    {
                        OverrideItem(!ResettingOverride, e.Index);
                    }
                }
                else
                {
                    OverrideDeletedItem(true, removedId);
                }
            }
        }

        internal void SetContentOverride(OverrideType overrideType)
        {
            if (CanOverride)
            {
                contentOverride = overrideType;
            }
        }

        internal void SetItemOverride(OverrideType overrideType, Index index)
        {
            if (CanOverride)
            {
                var id = IndexToId(index);
                SetOverride(overrideType, id, itemOverrides);
            }
        }

        internal void SetKeyOverride(OverrideType overrideType, Index index)
        {
            if (CanOverride)
            {
                var id = IndexToId(index);
                SetOverride(overrideType, id, keyOverrides);
            }
        }

        private static void SetOverride(OverrideType overrideType, ItemId id, Dictionary<ItemId, OverrideType> dictionary)
        {
            if (overrideType == OverrideType.Base)
            {
                dictionary.Remove(id);
            }
            else
            {
                dictionary[id] = overrideType;
            }
        }

        public OverrideType GetContentOverride()
        {
            return contentOverride;
        }

        public OverrideType GetItemOverride(Index index)
        {
            var result = OverrideType.Base;
            ItemId id;
            if (!TryIndexToId(index, out id))
                return result;
            return itemOverrides.TryGetValue(id, out result) ? result : OverrideType.Base;
        }

        public OverrideType GetKeyOverride(Index index)
        {
            var result = OverrideType.Base;
            ItemId id;
            if (!TryIndexToId(index, out id))
                return result;
            return keyOverrides.TryGetValue(id, out result) ? result : OverrideType.Base;
        }

        public bool IsContentOverridden()
        {
            return (contentOverride & OverrideType.New) == OverrideType.New;
        }

        public bool IsItemOverridden(Index index)
        {
            OverrideType result;
            ItemId id;
            if (!TryIndexToId(index, out id))
                return false;
            return itemOverrides.TryGetValue(id, out result) && (result & OverrideType.New) == OverrideType.New;
        }

        public bool IsItemOverriddenDeleted(ItemId id)
        {
            OverrideType result;
            return IsItemDeleted(id) && itemOverrides.TryGetValue(id, out result) && (result & OverrideType.New) == OverrideType.New;
        }

        public bool IsKeyOverridden(Index index)
        {
            OverrideType result;
            ItemId id;
            if (!TryIndexToId(index, out id))
                return false;
            return keyOverrides.TryGetValue(id, out result) && (result & OverrideType.New) == OverrideType.New;
        }

        public bool IsContentInherited()
        {
            return BaseContent != null && !IsContentOverridden();
        }

        public bool IsItemInherited(Index index)
        {
            return BaseContent != null && !IsItemOverridden(index);
        }

        public bool IsKeyInherited(Index index)
        {
            return BaseContent != null && !IsKeyOverridden(index);
        }

        public IEnumerable<Index> GetOverriddenItemIndices()
        {
            if (BaseContent == null)
                yield break;

            CollectionItemIdentifiers ids;
            var collection = Retrieve();
            if (!TryGetCollectionItemIds(collection, out ids))
                yield break;

            foreach (var flags in itemOverrides)
            {
                if ((flags.Value & OverrideType.New) == OverrideType.New)
                {
                    // If the override is a deleted item, there's no matching index to return.
                    if (ids.IsDeleted(flags.Key))
                        continue;

                    yield return IdToIndex(flags.Key);
                }
            }
        }

        public IEnumerable<Index> GetOverriddenKeyIndices()
        {
            if (BaseContent == null)
                yield break;

            CollectionItemIdentifiers ids;
            var collection = Retrieve();
            if (!TryGetCollectionItemIds(collection, out ids))
                yield break;

            foreach (var flags in keyOverrides)
            {
                if ((flags.Value & OverrideType.New) == OverrideType.New)
                {
                    // If the override is a deleted item, there's no matching index to return.
                    if (ids.IsDeleted(flags.Key))
                        continue;

                    yield return IdToIndex(flags.Key);
                }
            }
        }

        internal Dictionary<ItemId, OverrideType> GetAllOverrides()
        {
            return itemOverrides;
        }
    }
}
