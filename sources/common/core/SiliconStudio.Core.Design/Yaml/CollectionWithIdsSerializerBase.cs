using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Descriptors;
using SiliconStudio.Core.Yaml.Serialization.Serializers;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A base class to serialize collections with unique identifiers for each item.
    /// </summary>
    public abstract class CollectionWithIdsSerializerBase : DictionarySerializer
    {
        /// <summary>
        /// A key that identifies the information about the instance that we need the store in the <see cref="ObjectContext.Properties"/> dictionary.
        /// </summary>
        protected static readonly object InstanceInfoKey = new object();

        /// <summary>
        /// A structure containing the information about the instance that we need the store in the <see cref="ObjectContext.Properties"/> dictionary. 
        /// </summary>
        protected class InstanceInfo
        {
            public InstanceInfo(object instance, ITypeDescriptor typeDescriptor)
            {               
                Instance = instance;
                Descriptor = typeDescriptor;
            }
            public readonly object Instance;
            public readonly ITypeDescriptor Descriptor;
        }

        public override object ReadYaml(ref ObjectContext objectContext)
        {
            // Create or transform the value to deserialize
            // If the new value to serialize is not the same as the one we were expecting to serialize
            CreateOrTransformObject(ref objectContext);
            var newValue = objectContext.Instance;

            var transformed = false;
            if (newValue != null && newValue.GetType() != objectContext.Descriptor.Type)
            {
                transformed = true;
                objectContext.Descriptor = objectContext.SerializerContext.FindTypeDescriptor(newValue.GetType());
            }

            ReadYamlAfterTransform(ref objectContext, transformed);

            TransformObjectAfterRead(ref objectContext);

            // Process members
            return objectContext.Instance;
        }

        public override void WriteYaml(ref ObjectContext objectContext)
        {
            // TODO: the API could be changed at the ObjectSerializer level so we can, through override:
            // TODO: - customize what to do -if- the object got transformed (currently: ObjectSerializer = routing, here = keep same serializer)
            // TODO: - override WriteYamlAfterTransform without overriding WriteYaml
            // TODO: - and similar with reading

            CreateOrTransformObject(ref objectContext);
            var newValue = objectContext.Instance;

            var transformed = false;
            if (newValue != null && newValue.GetType() != objectContext.Descriptor.Type)
            {
                transformed = true;
                objectContext.Descriptor = objectContext.SerializerContext.FindTypeDescriptor(newValue.GetType());
            }

            WriteYamlAfterTransform(ref objectContext, transformed);
        }

        protected virtual void ReadYamlAfterTransform(ref ObjectContext objectContext, bool transformed)
        {
            ReadMembers<MappingStart, MappingEnd>(ref objectContext);
        }

        protected virtual void WriteYamlAfterTransform(ref ObjectContext objectContext, bool transformed)
        {
            var type = objectContext.Instance.GetType();
            var context = objectContext.SerializerContext;

            // Resolve the style, use default style if not defined.
            var style = GetStyle(ref objectContext);

            context.Writer.Emit(new MappingStartEventInfo(objectContext.Instance, type) { Tag = objectContext.Tag, Anchor = objectContext.Anchor, Style = style });
            WriteMembers(ref objectContext);
            context.Writer.Emit(new MappingEndEventInfo(objectContext.Instance, type));
        }
        /// <inheritdoc/>
        protected override void CreateOrTransformObject(ref ObjectContext objectContext)
        {
            // Allow to deserialize the old way
            if (!objectContext.SerializerContext.IsSerializing && objectContext.Reader.Accept<SequenceStart>())
            {
                base.CreateOrTransformObject(ref objectContext);
                return;
            }

            // Ignore collections flagged as having non-identifiable items
            if (!AreCollectionItemsIdentifiable(ref objectContext))
            {
                base.CreateOrTransformObject(ref objectContext);
                return;
            }

            // Store the information on the actual instance before transforming.
            var info = new InstanceInfo(objectContext.Instance, objectContext.Descriptor);
            objectContext.Properties.Add(InstanceInfoKey, info);

            if (objectContext.SerializerContext.IsSerializing && objectContext.Instance != null)
            {
                // We're serializing, transform the collection to a dictionary of <id, items>
                objectContext.Instance = TransformForSerialization(objectContext.Descriptor, objectContext.Instance);
            }
            else
            {
                // We're deserializing, create an empty dictionary of <id, items>
                objectContext.Instance = CreatEmptyContainer(objectContext.Descriptor);
            }
        }

        protected override void WriteDictionaryItems(ref ObjectContext objectContext)
        {
            var dictionaryDescriptor = (DictionaryDescriptor)objectContext.Descriptor;
            var keyValues = dictionaryDescriptor.GetEnumerator(objectContext.Instance).ToList();

            // Not sorting the keys here, they should be already properly sorted when we arrive here
            // TODO: Allow to disable sorting externally, to avoid overriding this method. NOTE: tampering with Settings.SortKeyForMapping is not an option, it is not local but applied to all children. (ParameterKeyDictionarySerializer is doing that and is buggy)

            var keyValueType = new KeyValuePair<Type, Type>(dictionaryDescriptor.KeyType, dictionaryDescriptor.ValueType);

            foreach (var keyValue in keyValues)
            {
                WriteDictionaryItem(ref objectContext, keyValue, keyValueType);
            }
        }

        protected static bool AreCollectionItemsIdentifiable(ref ObjectContext objectContext)
        {
            object nonIdentifiableItems;

            // Check in the serializer context first, for disabling of item identifiers at parent type level
            if (objectContext.SerializerContext.Properties.TryGetValue(NonIdentifiableCollectionItemsAttribute.Key, out nonIdentifiableItems) && (bool)nonIdentifiableItems)
                return false;

            // Then check locally for disabling of item identifiers at member level
            if (objectContext.Properties.TryGetValue(NonIdentifiableCollectionItemsAttribute.Key, out nonIdentifiableItems) && (bool)nonIdentifiableItems)
                return false;

            return true;
        }

        protected override bool CheckIsSequence(ref ObjectContext objectContext)
        {
            var collectionDescriptor = objectContext.Descriptor as CollectionDescriptor;

            // If the dictionary is pure, we can directly output a sequence instead of a mapping
            return collectionDescriptor != null && collectionDescriptor.IsPureCollection;
        }

        /// <summary>
        /// Transforms the given collection or dictionary into a dictionary of (ids, items) or a dictionary of (ids & keys, items).
        /// </summary>
        /// <param name="descriptor">The type descriptor of the collection.</param>
        /// <param name="collection">The collection for which to create the mapping dictionary.</param>
        /// <returns>A dictionary mapping the id to the element of the initial collection.</returns>
        protected abstract object TransformForSerialization(ITypeDescriptor descriptor, object collection);

        /// <summary>
        /// Creates an empty dictionary that can store the mapping of ids to items of the collection.
        /// </summary>
        /// <param name="descriptor">The type descriptor of the collection for which to create the dictionary.</param>
        /// <returns>An empty dictionary for mapping ids to elements.</returns>
        protected abstract IDictionary CreatEmptyContainer(ITypeDescriptor descriptor);

        /// <summary>
        /// Transforms a dictionary containing the mapping of ids to items into the actual collection, and store the ids in the <see cref="Reflection.ShadowObject"/>.
        /// </summary>
        /// <param name="container">The dictionary mapping ids to item.</param>
        /// <param name="targetDescriptor">The type descriptor of the actual collection to fill.</param>
        /// <param name="targetCollection">The instance of the actual collection to fill.</param>
        protected abstract void TransformAfterDeserialization(IDictionary container, ITypeDescriptor targetDescriptor, object targetCollection);
    }
}
