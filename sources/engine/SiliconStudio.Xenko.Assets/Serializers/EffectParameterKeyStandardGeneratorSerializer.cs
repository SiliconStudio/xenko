// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;

using SharpDX.DirectWrite;

using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;

using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Assets.Effect;
using SiliconStudio.Xenko.Assets.Effect.ValueGenerators;
using SiliconStudio.Xenko.Effects;

using ITypeDescriptor = SharpYaml.Serialization.ITypeDescriptor;

namespace SiliconStudio.Xenko.Assets.Serializers
{
    [YamlSerializerFactory]
    internal class EffectParameterKeyStandardGeneratorSerializer : DictionarySerializer, IDataCustomVisitor
    {
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            var type = typeDescriptor.Type;
            return CanVisit(type) ? this : null;
        }
        protected override KeyValuePair<object, object> ReadDictionaryItem(ref ObjectContext objectContext, KeyValuePair<Type, Type> keyValueType)
        {
            var readPair = base.ReadDictionaryItem(ref objectContext, keyValueType);

            if (readPair.Value != null && readPair.Value.GetType() == typeof(List<object>))
            {
                var key = (ParameterKey)readPair.Key;

                var newValueType = typeof(EffectParameterValuesGenerator<>).MakeGenericType(key.PropertyType);
                var newList = (IEffectParameterValueGenerator)Activator.CreateInstance(newValueType);

                foreach (var item in (List<object>)readPair.Value)
                    newList.AddValue(key, item);

                readPair = new KeyValuePair<object, object>(readPair.Key, newList);
            }

            return readPair;
        }

        protected override void WriteDictionaryItem(ref ObjectContext objectContext, KeyValuePair<object, object> keyValue, KeyValuePair<Type, Type> types)
        {
            var key = keyValue.Key as ParameterKey;
            if (key != null && keyValue.Value != null)
            {
                var valueType = keyValue.Value.GetType();
                if (valueType != typeof(List<object>) && valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(EffectParameterValuesGenerator<>))
                {
                    // cast in List<object>
                    var newTypes = new KeyValuePair<Type, Type>(types.Key, typeof(List<object>));
                    var newList = ((IEffectParameterValueGenerator)keyValue.Value).GenerateValues(keyValue.Key as ParameterKey).ToList();
                    var newKeyValue = new KeyValuePair<object, object>(keyValue.Key, newList);
                    base.WriteDictionaryItem(ref objectContext, newKeyValue, newTypes);
                    return;
                }
            }

            base.WriteDictionaryItem(ref objectContext, keyValue, types);
        }

        public bool CanVisit(Type type)
        {
            return typeof(EffectParameterKeyStandardGenerator) == type;
        }

        public void Visit(ref VisitorContext context)
        {
            // Visit a ComputeColorParameters without visiting properties
            context.Visitor.VisitObject(context.Instance, context.Descriptor, false);
        }
    }
}
