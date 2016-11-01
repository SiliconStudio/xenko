using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Storage;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Serializers;

namespace SiliconStudio.Core.Yaml
{
    [DataContract]
    public struct ItemId : IComparable<ItemId>, IEquatable<ItemId>
    {
        private readonly ObjectId Value;

        public ItemId(byte[] bytes)
        {
            Value = new ObjectId(bytes);
        }

        internal ItemId(ObjectId id)
        {
            Value = id;
        }

        public static ItemId Empty { get; } = new ItemId(ObjectId.Empty);

        public static ItemId New()
        {
            return new ItemId(ObjectId.New());
        }

        public bool Equals(ItemId other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is ItemId && Equals((ItemId)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(ItemId left, ItemId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ItemId left, ItemId right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(ItemId other)
        {
            return Value.CompareTo(other.Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static ItemId Parse(string input)
        {
            ObjectId objectId;
            if (!ObjectId.TryParse(input, out objectId))
                throw new ArgumentException("Unable to parse input string.");
            return new ItemId(objectId);
        }
    }

    /// <summary>
    /// A Yaml serializer for <see cref="ItemId"/>
    /// </summary>
    [YamlSerializerFactory]
    internal class ItemIdSerializer : ItemIdSerializerBase
    {
        public override bool CanVisit(Type type)
        {
            return type == typeof(ItemId);
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            ObjectId id;
            ObjectId.TryParse(fromScalar.Value, out id);
            return new ItemId(id);
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            return ((ItemId)objectContext.Instance).ToString();
        }
    }

    [DataContract]
    public class CollectionWithItemIds<TItem> : OrderedDictionary<ItemId, TItem>
    {

    }

    [DataContract]
    public class DictionaryWithItemIds<TKey, TValue> : OrderedDictionary<KeyWithId<TKey>, TValue>
    {

    }

    public class CollectionItemIdentifiers
    {
        // TODO: we could sort only at serialization
        private readonly SortedList<object, ItemId> keyToIdMap = new SortedList<object, ItemId>(new DefaultKeyComparer());
        private readonly HashSet<ItemId> deletedItems = new HashSet<ItemId>();

        public ItemId this[object key] { get { return keyToIdMap[key]; } set { keyToIdMap[key] = value; } }

        public IEnumerable<ItemId> DeletedItems => deletedItems;

        public int KeyCount => keyToIdMap.Count;

        public int DeletedCount => deletedItems.Count;

        public int Count => KeyCount + DeletedCount;

        public void Add(object key, ItemId id)
        {
            keyToIdMap.Add(key, id);
        }

        public void Insert(int index, ItemId id)
        {
            for (var i = keyToIdMap.Count; i > index; --i)
            {
                keyToIdMap[i] = keyToIdMap[i-1];

            }
            keyToIdMap[index] = id;
        }

        public void Clear()
        {
            keyToIdMap.Clear();
            deletedItems.Clear();
        }

        public bool ContainsKey(object key)
        {
            return keyToIdMap.ContainsKey(key);
        }

        public bool TryGet(object key, out ItemId id)
        {
            return keyToIdMap.TryGetValue(key, out id);
        }

        public void Delete(object key, bool markAsDeleted = true)
        {
            var id = keyToIdMap[key];
            keyToIdMap.Remove(key);
            if (markAsDeleted)
            {
                MarkAsDeleted(id);
            }
        }

        public void DeleteAndShift(int index, bool markAsDeleted = true)
        {
            var id = keyToIdMap[index];
            for (var i = index + 1; i < keyToIdMap.Count; ++i)
            {
                keyToIdMap[i - 1] = keyToIdMap[i];
            }
            keyToIdMap.Remove(keyToIdMap.Count - 1);

            if (markAsDeleted)
            {
                MarkAsDeleted(id);
            }
        }

        public void MarkAsDeleted(ItemId id)
        {
            deletedItems.Add(id);
        }

        public void UnmarkAsDeleted(ItemId id)
        {
            deletedItems.Remove(id);
        }

        public void Validate(bool isList)
        {
            var ids = new HashSet<ItemId>(keyToIdMap.Values);
            if (ids.Count != keyToIdMap.Count)
                throw new InvalidOperationException("Two elements of the collection have the same id");

            foreach (var deleted in deletedItems)
                ids.Add(deleted);

            if (ids.Count != keyToIdMap.Count + deletedItems.Count)
                throw new InvalidOperationException("An id is both marked as deleted and associated to a key of the collection.");
        }

        public ItemId FindMissingId(CollectionItemIdentifiers baseIds)
        {
            var hashSet = new HashSet<ItemId>(deletedItems);
            foreach (var item in keyToIdMap)
            {
                hashSet.Add(item.Value);
            }

            var missingId = ItemId.Empty;
            foreach (var item in baseIds.keyToIdMap)
            {
                if (!hashSet.Contains(item.Value))
                {
                    // TODO: if we have scenario where this is ok, I guess we can just return the first one.
                    if (missingId != ItemId.Empty)
                        throw new InvalidOperationException("Multiple ids are missing.");

                    missingId = item.Value;
                }
            }

            foreach (var item in baseIds.deletedItems)
            {
                if (!hashSet.Contains(item))
                {
                    // TODO: if we have scenario where this is ok, I guess we can just return the first one.
                    if (missingId != ItemId.Empty)
                        throw new InvalidOperationException("Multiple ids are missing.");

                    missingId = item;
                }
            }

            return missingId;
        }

        public object GetKey(ItemId itemId)
        {
            // TODO: add indexing by guid to avoid O(n)
            return keyToIdMap.FirstOrDefault(x => x.Value == itemId).Key;
        }

        public ItemId GetId(object key)
        {
            return keyToIdMap.FirstOrDefault(x => Equals(x.Key, key)).Value;
        }

        public void CloneInto(CollectionItemIdentifiers target, IReadOnlyDictionary<object, object> referenceTypeClonedKeys)
        {
            target.keyToIdMap.Clear();
            target.deletedItems.Clear();
            foreach (var key in keyToIdMap)
            {
                object clonedKey;
                if (key.Key.GetType().IsValueType)
                {
                    target.Add(key.Key, key.Value);
                }
                else if (referenceTypeClonedKeys != null && referenceTypeClonedKeys.TryGetValue(key.Key, out clonedKey))
                {
                    target.Add(clonedKey, key.Value);
                }
                else
                {
                    throw new KeyNotFoundException("Unable to find the non-value type key in the dictionary of cloned keys.");
                }
            }
        }

        public bool IsDeleted(ItemId itemId)
        {
            return DeletedItems.Contains(itemId);
        }
    }

    public interface IKeyWithId
    {
        ItemId Id { get; }
        object Key { get; }
        Type KeyType { get; }
        bool IsDeleted { get; }
    }

    public struct KeyWithId<TKey> : IKeyWithId
    {
        public KeyWithId(ItemId id, TKey key)
        {
            Id = id;
            Key = key;
        }
        public readonly ItemId Id;
        public TKey Key;
        ItemId IKeyWithId.Id => Id;
        object IKeyWithId.Key => Key;
        bool IKeyWithId.IsDeleted => false;
        Type IKeyWithId.KeyType => typeof(TKey);
    }

    public struct DeletedKeyWithId<TKey> : IKeyWithId
    {
        public DeletedKeyWithId(ItemId id)
        {
            Id = id;
        }
        public readonly ItemId Id;
        public TKey Key => default(TKey);
        ItemId IKeyWithId.Id => Id;
        object IKeyWithId.Key => Key;
        bool IKeyWithId.IsDeleted => true;
        Type IKeyWithId.KeyType => typeof(TKey);
    }

    public abstract class ItemIdSerializerBase : AssetScalarSerializerBase
    {
        public static PropertyKey<string> OverrideInfoKey = new PropertyKey<string>("OverrideInfo", typeof(ItemIdSerializer));

        protected override void WriteScalar(ref ObjectContext objectContext, ScalarEventInfo scalar)
        {
            string overrideInfo;
            if (objectContext.SerializerContext.Properties.TryGetValue(OverrideInfoKey, out overrideInfo))
            {
                scalar.RenderedValue += overrideInfo;
                objectContext.SerializerContext.Properties.Remove(OverrideInfoKey);
            }

            base.WriteScalar(ref objectContext, scalar);
        }
    }

    public class KeyWithIdSerializer : ItemIdSerializerBase, IYamlSerializableFactory
    {
        public override object ConvertFrom(ref ObjectContext objectContext, Scalar fromScalar)
        {
            var idIndex = fromScalar.Value.IndexOf('~');
            var id = ItemId.Empty;
            var keyString = fromScalar.Value;
            if (idIndex >= 0)
            {
                var idString = fromScalar.Value.Substring(0, idIndex);
                keyString = fromScalar.Value.Substring(idIndex + 1);
                id = ItemId.Parse(idString);
            }
            var keyType = objectContext.Descriptor.Type.GetGenericArguments()[0];
            var keyDescriptor = objectContext.SerializerContext.FindTypeDescriptor(keyType);
            var keySerializer = objectContext.SerializerContext.Serializer.GetSerializer(objectContext.SerializerContext, keyDescriptor);
            var scalarKeySerializer = keySerializer as ScalarSerializerBase;
            // TODO: deserialize non-scalar keys!
            if (scalarKeySerializer == null)
                throw new InvalidOperationException("Non-scalar key not yet supported!");

            var context = new ObjectContext(objectContext.SerializerContext, null, keyDescriptor);
            var key = scalarKeySerializer.ConvertFrom(ref context, new Scalar(keyString));
            var result = Activator.CreateInstance(typeof(KeyWithId<>).MakeGenericType(keyType), id, key);
            return result;
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            var key = (IKeyWithId)objectContext.Instance;
            var keyDescriptor = objectContext.SerializerContext.FindTypeDescriptor(key.KeyType);
            var keySerializer = objectContext.SerializerContext.Serializer.GetSerializer(objectContext.SerializerContext, keyDescriptor);

            // TODO: serialize non-scalar keys!
            // Guid:
            //     Key: {Key}
            //     Value: {Value}

            var scalarKeySerializer = keySerializer as ScalarSerializerBase;
            if (scalarKeySerializer == null)
                throw new InvalidOperationException("Non-scalar key not yet supported!");

            var context = new ObjectContext(objectContext.SerializerContext, key.Key, keyDescriptor);

            if (key.IsDeleted)
                return $"{key.Id}~";

            var keyString = scalarKeySerializer.ConvertTo(ref context);
            return $"{key.Id}~{keyString}";
        }

        public override bool CanVisit(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyWithId<>))
                return true;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DeletedKeyWithId<>))
                return true;

            return false;
        }
    }

    public static class CollectionItemIdHelper
    {
        public static readonly object DeletedKey = new object();

        // TODO: do we really need to pass an object to this constructor?
        public static ShadowObjectPropertyKey CollectionItemIdKey = new ShadowObjectPropertyKey(new object());

        public static void GenerateMissingItemIds(object rootObject)
        {
            var visitor = new CollectionIdGenerator();
            visitor.Visit(rootObject);
        }

        public static bool TryGetCollectionItemIds(object instance, out CollectionItemIdentifiers itemIds)
        {
            var shadow = ShadowObject.Get(instance);
            if (shadow == null)
            {
                itemIds = null;
                return false;
            }

            object result;
            itemIds = shadow.TryGetValue(CollectionItemIdKey, out result) ? (CollectionItemIdentifiers)result : null;
            return result != null;
        }

        public static CollectionItemIdentifiers GetCollectionItemIds(object instance)
        {
            var shadow = ShadowObject.GetOrCreate(instance);
            object result;
            if (shadow.TryGetValue(CollectionItemIdKey, out result))
            {
                return (CollectionItemIdentifiers)result;
            }

            var itemIds = new CollectionItemIdentifiers();
            shadow.Add(CollectionItemIdKey, itemIds);
            return itemIds;
        }
    }
}
