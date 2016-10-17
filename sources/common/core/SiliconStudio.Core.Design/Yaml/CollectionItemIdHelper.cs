using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Serializers;
using DictionaryDescriptor = SiliconStudio.Core.Yaml.Serialization.Descriptors.DictionaryDescriptor;
using ITypeDescriptor = SiliconStudio.Core.Yaml.Serialization.ITypeDescriptor;

namespace SiliconStudio.Core.Yaml
{
    public class CollectionWithItemIdsSerializer : DictionarySerializer
    {
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            return typeDescriptor.Type.IsGenericType && typeDescriptor.Type.GetGenericTypeDefinition() == typeof(CollectionWithItemIds<>) ? this : null;
        }

        protected override void WriteDictionaryItems(ref ObjectContext objectContext)
        {
            var dictionaryDescriptor = (DictionaryDescriptor)objectContext.Descriptor;
            var keyValues = dictionaryDescriptor.GetEnumerator(objectContext.Instance).ToList();
            
            // Not sorting the keys here, they should be already properly sorted when we arrive here

            var keyValueType = new KeyValuePair<Type, Type>(dictionaryDescriptor.KeyType, dictionaryDescriptor.ValueType);

            foreach (var keyValue in keyValues)
            {
                WriteDictionaryItem(ref objectContext, keyValue, keyValueType);
            }
        }
    }

    [DataContract]
    public class CollectionWithItemIds<TItem> : OrderedDictionary<Guid, TItem>
    {

    }

    [DataContract]
    public class DictionaryWithItemIds<TKey, TValue> : OrderedDictionary<KeyWithId, TValue>
    {

    }

    public class CollectionItemIdentifiers
    {
        public OrderedDictionary<object, Guid> KeyToIdMap { get; } = new OrderedDictionary<object, Guid>();

        public List<Guid> DeletedItems { get; } = new List<Guid>();
    }

    public struct KeyWithId
    {
        public KeyWithId(Guid id, object key)
        {
            Id = id;
            Key = key;
        }
        public readonly Guid Id;
        public object Key;
    }

    public static class CollectionItemIdHelper
    {
        // TODO: move to Asset level
        public const string YamlSpecialId = "~ItemId";

        public const string YamlDeletedKey = "(~Deleted)";

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

        public static object TransformForSerialization(ITypeDescriptor descriptor, object collection)
        {
            var collectionDescriptor = descriptor as Serialization.Descriptors.CollectionDescriptor;
            var dictionaryDescriptor = descriptor as Serialization.Descriptors.DictionaryDescriptor;
            if (collectionDescriptor != null)
            {
                var type = typeof(CollectionWithItemIds<>).MakeGenericType(collectionDescriptor.ElementType);
                if (type.GetConstructor(Type.EmptyTypes) == null)
                    throw new InvalidOperationException("The type of collection does not have a parameterless constructor.");
                var instance = (IDictionary)Activator.CreateInstance(type);
                var identifier = GetCollectionItemIds(collection);
                var i = 0;
                foreach (var item in (IEnumerable)collection)
                {
                    Guid id;
                    if (!identifier.KeyToIdMap.TryGetValue(i, out id))
                    {
                        id = Guid.NewGuid();
                    }
                    instance.Add(id, item);
                    ++i;
                }

                return instance;
            }
            if (dictionaryDescriptor != null)
            {
                var type = typeof(DictionaryWithItemIds<,>).MakeGenericType(dictionaryDescriptor.KeyType, dictionaryDescriptor.ValueType);
                if (type.GetConstructor(Type.EmptyTypes) == null)
                    throw new InvalidOperationException("The type of dictionary does not have a parameterless constructor.");
                var instance = (IDictionary)Activator.CreateInstance(type);
                var identifier = GetCollectionItemIds(collection);
                var i = 0;
                foreach (var item in dictionaryDescriptor.GetEnumerator(collection))
                {
                    Guid id;
                    if (!identifier.KeyToIdMap.TryGetValue(i, out id))
                    {
                        id = Guid.NewGuid();
                    }
                    var keyWithId = new KeyWithId(id, item.Key);
                    instance.Add(keyWithId, item.Value);
                    ++i;
                }

                return instance;
            }

            throw new InvalidOperationException("The given object is not a collection or a dictionary");
        }

        public static object CreatEmptyContainer(ITypeDescriptor descriptor)
        {
            var collectionDescriptor = descriptor as Serialization.Descriptors.CollectionDescriptor;
            var dictionaryDescriptor = descriptor as Serialization.Descriptors.DictionaryDescriptor;
            if (collectionDescriptor != null)
            {
                var type = typeof(CollectionWithItemIds<>).MakeGenericType(collectionDescriptor.ElementType);
                if (type.GetConstructor(Type.EmptyTypes) == null) throw new InvalidOperationException("The type of collection does not have a parameterless constructor.");
                return (IDictionary)Activator.CreateInstance(type);
            }
            if (dictionaryDescriptor != null)
            {
                var type = typeof(DictionaryWithItemIds<,>).MakeGenericType(dictionaryDescriptor.KeyType, dictionaryDescriptor.ValueType);
                if (type.GetConstructor(Type.EmptyTypes) == null) throw new InvalidOperationException("The type of dictionary does not have a parameterless constructor.");
                return (IDictionary)Activator.CreateInstance(type);
            }

            throw new InvalidOperationException("The given object is not a collection or a dictionary");
        }

        public static void TransformAfterDeserialization(object container, ITypeDescriptor targetDescriptor, object targetCollection)
        {
            var collectionDescriptor = targetDescriptor as Serialization.Descriptors.CollectionDescriptor;
            var dictionaryDescriptor = targetDescriptor as Serialization.Descriptors.DictionaryDescriptor;
            if (collectionDescriptor != null)
            {
                var type = typeof(CollectionWithItemIds<>).MakeGenericType(collectionDescriptor.ElementType);
                if (!type.IsInstanceOfType(container))
                    throw new InvalidOperationException("The given container does not match the expected type.");
                var identifier = GetCollectionItemIds(targetCollection);
                identifier.KeyToIdMap.Clear();
                var i = 0;
                var enumerator = ((IDictionary)container).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    collectionDescriptor.CollectionAdd(targetCollection, enumerator.Value);
                    identifier.KeyToIdMap.Add(i, (Guid)enumerator.Key);
                    ++i;
                }
            }
            else if (dictionaryDescriptor != null)
            {
                var type = typeof(DictionaryWithItemIds<,>).MakeGenericType(dictionaryDescriptor.KeyType, dictionaryDescriptor.ValueType);
                if (!type.IsInstanceOfType(container))
                    throw new InvalidOperationException("The given container does not match the expected type.");
                var identifier = GetCollectionItemIds(targetCollection);
                identifier.KeyToIdMap.Clear();
                var enumerator = ((IDictionary)container).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var keyWithId = (KeyWithId)enumerator.Key;
                    dictionaryDescriptor.AddToDictionary(targetCollection, keyWithId.Key, enumerator.Value);
                    identifier.KeyToIdMap.Add(keyWithId.Key, keyWithId.Id);
                }
            }
            else
            {
                throw new InvalidOperationException("The given object is not a collection or a dictionary");
            }
        }
    }
}
