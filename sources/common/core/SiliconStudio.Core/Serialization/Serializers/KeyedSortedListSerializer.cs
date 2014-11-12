using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Core.Serialization.Serializers
{
    public class KeyedSortedListSerializer<TKeyedList, TKey, T> : DataSerializer<TKeyedList>, IDataSerializerInitializer, IDataSerializerGenericInstantiation where TKeyedList : KeyedSortedList<TKey, T>
    {
        private DataSerializer<T> itemDataSerializer;

        public override void PreSerialize(ref TKeyedList obj, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Deserialize)
            {
                if (obj == null)
                    obj = Activator.CreateInstance<TKeyedList>();
                else
                    obj.Clear();
            }
        }

        /// <inheritdoc/>
        public void Initialize(SerializerSelector serializerSelector)
        {
            itemDataSerializer = MemberSerializer<T>.Create(serializerSelector);
        }

        /// <inheritdoc/>
        public override void Serialize(ref TKeyedList obj, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Deserialize)
            {
                // TODO: We could probably avoid using TrackingKeyedList.Add, and directly fill the items list (since items are supposed to be sorted already).
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

        public void EnumerateGenericInstantiations(SerializerSelector serializerSelector, IList<Type> genericInstantiations)
        {
            genericInstantiations.Add(typeof(T));
        }
    }
}