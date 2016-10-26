using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.Core.Yaml
{
    [DataContract]
    public class ObjectPath : IEquatable<ObjectPath> // TODO: Rename this possibly, it's really tied to collection with ids and override
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
            public Identifier AsItemId() { if (Type != ItemType.ItemId) throw new InvalidOperationException("This item is not a item Id"); return (Identifier)Value; }

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

        public void PushItemId(Identifier itemId)
        {
            items.Add(new Item(ItemType.ItemId, itemId));
        }

        public ObjectPath Clone()
        {
            var clone = new ObjectPath();
            clone.items.AddRange(items);
            return clone;
        }

        public bool Equals(ObjectPath other)
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
            return Equals((ObjectPath)obj);
        }

        public override int GetHashCode()
        {
            return items.Aggregate(0, (hashCode, item) => (hashCode * 397) ^ item.GetHashCode());
        }

        public static bool operator ==(ObjectPath left, ObjectPath right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ObjectPath left, ObjectPath right)
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

        public static bool IsCollectionWithIdType(Type type, object key, out Identifier id)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(CollectionWithItemIds<>))
                {
                    id = (Identifier)key;
                    return true;
                }
                if (type.GetGenericTypeDefinition() == typeof(DictionaryWithItemIds<,>))
                {
                    id = ((IKeyWithId)key).Id;
                    return true;
                }
            }

            id = Identifier.Empty;
            return false;
        }
    }
}
