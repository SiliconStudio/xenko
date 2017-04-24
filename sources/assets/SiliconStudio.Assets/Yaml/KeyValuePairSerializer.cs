// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Serializers;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A Yaml Serializer for <see cref="KeyValuePair{TKey,TValue}"/>.
    /// Because this type is immutable we need to implement a special serializer.
    /// </summary>
    [YamlSerializerFactory(YamlSerializerFactoryAttribute.Default)]
    internal class KeyValuePairSerializer : ObjectSerializer
    {
        private struct MutableKeyValuePair<TKey, TValue>
        {
            public MutableKeyValuePair(KeyValuePair<TKey, TValue> kv)
            {
                Key = kv.Key;
                Value = kv.Value;
            }
            
            public TKey Key { get; set; }

            public TValue Value { get; set; }
        }

        /// <inheritdoc />
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            if (!typeDescriptor.Type.IsGenericType)
                return null;

            var genericTypeDefinition = typeDescriptor.Type.GetGenericTypeDefinition();
            return genericTypeDefinition == typeof(KeyValuePair<,>) ? this : null;
        }

        /// <inheritdoc />
        protected override void CreateOrTransformObject(ref ObjectContext objectContext)
        {
            var keyValueType = objectContext.Descriptor.Type;
            var typeArguments = keyValueType.GetGenericArguments();
            var mutableKeyValueType = typeof(MutableKeyValuePair<,>).MakeGenericType(typeArguments);
            objectContext.Instance = objectContext.SerializerContext.IsSerializing
                ? Activator.CreateInstance(mutableKeyValueType, objectContext.Instance)
                : Activator.CreateInstance(mutableKeyValueType);
        }

        /// <inheritdoc />
        protected override void TransformObjectAfterRead(ref ObjectContext objectContext)
        {
            var mutableKeyValueType = objectContext.Descriptor.Type;
            var typeArguments = mutableKeyValueType.GetGenericArguments();
            var keyValueType = typeof(KeyValuePair<,>).MakeGenericType(typeArguments);
            var key = objectContext.Descriptor["Key"].Get(objectContext.Instance);
            var value = objectContext.Descriptor["Value"].Get(objectContext.Instance);
            objectContext.Instance = Activator.CreateInstance(keyValueType, key, value);
        }
    }
}
