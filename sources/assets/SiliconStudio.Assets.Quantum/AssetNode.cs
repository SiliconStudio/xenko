using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    public class AssetNode : GraphNode
    {
        public static readonly int ResetFromBase;
        private bool contentUpdating;
        private Func<object, object> cloner;
        private OverrideType contentOverride;
        private readonly Dictionary<ItemId, OverrideType> itemOverrides = new Dictionary<ItemId, OverrideType>();
        private readonly Dictionary<ItemId, OverrideType> keyOverrides = new Dictionary<ItemId, OverrideType>();

        static AssetNode()
        {
            ResetFromBase = Enum.GetValues(typeof(ContentChangeType)).Length;
        }

        public AssetNode(string name, IContent content, Guid guid)
            : base(name, content, guid)
        {
            Cloner = CloneFromBase;
            Content.PrepareChange += (sender, e) => contentUpdating = true;
            Content.FinalizeChange += (sender, e) => contentUpdating = false;
            Content.Changed += ContentChanged;
        }

        public sealed override IContent Content => base.Content;

        public Func<object, object> Cloner { get { return cloner; } set { if (value == null) throw new ArgumentNullException(nameof(value)); cloner = value; } }

        public event EventHandler<EventArgs> OverrideChanging;

        public event EventHandler<EventArgs> OverrideChanged;

        public IContent BaseContent { get; private set; }

        internal bool ResettingOverride { get; private set; }

        public void OverrideContent(bool isOverridden)
        {
            contentOverride = isOverridden ? OverrideType.New : OverrideType.Base;
        }

        public void OverrideItem(bool isOverridden, Index index)
        {
            SetItemOverride(isOverridden ? OverrideType.New : OverrideType.Base, index);
        }

        public void OverrideKey(bool isOverridden, Index index)
        {
            SetKeyOverride(isOverridden ? OverrideType.New : OverrideType.Base, index);
        }

        internal void SetContentOverride(OverrideType overrideType)
        {
            contentOverride = overrideType;
        }

        internal void SetItemOverride(OverrideType overrideType, Index index)
        {
            var id = IndexToId(index);
            SetOverride(overrideType, id, itemOverrides);
        }

        internal void SetKeyOverride(OverrideType overrideType, Index index)
        {
            var id = IndexToId(index);
            SetOverride(overrideType, id, keyOverrides);
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
            OverrideType result;
            var id = IndexToId(index);
            return itemOverrides.TryGetValue(id, out result) ? result : OverrideType.Base;
        }

        public OverrideType GetKeyOverride(Index index)
        {
            OverrideType result;
            var id = IndexToId(index);
            return keyOverrides.TryGetValue(id, out result) ? result : OverrideType.Base;
        }

        public bool IsContentOverridden()
        {
            return (contentOverride & OverrideType.New) == OverrideType.New;
        }

        public bool IsItemOverridden(Index index)
        {
            OverrideType result;
            var id = IndexToId(index);
            return itemOverrides.TryGetValue(id, out result) && (result & OverrideType.New) == OverrideType.New;
        }

        public bool IsKeyOverridden(Index index)
        {
            OverrideType result;
            var id = IndexToId(index);
            return keyOverrides.TryGetValue(id, out result) && (result & OverrideType.New) == OverrideType.New;
        }

        // TODO: implement Is[Content/Item/Key]Inherited instead
        public bool IsInherited(Index index)
        {
            if (BaseContent == null)
                return false;

            OverrideType result;
            var id = IndexToId(index);
            return !itemOverrides.TryGetValue(id, out result) || (result & OverrideType.New) != OverrideType.New;
        }

        public IEnumerable<Index> GetOverriddenItemIndices()
        {
            if (BaseContent == null)
                yield break;

            foreach (var flags in itemOverrides)
            {
                if ((flags.Value & OverrideType.New) == OverrideType.New)
                {
                    yield return IdToIndex(flags.Key);
                }
            }
        }

        public IEnumerable<Index> GetOverriddenKeyIndices()
        {
            if (BaseContent == null)
                yield break;

            foreach (var flags in keyOverrides)
            {
                if ((flags.Value & OverrideType.New) == OverrideType.New)
                {
                    yield return IdToIndex(flags.Key);
                }
            }
        }

        internal Dictionary<ItemId, OverrideType> GetAllOverrides()
        {
            return itemOverrides;
        }

        private object RetrieveBaseContent(Index index)
        {
            object baseContent = null;

            var baseNode = (AssetNode)BaseContent?.OwnerNode;
            if (baseNode != null)
            {
                var id = IndexToId(index);
                var baseIndex = baseNode.IdToIndex(id);
                baseContent = baseNode.Content.Retrieve(baseIndex);
            }

            return baseContent;
        }

        public Index RetrieveDerivedIndex(Index baseIndex, object baseValue)
        {
            var memberContent = BaseContent as MemberContent;
            if (memberContent == null || BaseContent == null)
                return Index.Empty;

            if (baseIndex.IsEmpty)
                return baseIndex;

            var id = baseValue != null ? IdentifiableHelper.GetId(baseValue) : Guid.Empty;

            if (id != Guid.Empty)
            {
                foreach (var index in Content.Indices)
                {
                    var value = Content.Retrieve(index);
                    if (value != null && IdentifiableHelper.GetId(value) == id)
                        return index;
                }
                return Index.Empty;
            }

            return Content.Indices.Any(x => Equals(x, baseIndex)) ? baseIndex : Index.Empty;
        }

        /// <summary>
        /// Clones the given object, remove any override information on it, and propagate its id (from <see cref="IdentifiableHelper"/>) to the cloned object.
        /// </summary>
        /// <param name="value">The object to clone.</param>
        /// <returns>A clone of the given object.</returns>
        /// <remarks>If the given object is null, this method returns null.</remarks>
        /// <remarks>If the given object is a content reference, the given object won't be cloned but directly returned.</remarks>
        public static object CloneFromBase(object value)
        {
            if (value == null)
                return null;

            // TODO: check if the cloner is aware of the content type (attached reference) and does not already avoid cloning them.
            // TODO FIXME
            //if (SessionViewModel.Instance.ContentReferenceService.IsContentType(value.GetType()))
            //    return value;

            var id = IdentifiableHelper.GetId(value);
            var result = AssetCloner.Clone(value, AssetClonerFlags.RemoveOverrides);
            IdentifiableHelper.SetId(result, id);
            return result;
        }

        public void SetBase(IContent baseContent)
        {
            BaseContent = baseContent;
        }

        private void ContentChanged(object sender, ContentChangeEventArgs e)
        {
            // Create new ids for collection items
            var baseNode = (AssetNode)BaseContent?.OwnerNode;
            switch (e.ChangeType)
            {
                case ContentChangeType.ValueChange:
                    break;
                case ContentChangeType.CollectionAdd:
                    {
                        var collectionDescriptor = e.Content.Descriptor as CollectionDescriptor;
                        var itemIds = CollectionItemIdHelper.GetCollectionItemIds(e.Content.Retrieve());
                        // Compute the id we will add for this item
                        ItemId itemId;
                        if (baseNode?.contentUpdating == true)
                        {
                            var baseCollection = baseNode.Content.Retrieve();
                            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(baseCollection);
                            itemId = itemIds.FindMissingId(baseIds);
                        }
                        else
                        {
                            itemId = ItemId.New();
                        }
                        // Add the id to the proper location (insert or add)
                        if (collectionDescriptor != null)
                        {
                            itemIds.Insert(e.Index.Int, itemId);
                        }
                        else
                        {
                            itemIds[e.Index.Value] = itemId;
                        }
                    }
                    break;
                case ContentChangeType.CollectionRemove:
                    {
                        var collectionDescriptor = e.Content.Descriptor as CollectionDescriptor;
                        bool markAsDelete = baseNode != null && !baseNode.contentUpdating;
                        if (collectionDescriptor != null)
                        {
                            var itemIds = CollectionItemIdHelper.GetCollectionItemIds(e.Content.Retrieve());
                            itemIds.DeleteAndShift(e.Index.Int, markAsDelete);
                        }
                        else
                        {
                            var itemIds = CollectionItemIdHelper.GetCollectionItemIds(e.Content.Retrieve());
                            itemIds.Delete(e.Index.Value, markAsDelete);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            // TODO FIXME
            //if (SessionViewModel.Instance.IsInFixupAssetContext)
            //    return;

            // Mark it as New if it does not come from the base
            if (!baseNode?.contentUpdating == true)
            {
                OverrideChanging?.Invoke(this, EventArgs.Empty);
                if (e.Index == Index.Empty)
                {
                    OverrideContent(!ResettingOverride);
                }
                else
                {
                    OverrideItem(!ResettingOverride, e.Index);                
                }
                OverrideChanged?.Invoke(this, EventArgs.Empty);
            }

        }

        public void ResetOverride(Index index, object overriddenValue, ContentChangeType changeType)
        {
            if (BaseContent == null || (changeType == ContentChangeType.ValueChange && !IsItemOverridden(index)))
                return;

            object baseValue;
            object clonedValue;
            ResettingOverride = true;
            switch (changeType)
            {
                case ContentChangeType.ValueChange:
                    baseValue = RetrieveBaseContent(index);
                    clonedValue = Cloner(baseValue);
                    Content.Update(clonedValue, index);
                    break;
                case ContentChangeType.CollectionRemove:
                    baseValue = RetrieveBaseContent(index);
                    clonedValue = Cloner(baseValue);
                    Content.Add(clonedValue, index);
                    break;
                case ContentChangeType.CollectionAdd:
                    var value = Content.Retrieve(index);
                    Content.Remove(value, index);
                    break;
            }
            ResettingOverride = false;
        }

        internal Index IdToIndex(ItemId id)
        {
            if (id == ItemId.Empty)
                return Index.Empty;

            var collection = Content.Retrieve();
            CollectionItemIdentifiers ids;
            if (!CollectionItemIdHelper.TryGetCollectionItemIds(collection, out ids))
                throw new InvalidOperationException("No Collection item identifier associated to the given collection.");
            return new Index(ids.GetKey(id));
        }

        internal ItemId IndexToId(Index index)
        {
            if (index == Index.Empty)
                return ItemId.Empty;

            var collection = Content.Retrieve();
            CollectionItemIdentifiers ids;
            if (!CollectionItemIdHelper.TryGetCollectionItemIds(collection, out ids))
                throw new InvalidOperationException("No Collection item identifier associated to the given collection.");
            return ids.GetId(index.Value);
        }

        public AssetNode ResolveObjectPath(ObjectPath path, out Index index, out bool overrideOnKey)
        {
            var currentNode = this;
            index = Index.Empty;
            overrideOnKey = false;
            for (var i = 0; i < path.Items.Count; i++)
            {
                var item = path.Items[i];
                switch (item.Type)
                {
                    case ObjectPath.ItemType.Member:
                        index = Index.Empty;
                        overrideOnKey = false;
                        if (currentNode.Content.IsReference)
                        {
                            currentNode = (AssetNode)currentNode.GetTarget();
                        }
                        currentNode = (AssetNode)currentNode.GetChild(item.AsMember());
                        break;
                    case ObjectPath.ItemType.Index:
                        index = new Index(item.Value);
                        overrideOnKey = true;
                        if (currentNode.Content.IsReference && i < path.Items.Count - 1)
                        {
                            currentNode = (AssetNode)currentNode.GetTarget(new Index(item.Value));
                        }
                        break;
                    case ObjectPath.ItemType.ItemId:
                        var ids = CollectionItemIdHelper.GetCollectionItemIds(currentNode.Content.Retrieve());
                        var key = ids.GetKey(item.AsItemId());
                        index = new Index(key);
                        overrideOnKey = false;
                        if (currentNode.Content.IsReference && i < path.Items.Count - 1)
                        {
                            currentNode = (AssetNode)currentNode.GetTarget(new Index(key));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return currentNode;
        }

        public bool IsItemDeleted(ItemId itemId)
        {
            var collection = Content.Retrieve();
            CollectionItemIdentifiers ids;
            if (!CollectionItemIdHelper.TryGetCollectionItemIds(collection, out ids))
                throw new InvalidOperationException("No Collection item identifier associated to the given collection.");
            return ids.IsDeleted(itemId);
        }
    }
}
