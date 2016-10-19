using System;
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Descriptors;
using SiliconStudio.Core.Yaml.Serialization.Serializers;

namespace SiliconStudio.Core.Yaml
{
    [DataContract]
    public class CollectionWithItemIds<TItem> : OrderedDictionary<Guid, TItem>
    {

    }

    [DataContract]
    public class DictionaryWithItemIds<TKey, TValue> : OrderedDictionary<KeyWithId<TKey>, TValue>
    {

    }

    public class CollectionItemIdentifiers
    {
        // TODO: we could sort only at serialization
        private readonly SortedList<object, Guid> keyToIdMap = new SortedList<object, Guid>(new DefaultKeyComparer());
        private readonly HashSet<Guid> deletedItems = new HashSet<Guid>();

        public Guid this[object key] { get { return keyToIdMap[key]; } set { keyToIdMap[key] = value; } }

        public IEnumerable<Guid> DeletedItems => deletedItems;

        public int KeyCount => keyToIdMap.Count;

        public int DeletedCount => deletedItems.Count;

        public int Count => KeyCount + DeletedCount;

        public void Add(object key, Guid id)
        {
            keyToIdMap.Add(key, id);
        }

        public void Insert(int index, Guid id)
        {
            for (var i = keyToIdMap.Count; i > index; --i)
            {
                keyToIdMap[i] = keyToIdMap[i-1];

            }
            keyToIdMap.Add(index, id);
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

        public bool TryGet(object key, out Guid id)
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

        public void MarkAsDeleted(Guid id)
        {
            deletedItems.Add(id);
        }

        public void UnmarkAsDeleted(Guid id)
        {
            deletedItems.Remove(id);
        }

        public void Validate(bool isList)
        {
            var ids = new HashSet<Guid>(keyToIdMap.Values);
            if (ids.Count != keyToIdMap.Count)
                throw new InvalidOperationException("Two elements of the collection have the same id");

            foreach (var deleted in deletedItems)
                ids.Add(deleted);

            if (ids.Count != keyToIdMap.Count + deletedItems.Count)
                throw new InvalidOperationException("An id is both marked as deleted and associated to a key of the collection.");
        }
    }

    public interface IKeyWithId
    {
        Guid Id { get; }
        object Key { get; }
    }

    public struct KeyWithId<TKey> : IKeyWithId
    {
        public KeyWithId(Guid id, TKey key)
        {
            Id = id;
            Key = key;
        }
        public readonly Guid Id;
        public TKey Key;
        Guid IKeyWithId.Id => Id;
        object IKeyWithId.Key => Key;
    }

    public class KeyWithIdSerializer : ScalarSerializerBase, IYamlSerializableFactory
    {
        public override object ConvertFrom(ref ObjectContext objectContext, Scalar fromScalar)
        {
            var idIndex = fromScalar.Value.IndexOf('~');
            var id = Guid.Empty;
            var keyString = fromScalar.Value;
            if (idIndex >= 0)
            {
                var idString = fromScalar.Value.Substring(0, idIndex);
                keyString = fromScalar.Value.Substring(idIndex + 1);
                id = Guid.Parse(idString);
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
            var keyDescriptor = objectContext.SerializerContext.FindTypeDescriptor(key.Key.GetType());
            var keySerializer = objectContext.SerializerContext.Serializer.GetSerializer(objectContext.SerializerContext, keyDescriptor);

            // TODO: serialize non-scalar keys!
            // Guid:
            //     Key: {Key}
            //     Value: {Value}

            var scalarKeySerializer = keySerializer as ScalarSerializerBase;
            if (scalarKeySerializer == null)
                throw new InvalidOperationException("Non-scalar key not yet supported!");

            var context = new ObjectContext(objectContext.SerializerContext, key.Key, keyDescriptor);
            var keyString = scalarKeySerializer.ConvertTo(ref context);
            return $"{key.Id}~{keyString}";
        }

        public IYamlSerializable TryCreate(SerializerContext context, IYamlTypeDescriptor typeDescriptor)
        {
            return typeDescriptor.Type.IsGenericType && typeDescriptor.Type.GetGenericTypeDefinition() == typeof(KeyWithId<>) ? this : null;
        }
    }

    public static class CollectionItemIdHelper
    {
        public static readonly object DeletedKey = new object();

        // TODO: do we really need to pass an object to this constructor?
        public static ShadowObjectPropertyKey CollectionItemIdKey = new ShadowObjectPropertyKey(new object());

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
            return true;
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
