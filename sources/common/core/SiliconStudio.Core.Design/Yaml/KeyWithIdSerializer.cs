using System;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Serializers;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A Yaml serializer for <see cref="IKeyWithId"/>.
    /// </summary>
    [YamlSerializerFactory]
    public class KeyWithIdSerializer : ItemIdSerializerBase
    {
        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override bool CanVisit(Type type)
        {
            return typeof(IKeyWithId).IsAssignableFrom(type);
        }
    }
}
