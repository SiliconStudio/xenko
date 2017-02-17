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
    public interface IAssetNode : IGraphNode
    {
        AssetPropertyGraph PropertyGraph { get; }

        IGraphNode BaseNode { get; }

        void SetContent(string key, IGraphNode node);

        IGraphNode GetContent(string key);

        event EventHandler<EventArgs> OverrideChanging;

        event EventHandler<EventArgs> OverrideChanged;

        /// <summary>
        /// Resets the overrides attached to this node and its descendants, recursively.
        /// </summary>
        /// <param name="indexToReset">The index of the override to reset in this node, if relevant.</param>
        // TODO: switch to two versions: one with and one without index, and move into specialized interfaces
        void ResetOverride(Index indexToReset);
    }

    internal interface IAssetNodeInternal : IAssetNode
    {
        bool ResettingOverride { get; set; }

        void SetPropertyGraph([NotNull] AssetPropertyGraph assetPropertyGraph);

        void SetBaseContent(IGraphNode node);
    }

    internal interface IAssetObjectNodeInternal : IAssetObjectNode, IAssetNodeInternal
    {
        void SetObjectReference(Index index, bool isReference);

        bool IsObjectReference(Index index);

        IEnumerable<Index> GetObjectReferenceIndices();

        void NotifyOverrideChanging();

        void NotifyOverrideChanged();
    }

    public interface IAssetObjectNode : IAssetNode, IObjectNode
    {
        // TODO: this should be only here!
        //void ResetOverride(Index indexToReset);

        [NotNull]
        new IAssetMemberNode this[string name] { get; }

        void OverrideItem(bool isOverridden, Index index);

        void OverrideKey(bool isOverridden, Index index);

        void OverrideDeletedItem(bool isOverridden, ItemId deletedId);

        bool IsItemDeleted(ItemId itemId);

        bool TryGetCollectionItemIds(object instance, out CollectionItemIdentifiers itemIds);

        void Restore(object restoredItem, ItemId id);

        void Restore(object restoredItem, Index index, ItemId id);

        void RemoveAndDiscard(object item, Index itemIndex, ItemId id);

        OverrideType GetItemOverride(Index index);

        OverrideType GetKeyOverride(Index index);

        bool IsItemInherited(Index index);

        bool IsKeyInherited(Index index);

        bool IsItemOverridden(Index index);

        bool IsItemOverriddenDeleted(ItemId id);

        bool IsKeyOverridden(Index index);

        ItemId IndexToId(Index index);

        bool TryIndexToId(Index index, out ItemId id);

        bool HasId(ItemId id);

        Index IdToIndex(ItemId id);

        bool TryIdToIndex(ItemId id, out Index index);

        IEnumerable<Index> GetOverriddenItemIndices();

        IEnumerable<Index> GetOverriddenKeyIndices();
    }

    public interface IAssetMemberNode : IAssetNode, IMemberNode
    {
        bool IsNonIdentifiableCollectionContent { get; }

        bool CanOverride { get; }

        [NotNull]
        new IAssetObjectNode Parent { get; }

        new IAssetObjectNode Target { get; }

        void OverrideContent(bool isOverridden);

        OverrideType GetContentOverride();

        bool IsContentOverridden();

        bool IsContentInherited();
    }

    internal struct AssetObjectNodeExtended
    {
        [NotNull] private readonly IAssetObjectNodeInternal node;
        private readonly Dictionary<string, IGraphNode> contents;
        private readonly Dictionary<ItemId, OverrideType> itemOverrides;
        private readonly Dictionary<ItemId, OverrideType> keyOverrides;
        private readonly HashSet<ItemId> objectReferences;
        private CollectionItemIdentifiers collectionItemIdentifiers;
        private ItemId restoringId;

        public AssetObjectNodeExtended([NotNull] IAssetObjectNodeInternal node)
        {
            this.node = node;
            contents = new Dictionary<string, IGraphNode>();
            itemOverrides = new Dictionary<ItemId, OverrideType>();
            keyOverrides = new Dictionary<ItemId, OverrideType>();
            objectReferences = new HashSet<ItemId>();
            collectionItemIdentifiers = null;
            restoringId = ItemId.Empty;
            PropertyGraph = null;
            BaseNode = null;
            ResettingOverride = false;
        }

        public AssetPropertyGraph PropertyGraph { get; private set; }

        public IGraphNode BaseNode { get; private set; }

        internal bool ResettingOverride { get; set; }

        public void SetContent(string key, IGraphNode node)
        {
            contents[key] = node;
        }

        public IGraphNode GetContent(string key)
        {
            IGraphNode node;
            contents.TryGetValue(key, out node);
            return node;
        }

        /// <inheritdoc/>
        public void ResetOverride(Index indexToReset)
        {
            OverrideItem(false, indexToReset);
            PropertyGraph.ResetOverride(node, indexToReset);
        }

        public void OverrideItem(bool isOverridden, Index index)
        {
            node.NotifyOverrideChanging();
            var id = IndexToId(index);
            SetOverride(isOverridden ? OverrideType.New : OverrideType.Base, id, itemOverrides);
            node.NotifyOverrideChanged();
        }

        public void OverrideKey(bool isOverridden, Index index)
        {
            node.NotifyOverrideChanging();
            var id = IndexToId(index);
            SetOverride(isOverridden ? OverrideType.New : OverrideType.Base, id, keyOverrides);
            node.NotifyOverrideChanged();
        }

        public void OverrideDeletedItem(bool isOverridden, ItemId deletedId)
        {
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(node.Retrieve(), out ids))
            {
                node.NotifyOverrideChanging();
                SetOverride(isOverridden ? OverrideType.New : OverrideType.Base, deletedId, itemOverrides);
                if (isOverridden)
                {
                    ids.MarkAsDeleted(deletedId);
                }
                else
                {
                    ids.UnmarkAsDeleted(deletedId);
                }
                node.NotifyOverrideChanged();
            }
        }

        public bool IsItemDeleted(ItemId itemId)
        {
            var collection = node.Retrieve();
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
            if (TryGetCollectionItemIds(node.Retrieve(), out ids))
            {
                // Remove the item from deleted ids if it was here.
                ids.UnmarkAsDeleted(id);
                // Get a clone of the CollectionItemIdentifiers before we add back the item.
                oldIds = new CollectionItemIdentifiers();
                ids.CloneInto(oldIds, null);
            }
            // Actually restore the item.
            node.Add(restoredItem);

            if (TryGetCollectionItemIds(node.Retrieve(), out ids) && oldIds != null)
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
            node.Add(restoredItem, index);
            restoringId = ItemId.Empty;
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(node.Retrieve(), out ids))
            {
                // Remove the item from deleted ids if it was here.
                ids.UnmarkAsDeleted(id);
            }
        }

        public void RemoveAndDiscard(object item, Index itemIndex, ItemId id)
        {
            node.Remove(item, itemIndex);
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(node.Retrieve(), out ids))
            {
                // Remove the item from deleted ids if it was here.
                ids.UnmarkAsDeleted(id);
            }
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

        public bool IsItemInherited(Index index)
        {
            return BaseNode != null && !IsItemOverridden(index);
        }

        public bool IsKeyInherited(Index index)
        {
            return BaseNode != null && !IsKeyOverridden(index);
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

        public IEnumerable<Index> GetOverriddenItemIndices()
        {
            if (BaseNode == null)
                yield break;

            CollectionItemIdentifiers ids;
            var collection = node.Retrieve();
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
            if (BaseNode == null)
                yield break;

            CollectionItemIdentifiers ids;
            var collection = node.Retrieve();
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

        public bool HasId(ItemId id)
        {
            Index index;
            return TryIdToIndex(id, out index);
        }

        public Index IdToIndex(ItemId id)
        {
            Index index;
            if (!TryIdToIndex(id, out index)) throw new InvalidOperationException("No Collection item identifier associated to the given collection.");
            return index;
        }

        public bool TryIdToIndex(ItemId id, out Index index)
        {
            if (id == ItemId.Empty)
            {
                index = Index.Empty;
                return true;
            }

            var collection = node.Retrieve();
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(collection, out ids))
            {
                index = new Index(ids.GetKey(id));
                return !index.IsEmpty;
            }
            index = Index.Empty;
            return false;

        }

        public ItemId IndexToId(Index index)
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

            var collection = node.Retrieve();
            CollectionItemIdentifiers ids;
            if (TryGetCollectionItemIds(collection, out ids))
            {
                return ids.TryGet(index.Value, out id);
            }
            id = ItemId.Empty;
            return false;
        }

        internal void SetObjectReference(Index index, bool isReference)
        {
            ItemId id;
            if (!TryIndexToId(index, out id))
            {
                // TODO: this could be supported but we have to play with indices when an insert operation occurs if we want to map by Index.
                throw new NotSupportedException("Setting object reference on collection with non-identifiable items is not supported");
            }
            if (isReference)
            {
                objectReferences.Add(id);
            }
            else
            {
                objectReferences.Remove(id);
            }
        }

        internal IEnumerable<Index> GetObjectReferenceIndices()
        {
            CollectionItemIdentifiers ids;
            var collection = node.Retrieve();
            TryGetCollectionItemIds(collection, out ids);

            foreach (var reference in objectReferences)
            {
                {
                    // If the override is a deleted item, there's no matching index to return.
                    if (ids.IsDeleted(reference))
                        continue;

                    yield return IdToIndex(reference);
                }
            }
        }

        internal bool IsObjectReference(Index index)
        {
            ItemId id;
            return TryIndexToId(index, out id) && objectReferences.Contains(id);
        }

        internal void OnItemChanged(object sender, ItemChangeEventArgs e)
        {
            var value = node.Retrieve();

            if (!CollectionItemIdHelper.HasCollectionItemIds(value))
                return;

            // Make sure that we have item ids everywhere we're supposed to.
            AssetCollectionItemIdHelper.GenerateMissingItemIds(e.Node.Retrieve());

            // Clear the cached item identifier collection.
            collectionItemIdentifiers = null;

            // Create new ids for collection items
            var baseNode = (AssetObjectNode)BaseNode;
            var isOverriding = baseNode != null && !PropertyGraph.UpdatingPropertyFromBase;
            var removedId = ItemId.Empty;
            switch (e.ChangeType)
            {
                case ContentChangeType.CollectionUpdate:
                    break;
                case ContentChangeType.CollectionAdd:
                    {
                        var collectionDescriptor = node.Descriptor as CollectionDescriptor;
                        var itemIds = CollectionItemIdHelper.GetCollectionItemIds(node.Retrieve());
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
                        var collectionDescriptor = node.Descriptor as CollectionDescriptor;
                        if (collectionDescriptor != null)
                        {
                            var itemIds = CollectionItemIdHelper.GetCollectionItemIds(e.Node.Retrieve());
                            removedId = itemIds.DeleteAndShift(e.Index.Int, isOverriding);
                        }
                        else
                        {
                            var itemIds = CollectionItemIdHelper.GetCollectionItemIds(e.Node.Retrieve());
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
                    OverrideItem(!ResettingOverride, e.Index);
                }
                else
                {
                    OverrideDeletedItem(true, removedId);
                }
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

        public void SetPropertyGraph(AssetPropertyGraph assetPropertyGraph)
        {
            if (assetPropertyGraph == null) throw new ArgumentNullException(nameof(assetPropertyGraph));
            PropertyGraph = assetPropertyGraph;
        }

        public void SetBaseContent(IGraphNode baseNode)
        {
            BaseNode = baseNode;
        }
    }

    internal class AssetObjectNode : ObjectNode, IAssetObjectNodeInternal
    {
        private AssetObjectNodeExtended ex;

        public AssetObjectNode([NotNull] INodeBuilder nodeBuilder, object value, Guid guid, ITypeDescriptor descriptor, bool isPrimitive, IReference reference)
            : base(nodeBuilder, value, guid, descriptor, isPrimitive, reference)
        {
            ex = new AssetObjectNodeExtended(this);
            ItemChanged += (sender, e) => ex.OnItemChanged(sender, e);
        }

        public AssetPropertyGraph PropertyGraph => ex.PropertyGraph;

        public IGraphNode BaseNode => ex.BaseNode;

        public new IAssetMemberNode this[string name] => (IAssetMemberNode)base[name];

        public event EventHandler<EventArgs> OverrideChanging;

        public event EventHandler<EventArgs> OverrideChanged;

        public void SetContent(string key, IGraphNode node) => ex.SetContent(key, node);

        public IGraphNode GetContent(string key) => ex.GetContent(key);

        public void ResetOverride(Index indexToReset) => ex.ResetOverride(indexToReset);

        public void OverrideItem(bool isOverridden, Index index) => ex.OverrideItem(isOverridden, index);

        public void OverrideKey(bool isOverridden, Index index) => ex.OverrideKey(isOverridden, index);

        public void OverrideDeletedItem(bool isOverridden, ItemId deletedId) => ex.OverrideDeletedItem(isOverridden, deletedId);

        public bool IsItemDeleted(ItemId itemId) => ex.IsItemDeleted(itemId);

        public bool TryGetCollectionItemIds(object instance, out CollectionItemIdentifiers itemIds) => ex.TryGetCollectionItemIds(instance, out itemIds);

        public void Restore(object restoredItem, ItemId id) => ex.Restore(restoredItem, id);

        public void Restore(object restoredItem, Index index, ItemId id) => ex.Restore(restoredItem, index, id);

        public void RemoveAndDiscard(object item, Index itemIndex, ItemId id) => ex.RemoveAndDiscard(item, itemIndex, id);

        public OverrideType GetItemOverride(Index index) => ex.GetItemOverride(index);

        public OverrideType GetKeyOverride(Index index) => ex.GetKeyOverride(index);

        public bool IsItemInherited(Index index) => ex.IsItemInherited(index);

        public bool IsKeyInherited(Index index) => ex.IsKeyInherited(index);

        public bool IsItemOverridden(Index index) => ex.IsItemOverridden(index);

        public bool IsItemOverriddenDeleted(ItemId id) => ex.IsItemOverriddenDeleted(id);

        public bool IsKeyOverridden(Index index) => ex.IsKeyOverridden(index);

        public IEnumerable<Index> GetOverriddenItemIndices() => ex.GetOverriddenItemIndices();

        public IEnumerable<Index> GetOverriddenKeyIndices() => ex.GetOverriddenKeyIndices();

        public ItemId IndexToId(Index index) => ex.IndexToId(index);

        public bool TryIndexToId(Index index, out ItemId id) => ex.TryIndexToId(index, out id);

        public bool HasId(ItemId id) => ex.HasId(id);

        public Index IdToIndex(ItemId id) => ex.IdToIndex(id);

        public bool TryIdToIndex(ItemId id, out Index index) => ex.TryIdToIndex(id, out index);

        void IAssetObjectNodeInternal.SetObjectReference(Index index, bool isReference) => ex.SetObjectReference(index, isReference);

        bool IAssetObjectNodeInternal.IsObjectReference(Index index) => ex.IsObjectReference(index);

        IEnumerable<Index> IAssetObjectNodeInternal.GetObjectReferenceIndices() => ex.GetObjectReferenceIndices();

        void IAssetObjectNodeInternal.NotifyOverrideChanging() => OverrideChanging?.Invoke(this, EventArgs.Empty);

        void IAssetObjectNodeInternal.NotifyOverrideChanged() => OverrideChanged?.Invoke(this, EventArgs.Empty);

        bool IAssetNodeInternal.ResettingOverride { get { return ex.ResettingOverride; } set { ex.ResettingOverride = value; } }

        void IAssetNodeInternal.SetPropertyGraph(AssetPropertyGraph assetPropertyGraph) => ex.SetPropertyGraph(assetPropertyGraph);

        void IAssetNodeInternal.SetBaseContent(IGraphNode node) => ex.SetBaseContent(node);
    }

    internal class AssetBoxedNode : BoxedNode, IAssetObjectNodeInternal
    {
        private AssetObjectNodeExtended ex;

        public AssetBoxedNode([NotNull] INodeBuilder nodeBuilder, object value, Guid guid, ITypeDescriptor descriptor, bool isPrimitive)
            : base(nodeBuilder, value, guid, descriptor, isPrimitive)
        {
            ex = new AssetObjectNodeExtended(this);
            ItemChanged += (sender, e) => ex.OnItemChanged(sender, e);
        }

        public AssetPropertyGraph PropertyGraph => ex.PropertyGraph;

        public IGraphNode BaseNode => ex.BaseNode;

        public new IAssetMemberNode this[string name] => (IAssetMemberNode)base[name];

        public event EventHandler<EventArgs> OverrideChanging;

        public event EventHandler<EventArgs> OverrideChanged;

        public void SetContent(string key, IGraphNode node) => ex.SetContent(key, node);

        public IGraphNode GetContent(string key) => ex.GetContent(key);

        public void ResetOverride(Index indexToReset) => ex.ResetOverride(indexToReset);

        public void OverrideItem(bool isOverridden, Index index) => ex.OverrideItem(isOverridden, index);

        public void OverrideKey(bool isOverridden, Index index) => ex.OverrideKey(isOverridden, index);

        public void OverrideDeletedItem(bool isOverridden, ItemId deletedId) => ex.OverrideDeletedItem(isOverridden, deletedId);

        public bool IsItemDeleted(ItemId itemId) => ex.IsItemDeleted(itemId);

        public bool TryGetCollectionItemIds(object instance, out CollectionItemIdentifiers itemIds) => ex.TryGetCollectionItemIds(instance, out itemIds);

        public void Restore(object restoredItem, ItemId id) => ex.Restore(restoredItem, id);

        public void Restore(object restoredItem, Index index, ItemId id) => ex.Restore(restoredItem, index, id);

        public void RemoveAndDiscard(object item, Index itemIndex, ItemId id) => ex.RemoveAndDiscard(item, itemIndex, id);

        public OverrideType GetItemOverride(Index index) => ex.GetItemOverride(index);

        public OverrideType GetKeyOverride(Index index) => ex.GetKeyOverride(index);

        public bool IsItemInherited(Index index) => ex.IsItemInherited(index);

        public bool IsKeyInherited(Index index) => ex.IsKeyInherited(index);

        public bool IsItemOverridden(Index index) => ex.IsItemOverridden(index);

        public bool IsItemOverriddenDeleted(ItemId id) => ex.IsItemOverriddenDeleted(id);

        public bool IsKeyOverridden(Index index) => ex.IsKeyOverridden(index);

        public IEnumerable<Index> GetOverriddenItemIndices() => ex.GetOverriddenItemIndices();

        public IEnumerable<Index> GetOverriddenKeyIndices() => ex.GetOverriddenKeyIndices();

        public ItemId IndexToId(Index index) => ex.IndexToId(index);

        public bool TryIndexToId(Index index, out ItemId id) => ex.TryIndexToId(index, out id);

        public bool HasId(ItemId id) => ex.HasId(id);

        public Index IdToIndex(ItemId id) => ex.IdToIndex(id);

        public bool TryIdToIndex(ItemId id, out Index index) => ex.TryIdToIndex(id, out index);

        void IAssetObjectNodeInternal.SetObjectReference(Index index, bool isReference) => ex.SetObjectReference(index, isReference);

        bool IAssetObjectNodeInternal.IsObjectReference(Index index) => ex.IsObjectReference(index);

        IEnumerable<Index> IAssetObjectNodeInternal.GetObjectReferenceIndices() => ex.GetObjectReferenceIndices();

        void IAssetObjectNodeInternal.NotifyOverrideChanging() => OverrideChanging?.Invoke(this, EventArgs.Empty);

        void IAssetObjectNodeInternal.NotifyOverrideChanged() => OverrideChanged?.Invoke(this, EventArgs.Empty);

        bool IAssetNodeInternal.ResettingOverride { get { return ex.ResettingOverride; } set { ex.ResettingOverride = value; } }

        void IAssetNodeInternal.SetPropertyGraph(AssetPropertyGraph assetPropertyGraph) => ex.SetPropertyGraph(assetPropertyGraph);

        void IAssetNodeInternal.SetBaseContent(IGraphNode node) => ex.SetBaseContent(node);
    }

    internal class AssetMemberNode : MemberNode, IAssetMemberNode, IAssetNodeInternal
    {
        private AssetPropertyGraph propertyGraph;
        private readonly Dictionary<string, IGraphNode> contents = new Dictionary<string, IGraphNode>();

        private OverrideType contentOverride;
        private bool isObjectReference;

        public AssetMemberNode(INodeBuilder nodeBuilder, Guid guid, IObjectNode parent, IMemberDescriptor memberDescriptor, bool isPrimitive, IReference reference)
            : base(nodeBuilder, guid, parent, memberDescriptor, isPrimitive, reference)
        {
            Changed += ContentChanged;
            IsNonIdentifiableCollectionContent = MemberDescriptor.GetCustomAttributes<NonIdentifiableCollectionItemsAttribute>(true)?.Any() ?? false;
            CanOverride = MemberDescriptor.GetCustomAttributes<NonOverridableAttribute>(true)?.Any() != true;
        }

        public bool IsNonIdentifiableCollectionContent { get; }

        public bool CanOverride { get; }

        internal bool ResettingOverride { get; set; }

        internal bool IsObjectReference { get; set; }

        public event EventHandler<EventArgs> OverrideChanging;

        public event EventHandler<EventArgs> OverrideChanged;

        public AssetPropertyGraph PropertyGraph { get { return propertyGraph; } internal set { if (value == null) throw new ArgumentNullException(nameof(value)); propertyGraph = value; } }

        public IGraphNode BaseNode { get; private set; }

        [NotNull]
        public new IAssetObjectNode Parent => (IAssetObjectNode)base.Parent;

        public new IAssetObjectNode Target => (IAssetObjectNode)base.Target;

        public void SetContent(string key, IGraphNode node)
        {
            contents[key] = node;
        }

        public IGraphNode GetContent(string key)
        {
            IGraphNode node;
            contents.TryGetValue(key, out node);
            return node;
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

        /// <inheritdoc/>
        public void ResetOverride(Index indexToReset)
        {
            OverrideContent(false);
            PropertyGraph.ResetOverride(this, indexToReset);
        }

        private void ContentChanged(object sender, MemberNodeChangeEventArgs e)
        {
            // Make sure that we have item ids everywhere we're supposed to.
            AssetCollectionItemIdHelper.GenerateMissingItemIds(e.Member.Retrieve());

            var node = (AssetMemberNode)e.Member;
            if (node.IsNonIdentifiableCollectionContent)
                return;

            // Don't update override if propagation from base is disabled.
            if (PropertyGraph?.Container == null || PropertyGraph?.Container?.PropagateChangesFromBase == false)
                return;

            // Mark it as New if it does not come from the base
            if (BaseNode != null && !PropertyGraph.UpdatingPropertyFromBase && !ResettingOverride)
            {
                OverrideContent(!ResettingOverride);
            }
        }

        internal void SetContentOverride(OverrideType overrideType)
        {
            if (CanOverride)
            {
                contentOverride = overrideType;
            }
        }

        public OverrideType GetContentOverride()
        {
            return contentOverride;
        }

        public bool IsContentOverridden()
        {
            return (contentOverride & OverrideType.New) == OverrideType.New;
        }

        public bool IsContentInherited()
        {
            return BaseNode != null && !IsContentOverridden();
        }

        bool IAssetNodeInternal.ResettingOverride { get; set; }

        void IAssetNodeInternal.SetPropertyGraph(AssetPropertyGraph assetPropertyGraph)
        {
            if (assetPropertyGraph == null) throw new ArgumentNullException(nameof(assetPropertyGraph));
            PropertyGraph = assetPropertyGraph;
        }

        void IAssetNodeInternal.SetBaseContent(IGraphNode node)
        {
            BaseNode = node;
        }
    }
}
