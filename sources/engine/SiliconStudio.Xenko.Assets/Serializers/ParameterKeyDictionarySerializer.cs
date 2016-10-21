// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Serializers;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Serializers
{
    [YamlSerializerFactory]
    internal class ParameterKeyDictionarySerializer : DictionarySerializer, IDataCustomVisitor
    {
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            var type = typeDescriptor.Type;
            return CanVisit(type) ? this : null;
        }

        protected override void WriteDictionaryItems(ref ObjectContext objectContext)
        {
            // Don't sort dictionary keys
            var savedSettings = objectContext.Settings.SortKeyForMapping;
            objectContext.Settings.SortKeyForMapping = false;
            base.WriteDictionaryItems(ref objectContext);
            objectContext.Settings.SortKeyForMapping = savedSettings;
        }

        protected override void WriteDictionaryItem(ref ObjectContext objectContext, KeyValuePair<object, object> keyValue, KeyValuePair<Type, Type> keyValueTypes)
        {
            var propertyKey = (PropertyKey)keyValue.Key;
            objectContext.SerializerContext.ObjectSerializerBackend.WriteDictionaryKey(ref objectContext, propertyKey, keyValueTypes.Key);

            // Deduce expected value type from PropertyKey
            objectContext.SerializerContext.ObjectSerializerBackend.WriteDictionaryKey(ref objectContext, keyValue.Value, propertyKey.PropertyType);
        }

        protected override KeyValuePair<object, object> ReadDictionaryItem(ref ObjectContext objectContext, KeyValuePair<Type, Type> keyValueTypes)
        {
            // Read PropertyKey
            var keyResult = (PropertyKey)objectContext.SerializerContext.ObjectSerializerBackend.ReadDictionaryKey(ref objectContext, keyValueTypes.Key);

            // Deduce expected value type from PropertyKey
            var valueResult = (PropertyKey)objectContext.SerializerContext.ObjectSerializerBackend.ReadDictionaryKey(ref objectContext, keyResult.PropertyType);

            return new KeyValuePair<object, object>(keyResult, valueResult);
        }

        public bool CanVisit(Type type)
        {
            return typeof(IDictionary<ParameterKey, object>).IsAssignableFrom(type)
                || typeof(IDictionary<PropertyKey, object>).IsAssignableFrom(type);
        }

        public void Visit(ref VisitorContext context)
        {
            // Visit a ComputeColorParameters without visiting properties
            context.Visitor.VisitObject(context.Instance, context.Descriptor, false);
        }
    }
}
