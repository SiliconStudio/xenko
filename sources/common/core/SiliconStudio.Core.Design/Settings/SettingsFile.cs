// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Core.Settings
{
    [YamlSerializerFactory]
    internal class ScriptCollectionSerializer : DictionarySerializer
    {
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            var type = typeDescriptor.Type;
            return type == typeof(SettingsDictionary) ? this : null;
        }

        protected override void WriteDictionaryItem(ref ObjectContext objectContext, KeyValuePair<object, object> keyValue, KeyValuePair<Type, Type> types)
        {
            var propertyKey = (UFile)keyValue.Key;
            objectContext.SerializerContext.WriteYaml(propertyKey, types.Key);

            // Deduce expected value type from PropertyKey
            var parsingEvents = (List<ParsingEvent>)keyValue.Value;
            var writer = objectContext.Writer;
            foreach (var parsingEvent in parsingEvents)
            {
                writer.Emit(parsingEvent);
            }
        }

        protected override KeyValuePair<object, object> ReadDictionaryItem(ref ObjectContext objectContext, KeyValuePair<Type, Type> keyValueType)
        {
            // Read PropertyKey
            var keyResult = (UFile)objectContext.SerializerContext.ReadYaml(null, keyValueType.Key);

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

    public class SettingsDictionary : Dictionary<UFile, List<ParsingEvent>>
    {
        
    }

    /// <summary>
    /// This class represents a set of settings that can be stored in a file. This class is public for serialization purpose only, and should not be used directly.
    /// </summary>
    [DataContract("SettingsFile")]
    public class SettingsFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsFile"/> class.
        /// </summary>
        public SettingsFile()
        {
            Settings = new SettingsDictionary();
        }

        /// <summary>
        /// Gets the collection of settings to serialize.
        /// </summary>
        [DataMemberCustomSerializer]
        public SettingsDictionary Settings { get; private set; }
    }
}