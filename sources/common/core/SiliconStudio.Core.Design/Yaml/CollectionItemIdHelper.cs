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
    public struct Identifier : IComparable<Identifier>, IEquatable<Identifier>
    {
        private readonly ObjectId Value;

        public Identifier(byte[] bytes)
        {
            Value = new ObjectId(bytes);
        }

        internal Identifier(ObjectId id)
        {
            Value = id;
        }

        public static Identifier Empty { get; } = new Identifier(ObjectId.Empty);

        public static Identifier New()
        {
            return new Identifier(ObjectId.New());
        }

        public bool Equals(Identifier other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is Identifier && Equals((Identifier)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static bool operator ==(Identifier left, Identifier right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Identifier left, Identifier right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(Identifier other)
        {
            return Value.CompareTo(other.Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static Identifier Parse(string input)
        {
            ObjectId objectId;
            if (!ObjectId.TryParse(input, out objectId))
                throw new ArgumentException("Unable to parse input string.");
            return new Identifier(objectId);
        }
    }

    /// <summary>
    /// A Yaml serializer for <see cref="Guid"/>
    /// </summary>
    [YamlSerializerFactory]
    internal class IdentifierSerializer : AssetScalarSerializerBase
    {
        public static PropertyKey<string> OverrideInfoKey = new PropertyKey<string>("OverrideInfo", typeof(IdentifierSerializer));
        public override bool CanVisit(Type type)
        {
            return type == typeof(Identifier);
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            ObjectId id;
            ObjectId.TryParse(fromScalar.Value, out id);
            return new Identifier(id);
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            return ((Identifier)objectContext.Instance).ToString();
        }

        protected override void WriteScalar(ref ObjectContext objectContext, ScalarEventInfo scalar)
        {
            string overrideInfo;
            if (objectContext.Properties.TryGetValue(OverrideInfoKey, out overrideInfo))
            {
                scalar.RenderedValue += overrideInfo;
            }

            base.WriteScalar(ref objectContext, scalar);
        }
    }

    [DataContract]
    public class CollectionWithItemIds<TItem> : OrderedDictionary<Identifier, TItem>
    {

    }

    [DataContract]
    public class DictionaryWithItemIds<TKey, TValue> : OrderedDictionary<KeyWithId<TKey>, TValue>
    {

    }

    public class CollectionItemIdentifiers
    {
        // TODO: we could sort only at serialization
        private readonly SortedList<object, Identifier> keyToIdMap = new SortedList<object, Identifier>(new DefaultKeyComparer());
        private readonly HashSet<Identifier> deletedItems = new HashSet<Identifier>();

        public Identifier this[object key] { get { return keyToIdMap[key]; } set { keyToIdMap[key] = value; } }

        public IEnumerable<Identifier> DeletedItems => deletedItems;

        public int KeyCount => keyToIdMap.Count;

        public int DeletedCount => deletedItems.Count;

        public int Count => KeyCount + DeletedCount;

        public void Add(object key, Identifier id)
        {
            keyToIdMap.Add(key, id);
        }

        public void Insert(int index, Identifier id)
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

        public bool TryGet(object key, out Identifier id)
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
            keyToIdMap.Remove(index);
            for (var i = index + 1; i < keyToIdMap.Count; ++i)
            {
                keyToIdMap[i - 1] = keyToIdMap[i];
                keyToIdMap.Remove(i);
            }
            if (markAsDeleted)
            {
                MarkAsDeleted(id);
            }
        }

        public void MarkAsDeleted(Identifier id)
        {
            deletedItems.Add(id);
        }

        public void UnmarkAsDeleted(Identifier id)
        {
            deletedItems.Remove(id);
        }

        public void Validate(bool isList)
        {
            var ids = new HashSet<Identifier>(keyToIdMap.Values);
            if (ids.Count != keyToIdMap.Count)
                throw new InvalidOperationException("Two elements of the collection have the same id");

            foreach (var deleted in deletedItems)
                ids.Add(deleted);

            if (ids.Count != keyToIdMap.Count + deletedItems.Count)
                throw new InvalidOperationException("An id is both marked as deleted and associated to a key of the collection.");
        }

        public Identifier FindMissingId(CollectionItemIdentifiers baseIds)
        {
            var hashSet = new HashSet<Identifier>(deletedItems);
            foreach (var item in keyToIdMap)
            {
                hashSet.Add(item.Value);
            }

            var missingId = Identifier.Empty;
            foreach (var item in baseIds.keyToIdMap)
            {
                if (!hashSet.Contains(item.Value))
                {
                    // TODO: if we have scenario where this is ok, I guess we can just return the first one.
                    if (missingId != Identifier.Empty)
                        throw new InvalidOperationException("Multiple ids are missing.");

                    missingId = item.Value;
                }
            }

            foreach (var item in baseIds.deletedItems)
            {
                if (!hashSet.Contains(item))
                {
                    // TODO: if we have scenario where this is ok, I guess we can just return the first one.
                    if (missingId != Identifier.Empty)
                        throw new InvalidOperationException("Multiple ids are missing.");

                    missingId = item;
                }
            }

            return missingId;
        }

        public object GetKey(Identifier identifier)
        {
            // TODO: add indexing by guid to avoid O(n)
            return keyToIdMap.FirstOrDefault(x => x.Value == identifier).Key;
        }

        public Identifier GetId(object key)
        {
            return keyToIdMap.FirstOrDefault(x => Equals(x.Key, key)).Value;
        }
    }

    public interface IKeyWithId
    {
        Identifier Id { get; }
        object Key { get; }
        Type KeyType { get; }
        bool IsDeleted { get; }
    }

    public struct KeyWithId<TKey> : IKeyWithId
    {
        public KeyWithId(Identifier id, TKey key)
        {
            Id = id;
            Key = key;
        }
        public readonly Identifier Id;
        public TKey Key;
        Identifier IKeyWithId.Id => Id;
        object IKeyWithId.Key => Key;
        bool IKeyWithId.IsDeleted => false;
        Type IKeyWithId.KeyType => typeof(TKey);
    }

    public struct DeletedKeyWithId<TKey> : IKeyWithId
    {
        public DeletedKeyWithId(Identifier id)
        {
            Id = id;
        }
        public readonly Identifier Id;
        public TKey Key => default(TKey);
        Identifier IKeyWithId.Id => Id;
        object IKeyWithId.Key => Key;
        bool IKeyWithId.IsDeleted => true;
        Type IKeyWithId.KeyType => typeof(TKey);
    }

    public class KeyWithIdSerializer : ScalarSerializerBase, IYamlSerializableFactory
    {
        public override object ConvertFrom(ref ObjectContext objectContext, Scalar fromScalar)
        {
            var idIndex = fromScalar.Value.IndexOf('~');
            var id = Identifier.Empty;
            var keyString = fromScalar.Value;
            if (idIndex >= 0)
            {
                var idString = fromScalar.Value.Substring(0, idIndex);
                keyString = fromScalar.Value.Substring(idIndex + 1);
                id = Identifier.Parse(idString);
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

        public IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            if (typeDescriptor.Type.IsGenericType && typeDescriptor.Type.GetGenericTypeDefinition() == typeof(KeyWithId<>))
                return this;
            if (typeDescriptor.Type.IsGenericType && typeDescriptor.Type.GetGenericTypeDefinition() == typeof(DeletedKeyWithId<>))
                return this;

            return null;
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
