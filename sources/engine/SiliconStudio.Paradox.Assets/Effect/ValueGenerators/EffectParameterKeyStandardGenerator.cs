// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Assets.Effect.ValueGenerators
{
    /// <summary>
    /// Default implementation for a <see cref="IEffectParameterGenerator"/> using a dictionary of <see cref="ParameterKey"/>
    /// associated with:
    /// <ul>
    /// <li>An object value</li>
    /// <li>A list of object value (implementing <see cref="IList{Object}"/></li>
    /// <li>A value generator <see cref="IEffectParameterValueGenerator"/></li>
    /// <li>A list of value generator <see cref="IEffectParameterValueGenerator"/></li>
    /// </ul>
    /// </summary>
    [DataContract("!fx.generator.standard")]
    [SiliconStudio.Core.Serialization.Serializers.DataSerializer(typeof(EffectParameterKeyStandardGeneratorSerializer))]
    public sealed class EffectParameterKeyStandardGenerator : Dictionary<ParameterKey, object>, IEffectParameterGenerator
    {
        public IEnumerable<KeyValuePair<ParameterKey, object>> Generate()
        {
            foreach (var item in this)
            {
                var collectionGenerator = item.Value as EffectParameterValueGeneratorCollection;
                if (collectionGenerator != null)
                {
                    foreach (var effectParameterValueGenerator in collectionGenerator)
                    {
                        foreach (var value in effectParameterValueGenerator.GenerateValues(item.Key))
                        {
                            yield return CreateKeyValue(item.Key, value);
                        }
                    }
                }
                else
                {
                    var valueGenerator = item.Value as IEffectParameterValueGenerator;
                    if (valueGenerator != null)
                    {
                        foreach (var value in valueGenerator.GenerateValues(item.Key))
                        {
                            yield return CreateKeyValue(item.Key, value);
                        }
                    }
                    else
                    {
                        var directValues = item.Value as IList<object>;
                        if (directValues != null)
                        {
                            foreach (var value in directValues)
                            {
                                yield return CreateKeyValue(item.Key, value);
                            }
                        }
                        else
                        {
                            yield return CreateKeyValue(item.Key, item.Value);
                        }
                    }
                }
            }
        }

        // TODO this is not reusable outside this class (in other generators?)
        private KeyValuePair<ParameterKey, object> CreateKeyValue(ParameterKey key, object value)
        {
            // Find the real key instead of key loaded from yaml
            // as it can be later change at runtime 
            var keyName= key.Name;
            key = ParameterKeys.FindByName(keyName);
            if (key == null)
            {
                throw new InvalidOperationException("ParameterKey [{0}] was not found from assemblies".ToFormat(keyName));
            }

            return new KeyValuePair<ParameterKey, object>(key, key.ConvertValue(value));
        }
    }

    internal static class EffectParameterGeneratorExtensions
    {
        private static readonly IList<KeyValuePair<ParameterKey, List<object>>> EmptyKeyValuePairs = new List<KeyValuePair<ParameterKey, List<object>>>().AsReadOnly();

        public static IList<KeyValuePair<ParameterKey, List<object>>> GenerateKeyValues(this IEffectParameterGenerator generator)
        {
            if (generator == null)
            {
                return EmptyKeyValuePairs;
            }

            var keyValues = new Dictionary<ParameterKey, List<object>>();
            foreach (var keyValue in generator.Generate())
            {
                List<object> values;
                if (!keyValues.TryGetValue(keyValue.Key, out values))
                {
                    values = new List<object>();
                    keyValues.Add(keyValue.Key, values);
                }
                values.Add(keyValue.Value);
            }
            return keyValues.ToList();
        }
    }

    internal class EffectParameterKeyStandardGeneratorSerializer : DataSerializer<EffectParameterKeyStandardGenerator>
    {
        /// <inheritdoc/>
        public override void Serialize(ref EffectParameterKeyStandardGenerator obj, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                stream.Write(obj.Count);
                foreach (var item in obj)
                {
                    stream.SerializeExtended(item.Key, mode);
                    var valueType = item.Value.GetType();
                    if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(EffectParameterValuesGenerator<>))
                    {
                        var value = (IEffectParameterValueGenerator)item.Value;
                        var count = value.GenerateValues(item.Key).Count();
                        stream.Write(count);
                        foreach (var item2 in value.GenerateValues(item.Key))
                        {
                            stream.SerializeExtended(item2, mode);
                        }
                    }
                    else
                    {
                        stream.SerializeExtended(item.Value, mode);
                    }
                }
            }
            else if (mode == ArchiveMode.Deserialize)
            {
                throw new NotImplementedException();
            }
        }
    }
}