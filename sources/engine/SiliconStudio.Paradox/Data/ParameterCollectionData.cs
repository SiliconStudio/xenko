// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Core.Serialization.Serializers;
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
// TODO: Need to review whether our SortedList is as fast as .NET one before switching it for everybody
using SiliconStudio.Core.Collections;
#endif

namespace SiliconStudio.Paradox.Effects.Data
{
    [DataSerializer(typeof(ParameterCollectionData.Serializer))]
    public partial class ParameterCollectionData : SortedList<ParameterKey, object>
    {
        public ParameterCollectionData()
        {
        }

        public void Set(ParameterKey key, object value)
        {
            var indexOfKey = this.IndexOfKey(key);
            if (indexOfKey >= 0)
            {
                this[key] = value;
                return;
            }

            Add(key, value);
        }

        internal class Serializer : DataSerializer<ParameterCollectionData>, IDataSerializerInitializer
        {
            private bool reuseReferences;

            public void Initialize(SerializerSelector serializerSelector)
            {
                reuseReferences = serializerSelector.ReuseReferences;
            }

            public override void PreSerialize(ref ParameterCollectionData obj, ArchiveMode mode, SerializationStream stream)
            {
                if (mode == ArchiveMode.Deserialize)
                {
                    // TODO: Peek the dictionary size
                    if (obj == null)
                        obj = new ParameterCollectionData();
                    else
                        obj.Clear();
                }
            }

            public override void Serialize(ref ParameterCollectionData obj, ArchiveMode mode, SerializationStream stream)
            {
                if (mode == ArchiveMode.Deserialize)
                {
                    // Should be null if it was
                    int count = stream.ReadInt32();
                    for (int i = 0; i < count; ++i)
                    {
                        ParameterKey key = null;
                        object value = null;
                        bool matchingType = false;

                        stream.SerializeExtended(ref key, mode);
                        stream.Serialize(ref matchingType);

                        var valueType = matchingType ? key.PropertyType : typeof(object);
                        if (reuseReferences)
                            MemberReuseSerializer.SerializeExtended(stream, valueType, ref value, mode);
                        else
                            MemberNonSealedSerializer.SerializeExtended(stream, valueType, ref value, mode);

                        obj.Add(key, value);
                    }
                }
                else if (mode == ArchiveMode.Serialize)
                {
                    stream.Write(obj.Count);
                    foreach (var item in obj)
                    {
                        var key = item.Key;

                        // When serializing convert the value type to the expecting type
                        // This should probably better done at the source (when creating/filling the ParameterCollectionData)
                        var value = item.Key.ConvertValue(item.Value);

                        stream.SerializeExtended(ref key, mode);
                        bool matchingType = value.GetType().GetTypeInfo().IsAssignableFrom(key.PropertyType.GetTypeInfo());
                        stream.Serialize(ref matchingType);

                        var valueType = matchingType ? key.PropertyType : typeof(object);

                        if (reuseReferences)
                            MemberReuseSerializer.SerializeExtended(stream, valueType, ref value, mode);
                        else
                            MemberNonSealedSerializer.SerializeExtended(stream, valueType, ref value, mode);
                    }
                }
            }
        }
    }

    public partial class ParameterCollectionDataConverter : DataConverter<ParameterCollectionData, ParameterCollection>
    {
        public override void ConvertToData(ConverterContext converterContext, ref ParameterCollectionData parameterCollectionData, ParameterCollection parameterCollection)
        {
            parameterCollectionData = new ParameterCollectionData();
            foreach (var parameter in parameterCollection.InternalValues)
            {
                if (parameterCollection.IsValueOwner(parameter.Value))
                    parameterCollectionData.Add(parameter.Key, parameter.Value.Object);
            }
        }

        public override void ConvertFromData(ConverterContext converterContext, ParameterCollectionData parameterCollectionData, ref ParameterCollection parameterCollection)
        {
            parameterCollection = new ParameterCollection("Deserialized collection");
            foreach (var parameter in parameterCollectionData)
            {
                var parameterValue = parameter.Value;
                if (parameterValue is ContentReference)
                    parameterValue = converterContext.ConvertFromData<object>(parameterValue);
                parameterCollection.SetObject(parameter.Key, parameterValue);
            }
        }
    }
}