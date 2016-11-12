using System;
using System.Collections.Generic;

namespace SiliconStudio.Core.Serialization.Serializers
{
    /// <summary>
    /// Data serializer for HashSet{T}.
    /// </summary>
    /// <typeparam name="T">Generics type of HashSet{T}.</typeparam>
    [DataSerializerGlobal(typeof(HashSetSerializer<>), typeof(HashSet<>), DataSerializerGenericMode.GenericArguments)]
    public class HashSetSerializer<T> : DataSerializer<HashSet<T>>, IDataSerializerGenericInstantiation
    {
        private DataSerializer<T> itemDataSerializer;

        /// <inheritdoc/>
        public override void Initialize(SerializerSelector serializerSelector)
        {
            itemDataSerializer = MemberSerializer<T>.Create(serializerSelector);
        }

        /// <inheritdoc/>
        public override void PreSerialize(ref HashSet<T> obj, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Deserialize)
            {
                if (obj == null)
                    obj = new HashSet<T>();
                else
                    obj.Clear();
            }
        }

        /// <inheritdoc/>
        public override void Serialize(ref HashSet<T> obj, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Deserialize)
            {
                int count = stream.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    T value = default(T);
                    itemDataSerializer.Serialize(ref value, mode, stream);
                    obj.Add(value);
                }
            }
            else if (mode == ArchiveMode.Serialize)
            {
                stream.Write(obj.Count);
                foreach (T item in obj)
                {
                    itemDataSerializer.Serialize(item, stream);
                }
            }
        }

        /// <inheritdoc/>
        public void EnumerateGenericInstantiations(SerializerSelector serializerSelector, IList<Type> genericInstantiations)
        {
            genericInstantiations.Add(typeof(T));
        }
    }
}