// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// A custom dictionary to keep track of the order the elements were inserted.
    /// </summary>
    [DataSerializer(typeof(GenericDictionary.Serializer))]
    [DataContract("GenericDictionary")]
    public class GenericDictionary : IDictionary<string, INodeParameter>
    {
        private List<KeyValuePair<string, INodeParameter>> internalDictionary;

        public GenericDictionary()
        {
            internalDictionary = new List<KeyValuePair<string, INodeParameter>>();
        }

        //TODO: custom enumerator?
        public IEnumerator<KeyValuePair<string, INodeParameter>> GetEnumerator()
        {
            return internalDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, INodeParameter> item)
        {
            internalDictionary.Add(item);
        }

        public void Clear()
        {
            internalDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, INodeParameter> item)
        {
            return internalDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, INodeParameter>[] array, int arrayIndex)
        {
            var copyCount = Math.Min(array.Length - arrayIndex, internalDictionary.Count);
            for (var i = 0; i < copyCount; ++i)
            {
                array[arrayIndex + i] = internalDictionary[i];
            }
        }

        public bool Remove(KeyValuePair<string, INodeParameter> item)
        {
            return internalDictionary.Remove(item);
        }

        public int Count
        {
            get
            {
                return internalDictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool ContainsKey(string key)
        {
            return internalDictionary.Any(x => x.Key == key);
        }

        public void Add(string key, INodeParameter value)
        {
            internalDictionary.Add(new KeyValuePair<string, INodeParameter>(key, value));
        }

        public bool Remove(string key)
        {
            if (ContainsKey(key))
            {
                internalDictionary.RemoveAll(x => x.Key == key);
                return true;
            }
            return false;
        }

        public bool TryGetValue(string key, out INodeParameter value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }
            value = null;
            return false;
        }

        public INodeParameter this[string key]
        {
            get
            {
                var found = internalDictionary.FirstOrDefault(x => x.Key == key).Value;
                if (found != null)
                    return found;
                throw new KeyNotFoundException();
            }
            set
            {
                var newValue = new KeyValuePair<string, INodeParameter>(key, value);
                var foundIndex = internalDictionary.FindIndex(x => x.Key == key);
                if (foundIndex >= 0)
                    internalDictionary[foundIndex] = newValue;
                else
                    internalDictionary.Add(newValue);
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return internalDictionary.Select(x => x.Key).ToList();
            }
        }

        public ICollection<INodeParameter> Values
        {
            get
            {
                return internalDictionary.Select(x => x.Value).ToList();
            }
        }

        internal class Serializer : DataSerializer<GenericDictionary>, IDataSerializerInitializer, IDataSerializerGenericInstantiation
        {

            private DataSerializer<KeyValuePair<string, INodeParameter>> itemDataSerializer;

            /// <inheritdoc/>
            public void Initialize(SerializerSelector serializerSelector)
            {
                itemDataSerializer = serializerSelector.GetSerializer<KeyValuePair<string, INodeParameter>>();
            }

            public override void PreSerialize(ref GenericDictionary obj, ArchiveMode mode, SerializationStream stream)
            {
                if (mode == ArchiveMode.Deserialize)
                {
                    // TODO: Peek the dictionary size
                    if (obj == null)
                        obj = new GenericDictionary();
                    else
                        obj.Clear();
                }
            }

            /// <inheritdoc/>
            public override void Serialize(ref GenericDictionary obj, ArchiveMode mode, SerializationStream stream)
            {
                if (mode == ArchiveMode.Deserialize)
                {
                    // Should be null if it was
                    int count = stream.ReadInt32();
                    for (int i = 0; i < count; ++i)
                    {
                        var value = new KeyValuePair<string, INodeParameter>();
                        itemDataSerializer.Serialize(ref value, mode, stream);
                        obj.Add(value.Key, value.Value);
                    }
                }
                else if (mode == ArchiveMode.Serialize)
                {
                    stream.Write(obj.Count);
                    foreach (var item in obj.internalDictionary)
                    {
                        itemDataSerializer.Serialize(item, stream);
                    }
                }
            }

            public void EnumerateGenericInstantiations(SerializerSelector serializerSelector, IList<Type> genericInstantiations)
            {
                genericInstantiations.Add(typeof(KeyValuePair<string, INodeParameter>));
            }
        }
    }
}
