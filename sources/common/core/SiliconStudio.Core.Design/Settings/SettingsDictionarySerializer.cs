// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Serializers;

namespace SiliconStudio.Core.Settings
{
    [YamlSerializerFactory(SettingsProfileSerializer.YamlProfile)]
    internal class SettingsDictionarySerializer : DictionarySerializer
    {
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            var type = typeDescriptor.Type;
            return type == typeof(SettingsDictionary) ? this : null;
        }

        protected override void WriteDictionaryItem(ref ObjectContext objectContext, KeyValuePair<object, object> keyValue, KeyValuePair<Type, Type> keyValueTypes)
        {
            var propertyKey = (UFile)keyValue.Key;
            objectContext.SerializerContext.ObjectSerializerBackend.WriteDictionaryKey(ref objectContext, propertyKey, keyValueTypes.Key);

            // Deduce expected value type from PropertyKey
            var parsingEvents = (List<ParsingEvent>)keyValue.Value;
            var writer = objectContext.Writer;
            foreach (var parsingEvent in parsingEvents)
            {
                writer.Emit(parsingEvent);
            }
        }

        protected override KeyValuePair<object, object> ReadDictionaryItem(ref ObjectContext objectContext, KeyValuePair<Type, Type> keyValueTypes)
        {
            // Read PropertyKey
            var keyResult = (UFile)objectContext.SerializerContext.ObjectSerializerBackend.ReadDictionaryKey(ref objectContext, keyValueTypes.Key);

            // Save the Yaml stream, in case loading fails we can keep this representation
            var parsingEvents = new List<ParsingEvent>();
            var reader = objectContext.Reader;
            var startDepth = reader.CurrentDepth;
            do
            {
                parsingEvents.Add(reader.Expect<ParsingEvent>());
            } while (reader.CurrentDepth > startDepth);


            var valueResult = parsingEvents;

            return new KeyValuePair<object, object>(keyResult, valueResult);
        }
    }
}
