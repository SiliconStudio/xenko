using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Yaml
{
    [DataContract]
    public class YamlAssetPath : IEquatable<YamlAssetPath>
    {
        public enum ItemType
        {
            Member,
            Index,
            ItemId
        }

        public struct Item : IEquatable<Item>
        {
            public readonly ItemType Type;
            public readonly object Value;
            public Item(ItemType type, object value)
            {
                Type = type;
                Value = value;
            }
            public string AsMember() { if (Type != ItemType.Member) throw new InvalidOperationException("This item is not a Member"); return (string)Value; }
            public ItemId AsItemId() { if (Type != ItemType.ItemId) throw new InvalidOperationException("This item is not a item Id"); return (ItemId)Value; }

            public bool Equals(Item other)
            {
                return Type == other.Type && Equals(Value, other.Value);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                return obj is Item && Equals((Item)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int)Type*397) ^ (Value?.GetHashCode() ?? 0);
                }
            }
        }

        private readonly List<Item> items = new List<Item>(16);

        public IReadOnlyList<Item> Items => items;

        public void PushMember(string memberName)
        {
            items.Add(new Item(ItemType.Member, memberName));
        }

        public void PushIndex(object index)
        {
            items.Add(new Item(ItemType.Index, index));
        }

        public void PushItemId(ItemId itemId)
        {
            items.Add(new Item(ItemType.ItemId, itemId));
        }

        public void Push(Item item)
        {
            items.Add(item);
        }

        public void RemoveFirstItem()
        {
            for (var i = 1; i < items.Count; ++i)
            {
                items[i - 1] = items[i];
            }
            if (items.Count > 0)
            {
                items.RemoveAt(items.Count - 1);
            }
        }

        public YamlAssetPath Clone()
        {
            var clone = new YamlAssetPath();
            clone.items.AddRange(items);
            return clone;
        }

        /// <summary>
        /// Convert this <see cref="YamlAssetPath"/> into a <see cref="MemberPath"/>.
        /// </summary>
        /// <param name="root">The actual instance that is root of this path.</param>
        /// <returns>An instance of <see cref="MemberPath"/> corresponding to the same target than this <see cref="YamlAssetPath"/>.</returns>
        [NotNull]
        public MemberPath ToMemberPath(object root)
        {
            var currentObject = root;
            var memberPath = new MemberPath();
            foreach (var item in Items)
            {
                if (currentObject == null)
                    throw new InvalidOperationException($"The path [{ToString()}] contains access to a member of a null object.");

                switch (item.Type)
                {
                    case ItemType.Member:
                    {
                        var typeDescriptor = TypeDescriptorFactory.Default.Find(currentObject.GetType());
                        var name = item.AsMember();
                        var memberDescriptor = typeDescriptor.Members.FirstOrDefault(x => x.Name == name);
                        if (memberDescriptor == null) throw new InvalidOperationException($"The path [{ToString()}] contains access to non-existing member [{name}].");
                        memberPath.Push(memberDescriptor);
                        currentObject = memberDescriptor.Get(currentObject);
                        break;
                    }
                    case ItemType.Index:
                    {
                        var typeDescriptor = TypeDescriptorFactory.Default.Find(currentObject.GetType());
                        var arrayDescriptor = typeDescriptor as ArrayDescriptor;
                        if (arrayDescriptor != null)
                        {
                            if (!(item.Value is int)) throw new InvalidOperationException($"The path [{ToString()}] contains non-integer index on an array.");
                            memberPath.Push(arrayDescriptor, (int)item.Value);
                            currentObject = arrayDescriptor.GetValue(currentObject, (int)item.Value);
                        }
                        var collectionDescriptor = typeDescriptor as CollectionDescriptor;
                        if (collectionDescriptor != null)
                        {
                            if (!(item.Value is int)) throw new InvalidOperationException($"The path [{ToString()}] contains non-integer index on a collection.");
                            memberPath.Push(collectionDescriptor, (int)item.Value);
                            currentObject = collectionDescriptor.GetValue(currentObject, (int)item.Value);
                        }
                        var dictionaryDescriptor = typeDescriptor as DictionaryDescriptor;
                        if (dictionaryDescriptor != null)
                        {
                            if (item.Value == null) throw new InvalidOperationException($"The path [{ToString()}] contains a null key on an dictionary.");
                            memberPath.Push(dictionaryDescriptor, item.Value);
                            currentObject = dictionaryDescriptor.GetValue(currentObject, item.Value);
                        }
                        break;
                    }
                    case ItemType.ItemId:
                    {
                        var ids = CollectionItemIdHelper.GetCollectionItemIds(currentObject);
                        var key = ids.GetKey(item.AsItemId());
                        var typeDescriptor = TypeDescriptorFactory.Default.Find(currentObject.GetType());
                        var arrayDescriptor = typeDescriptor as ArrayDescriptor;
                        if (arrayDescriptor != null)
                        {
                            if (!(key is int)) throw new InvalidOperationException($"The path [{ToString()}] contains a non-valid item id on an array.");
                            memberPath.Push(arrayDescriptor, (int)key);
                            currentObject = arrayDescriptor.GetValue(currentObject, (int)key);
                        }
                        var collectionDescriptor = typeDescriptor as CollectionDescriptor;
                        if (collectionDescriptor != null)
                        {
                            if (!(key is int)) throw new InvalidOperationException($"The path [{ToString()}] contains a non-valid item id on a collection.");
                            memberPath.Push(collectionDescriptor, (int)key);
                            currentObject = collectionDescriptor.GetValue(currentObject, (int)key);
                        }
                        var dictionaryDescriptor = typeDescriptor as DictionaryDescriptor;
                        if (dictionaryDescriptor != null)
                        {
                            if (key == null) throw new InvalidOperationException($"The path [{ToString()}] contains a non-valid item id on an dictionary.");
                            memberPath.Push(dictionaryDescriptor, key);
                            currentObject = dictionaryDescriptor.GetValue(currentObject, key);
                        }
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return memberPath;
        }

        [NotNull]
        public static YamlAssetPath FromMemberPath(MemberPath path, object root)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            var result = new YamlAssetPath();
            var clone = new MemberPath();
            foreach (var item in path.Decompose())
            {
                if (item.MemberDescriptor != null)
                {
                    clone.Push(item.MemberDescriptor);
                    var member = item.MemberDescriptor.Name;
                    result.PushMember(member);
                }
                else
                {
                    object index = null;
                    var arrayItem = item as MemberPath.ArrayPathItem;
                    if (arrayItem != null)
                    {
                        clone.Push(arrayItem.Descriptor, arrayItem.Index);
                        index = arrayItem.Index;
                    }
                    var collectionItem = item as MemberPath.CollectionPathItem;
                    if (collectionItem != null)
                    {
                        clone.Push(collectionItem.Descriptor, collectionItem.Index);
                        index = collectionItem.Index;
                    }
                    var dictionaryItem = item as MemberPath.DictionaryPathItem;
                    if (dictionaryItem != null)
                    {
                        clone.Push(dictionaryItem.Descriptor, dictionaryItem.Key);
                        index = dictionaryItem.Key;
                    }
                    CollectionItemIdentifiers ids;
                    if (!CollectionItemIdHelper.TryGetCollectionItemIds(clone.GetValue(root), out ids))
                    {
                        result.PushIndex(index);
                    }
                    else
                    {
                        var id = ids[index];
                        // Create a new id if we don't have any so far
                        if (id == ItemId.Empty)
                            id = ItemId.New();
                        result.PushItemId(id);
                    }
                }
            }
            return result;
        }

        public bool Equals(YamlAssetPath other)
        {
            if (Items.Count != other?.Items.Count)
                return false;

            for (var i = 0; i < Items.Count; ++i)
            {
                if (!Items[i].Equals(other.Items[i]))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((YamlAssetPath)obj);
        }

        public override int GetHashCode()
        {
            return items.Aggregate(0, (hashCode, item) => (hashCode * 397) ^ item.GetHashCode());
        }

        public static bool operator ==(YamlAssetPath left, YamlAssetPath right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(YamlAssetPath left, YamlAssetPath right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(object)");
            foreach (var item in items)
            {
                switch (item.Type)
                {
                    case ItemType.Member:
                        sb.Append('.');
                        sb.Append(item.Value);
                        break;
                    case ItemType.Index:
                        sb.Append('[');
                        sb.Append(item.Value);
                        sb.Append(']');
                        break;
                    case ItemType.ItemId:
                        sb.Append('{');
                        sb.Append(item.Value);
                        sb.Append('}');
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return sb.ToString();
        }

        public static bool IsCollectionWithIdType(Type type, object key, out ItemId id, out object actualKey)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(CollectionWithItemIds<>))
                {
                    id = (ItemId)key;
                    actualKey = key;
                    return true;
                }
                if (type.GetGenericTypeDefinition() == typeof(DictionaryWithItemIds<,>))
                {
                    var keyWithId = (IKeyWithId)key;
                    id = keyWithId.Id;
                    actualKey = keyWithId.Key;
                    return true;
                }
            }

            id = ItemId.Empty;
            actualKey = key;
            return false;
        }

        public static bool IsCollectionWithIdType(Type type, object key, out ItemId id)
        {
            object actualKey;
            return IsCollectionWithIdType(type, key, out id, out actualKey);
        }

        [NotNull, Pure]
        public YamlAssetPath Append([CanBeNull] YamlAssetPath other)
        {
            var result = new YamlAssetPath();
            result.items.AddRange(items);
            if (other != null)
            {
                result.items.AddRange(other.items);
            }
            return result;
        }
    }
}
