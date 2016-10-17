using System.Reflection;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Serializers;

namespace SiliconStudio.Core.Yaml
{
    public class CollectionWithIdsSerializer : CollectionSerializer
    {
        private struct InstanceInfo
        {
            public object Instance;
            public Serialization.Descriptors.CollectionDescriptor Descriptor;
        }

        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            if (typeDescriptor is Serialization.Descriptors.CollectionDescriptor)
            {
                var dataStyle = typeDescriptor.Type.GetCustomAttribute<DataStyleAttribute>();
                if (dataStyle == null || dataStyle.Style != DataStyle.Compact)
                    return this;
            }
            return null;
        }

        protected override void CreateOrTransformObject(ref ObjectContext objectContext)
        {
            // Allow to deserialize the old way
            if (!objectContext.SerializerContext.IsSerializing && objectContext.Reader.Accept<SequenceStart>())
            {
                base.CreateOrTransformObject(ref objectContext);
                return;
            }

            if (!AreCollectionItemsIdentifiable(ref objectContext))
            {
                base.CreateOrTransformObject(ref objectContext);
                return;
            }

            var info = new InstanceInfo { Instance = objectContext.Instance, Descriptor = (Serialization.Descriptors.CollectionDescriptor)objectContext.Descriptor };
            objectContext.Properties.Add("InstanceInfo", info);
            //IDictionary<Guid, object> ids;
            if (objectContext.SerializerContext.IsSerializing && objectContext.Instance != null)
            {
                //var collection = (ICollection)objectContext.Instance;
                //ids = CollectionItemIdHelper.GetItemIds(objectContext.Instance);
                objectContext.Instance = CollectionItemIdHelper.TransformForSerialization(objectContext.Descriptor, objectContext.Instance);
            }
            else
            {
                objectContext.Instance = CollectionItemIdHelper.CreatEmptyContainer(objectContext.Descriptor);
            }
            //objectContext.Instance = CollectionItemIdHelper.ToTypedDictionary(ids, info.Descriptor.ElementType);
        }

        protected override void TransformObjectAfterRead(ref ObjectContext objectContext)
        {
            object infoObject;
            if (!objectContext.Properties.TryGetValue("InstanceInfo", out infoObject))
            {
                base.TransformObjectAfterRead(ref objectContext);
                return;
            }
            var info = (InstanceInfo)infoObject;

            if (info.Instance != null)
            {
                //var dictionary = CollectionItemIdHelper.GetItemIds(info.Instance);
                //dictionary.Clear();
                //var ids = CollectionItemIdHelper.ToObjectDictionary((IDictionary)objectContext.Instance);
                //foreach (var id in ids)
                //{
                //    info.Descriptor.CollectionAdd(info.Instance, id.Value);
                //    dictionary.Add(id.Key, id.Value);
                //}
                CollectionItemIdHelper.TransformAfterDeserialization(objectContext.Instance, info.Descriptor, info.Instance);
            }
            objectContext.Instance = info.Instance;
            base.TransformObjectAfterRead(ref objectContext);
        }

        private static bool AreCollectionItemsIdentifiable(ref ObjectContext objectContext)
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
    }
}