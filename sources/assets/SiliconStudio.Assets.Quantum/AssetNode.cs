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
        private readonly Dictionary<ItemId, MemberFlags> memberFlags = new Dictionary<ItemId, MemberFlags>();

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

        public event EventHandler<EventArgs> MemberFlagsChanging;

        public event EventHandler<EventArgs> MemberFlagsChanged;

        public IContent BaseContent { get; private set; }

        internal bool ResettingOverride { get; private set; }

        public void SetIsInherited(bool isInherited, Index index)
        {
            var id = IndexToId(index);
            memberFlags[id] = isInherited ? MemberFlags.Inherited : MemberFlags.Default;
        }

        // TODO: we might want to turn this private and only expose IsInherited/IsOverridden etc.
        public MemberFlags GetMemberFlags(Index index)
        {
            MemberFlags result;
            var id = IndexToId(index);
            return memberFlags.TryGetValue(id, out result) ? result : MemberFlags.Default;
        }

        public bool IsInherited(Index index)
        {
            return (GetMemberFlags(index) & MemberFlags.Inherited) == MemberFlags.Inherited;
        }

        public bool IsOverridden(Index index)
        {
            return BaseContent != null && !IsInherited(index);
        }

        internal IReadOnlyDictionary<ItemId, MemberFlags> GetAllMemberFlags()
        {
            return memberFlags;
        }

        public IEnumerable<Index> GetOverriddenIndices()
        {
            if (BaseContent == null)
                yield break;

            foreach (var flags in memberFlags)
            {
                if ((flags.Value & MemberFlags.Inherited) != MemberFlags.Inherited)
                {
                    yield return IdToIndex(flags.Key);
                }
            }
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
            // TODO FIXME
            //if (SessionViewModel.Instance.IsInFixupAssetContext)
            //    return;

            var baseNode = (AssetNode)BaseContent?.OwnerNode;
            if (e.ChangeType == ContentChangeType.ValueChange)
            {
                if (!(baseNode?.contentUpdating ?? false))
                {
                    // TODO: pass the index to the event to avoid unuseful notifications
                    MemberFlagsChanging?.Invoke(this, EventArgs.Empty);
                    SetIsInherited(ResettingOverride, e.Index);
                    MemberFlagsChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            if (e.ChangeType == ContentChangeType.CollectionAdd)
            {
                var collection = e.Content.Retrieve();
                var ids = CollectionItemIdHelper.GetCollectionItemIds(collection);
                ItemId itemId;

                if (baseNode?.contentUpdating ?? false)
                {
                    var baseCollection = baseNode?.Content.Retrieve();
                    var baseIds = CollectionItemIdHelper.GetCollectionItemIds(baseCollection);
                    itemId = ids.FindMissingId(baseIds);
                }
                else
                {
                    itemId = ItemId.New();
                }

                if (e.Content.Descriptor is CollectionDescriptor)
                {
                    var index = e.Index.Int;
                    ids.Insert(index, itemId);
                }
                else
                {
                    ids.Add(e.Index.Value, itemId);
                }
                if (!(baseNode?.contentUpdating ?? false))
                {
                    SetIsInherited(false, e.Index);
                }
            }

            if (e.ChangeType == ContentChangeType.CollectionRemove)
            {
                if (baseNode != null)
                {
                    var collection = e.Content.Retrieve();
                    var ids = CollectionItemIdHelper.GetCollectionItemIds(collection);
                    if (e.Content.Descriptor is CollectionDescriptor)
                        ids.DeleteAndShift(e.Index.Int);
                    else
                        ids.Delete(e.Index.Value);
                }
            }
        }

        public void ResetOverride(Index index, object overriddenValue, ContentChangeType changeType)
        {
            if (BaseContent == null || (changeType == ContentChangeType.ValueChange && !IsOverridden(index)))
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
            var ids = CollectionItemIdHelper.GetCollectionItemIds(collection);
            return new Index(ids.GetKey(id));
        }

        internal ItemId IndexToId(Index index)
        {
            if (index == Index.Empty)
                return ItemId.Empty;

            var collection = Content.Retrieve();
            var ids = CollectionItemIdHelper.GetCollectionItemIds(collection);
            return ids.GetId(index.Value);
        }

        public AssetNode ResolveObjectPath(ObjectPath path, out Index index)
        {
            var currentNode = this;
            index = Index.Empty;
            for (var i = 0; i < path.Items.Count; i++)
            {
                var item = path.Items[i];
                switch (item.Type)
                {
                    case ObjectPath.ItemType.Member:
                        index = Index.Empty;
                        if (currentNode.Content.IsReference)
                        {
                            currentNode = (AssetNode)currentNode.GetTarget();
                        }
                        currentNode = (AssetNode)currentNode.GetChild(item.AsMember());
                        break;
                    case ObjectPath.ItemType.Index:
                        index = new Index(item.Value);
                        if (currentNode.Content.IsReference && i < path.Items.Count - 1)
                        {
                            currentNode = (AssetNode)currentNode.GetTarget(new Index(item.Value));
                        }
                        break;
                    case ObjectPath.ItemType.ItemId:
                        var ids = CollectionItemIdHelper.GetCollectionItemIds(currentNode.Content.Retrieve());
                        var key = ids.GetKey(item.AsItemId());
                        index = new Index(key);
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
    }
}
