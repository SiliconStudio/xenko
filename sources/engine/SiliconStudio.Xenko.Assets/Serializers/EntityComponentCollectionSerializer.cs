// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Serializers
{
    /// <summary>
    /// Error resistant Script loading. It should work even if there is missing properties, members or types.
    /// If main script type is missing (usually due to broken assemblies), it will keep the Yaml representation so that it can be properly saved alter.
    /// </summary>
    [YamlSerializerFactory(YamlAssetProfile.Name)]
    internal class EntityComponentCollectionSerializer : CollectionWithIdsSerializer
    {
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            var type = typeDescriptor.Type;
            return type == typeof(EntityComponentCollection) ? this : null;
        }

        // TODO: we could avoid duplicating (most) of this method from CollectionWithIdsSerializer if DictionarySerializer had a ReadDictionaryKey and ReadDictionaryValue that would call directly the same method of the backend (then we could override only ReadDictionaryValue)
        protected override KeyValuePair<object, object> ReadDictionaryItem(ref ObjectContext objectContext, KeyValuePair<Type, Type> keyValueTypes)
        {
            var keyResult = objectContext.ObjectSerializerBackend.ReadDictionaryKey(ref objectContext, keyValueTypes.Key);
            var peek = objectContext.SerializerContext.Reader.Peek<Scalar>();
            if (Equals(peek?.Value, YamlDeletedKey))
            {
                return ReadDeletedDictionaryItem(ref objectContext, keyResult);
            }
            var valueResult = ReadCollectionItem(ref objectContext, keyValueTypes.Value, keyResult);
            return new KeyValuePair<object, object>(keyResult, valueResult);
        }

        // TODO: similar situation as above
        protected override void WriteDictionaryItem(ref ObjectContext objectContext, KeyValuePair<object, object> keyValue, KeyValuePair<Type, Type> keyValueTypes)
        {
            objectContext.ObjectSerializerBackend.WriteDictionaryKey(ref objectContext, keyValue.Key, keyValueTypes.Key);
            WriteCollectionItem(ref objectContext, keyValue.Key, keyValue.Value, keyValueTypes.Value);
        }

        private static object ReadCollectionItem(ref ObjectContext objectContext, Type itemType, object key)
        {
            // Save the Yaml stream, in case loading fails we can keep this representation
            var parsingEvents = new List<ParsingEvent>();
            var reader = objectContext.Reader;
            var startDepth = reader.CurrentDepth;
            do
            {
                parsingEvents.Add(reader.Expect<ParsingEvent>());
            } while (reader.CurrentDepth > startDepth);

            // Save states
            var previousReader = objectContext.SerializerContext.Reader;
            var previousAllowErrors = objectContext.SerializerContext.AllowErrors;

            objectContext.SerializerContext.Reader = new EventReader(new MemoryParser(parsingEvents));
            objectContext.SerializerContext.AllowErrors = true;

            try
            {
                return objectContext.ObjectSerializerBackend.ReadDictionaryValue(ref objectContext, itemType, key);
            }
            catch (YamlException ex)
            {
                // There was a failure, let's keep this object so that it can be serialized back later
                var startEvent = parsingEvents.FirstOrDefault() as MappingStart;
                string typeName = !string.IsNullOrEmpty(startEvent?.Tag) ? startEvent.Tag.Substring(1) : null;

                var log = objectContext.SerializerContext.Logger;
                log?.Warning($"Could not deserialize script {typeName}", ex);

                return new UnloadableComponent(parsingEvents, typeName);
            }
            finally
            {
                // Restore states
                objectContext.SerializerContext.Reader = previousReader;
                objectContext.SerializerContext.AllowErrors = previousAllowErrors;
            }
        }

        private static void WriteCollectionItem(ref ObjectContext objectContext, object key, object value, Type valueType)
        {
            // Check if we have a Yaml representation (in case loading failed)
            var unloadableScript = value as UnloadableComponent;
            if (unloadableScript != null)
            {
                var writer = objectContext.Writer;
                foreach (var parsingEvent in unloadableScript.ParsingEvents)
                {
                    writer.Emit(parsingEvent);
                }
                return;
            }

            objectContext.ObjectSerializerBackend.WriteDictionaryValue(ref objectContext, key, value, valueType);
        }
    }
}
