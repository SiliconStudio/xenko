// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;

using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Paradox.Effects;

using ITypeDescriptor = SharpYaml.Serialization.ITypeDescriptor;

namespace SiliconStudio.Paradox.Assets.Serializers
{
    [YamlSerializerFactory]
    internal class ParameterKeyDictionarySerializer : DictionarySerializer, IDataCustomVisitor
    {
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            var type = typeDescriptor.Type;
            return CanVisit(type) ? this : null;
        }

        protected override KeyValuePair<object, object> ReadDictionaryItem(ref ObjectContext objectContext, KeyValuePair<Type, Type> keyValueType)
        {
            var keyValue = base.ReadDictionaryItem(ref objectContext, keyValueType);
            // For value types, try to convert to their real type
            if (keyValue.Value != null && keyValue.GetType().IsValueType)
            {
                keyValue = new KeyValuePair<object, object>(keyValue.Key, ((ParameterKey)keyValue.Key).ConvertValue(keyValue.Value));
            }
            return keyValue;
        }

        public bool CanVisit(Type type)
        {
            return typeof(IDictionary<ParameterKey, object>).IsAssignableFrom(type);
        }

        public void Visit(ref VisitorContext context)
        {
            // Visit a GenericDictionary without visiting properties
            context.Visitor.VisitObject(context.Instance, context.Descriptor, false);
        }
    }
}