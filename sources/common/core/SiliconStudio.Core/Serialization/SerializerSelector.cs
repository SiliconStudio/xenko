// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Core.Serialization
{
    public delegate void SerializeObjectDelegate(SerializationStream stream, ref object obj, ArchiveMode archiveMode);

    public class SerializerContext
    {
        public PropertyContainer Tags;

        public SerializerContext()
        {
            SerializerSelector = SerializerSelector.Default;
            Tags = new PropertyContainer(this);
        }

        /// <summary>
        /// Gets or sets the serializer.
        /// </summary>
        /// <value>
        /// The serializer.
        /// </value>
        public SerializerSelector SerializerSelector { get; set; }

        public T Get<T>(PropertyKey<T> key)
        {
            return Tags.Get(key);
        }

        public void Set<T>(PropertyKey<T> key, T value)
        {
            Tags.SetObject(key, value);
        }
    }

    /// <summary>
    /// Serializer context. It holds DataSerializer{T} objects and their factories.
    /// </summary>
    public class SerializerSelector
    {
        private readonly List<DataSerializerFactory> dataSerializerFactories = new List<DataSerializerFactory>();
        private readonly Dictionary<Type, DataSerializer> dataSerializersByType = new Dictionary<Type, DataSerializer>();
        private readonly Dictionary<ObjectId, DataSerializer> dataSerializersByTypeId = new Dictionary<ObjectId, DataSerializer>();

        /// <summary>
        /// Gets the default instance of Serializer.
        /// </summary>
        /// <value>
        /// The default instance.
        /// </value>
        public static SerializerSelector Default { get; internal set; }

        public static SerializerSelector Asset { get; internal set; }
        public static SerializerSelector AssetWithReuse { get; internal set; }

        static SerializerSelector()
        {
            Default = new SerializerSelector();
            Default.RegisterProfile("Default");

            Asset = new SerializerSelector();
            Asset.RegisterProfile("Default");
            Asset.RegisterProfile("Asset");

            AssetWithReuse = new SerializerSelector();
            AssetWithReuse.RegisterProfile("Default");
            AssetWithReuse.RegisterProfile("Asset");
            AssetWithReuse.ReuseReferences = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [serialization reuses references]
        /// (that is, each reference gets assigned an ID and if it is serialized again, same instance will be reused).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [serialization reuses references]; otherwise, <c>false</c>.
        /// </value>
        public bool ReuseReferences { get; set; }

        public bool HashOnly { get; set; }

        public SerializerSelector RegisterProfile(string profileName)
        {
            RegisterFactory(DataSerializerFactory.CreateDataSerializerFactory(profileName));
            return this;
        }

        /// <summary>
        /// Registers the <see cref="DataSerializer{T}"/> factory.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public SerializerSelector RegisterFactory(DataSerializerFactory factory)
        {
            dataSerializerFactories.Add(factory);
            return this;
        }

        /// <summary>
        /// Registers the serializer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer">The serializer.</param>
        public SerializerSelector RegisterSerializer<T>(DataSerializer<T> serializer)
        {
            lock (dataSerializersByType)
            {
                PrepareSerializer(serializer);
                dataSerializersByType[typeof(T)] = serializer;
                dataSerializersByTypeId[serializer.SerializationTypeId] = serializer;
            }

            return this;
        }

        public DataSerializer GetSerializer(ref ObjectId typeId)
        {
            lock (dataSerializersByType)
            {
                DataSerializer dataSerializer;
                if (!dataSerializersByTypeId.TryGetValue(typeId, out dataSerializer))
                {
                    // Iterate over IDataSerializerFactory
                    for (int index = dataSerializerFactories.Count - 1; index >= 0; index--)
                    {
                        var dataSerializerFactory = dataSerializerFactories[index];
                        if (!dataSerializerFactory.CanSerialize(ref typeId))
                            continue;

                        // Found a serializer, initialize it
                        dataSerializer = dataSerializerFactory.GetSerializer(ref typeId);
                        dataSerializersByType[dataSerializer.SerializationType] = dataSerializer;
                        dataSerializersByTypeId[typeId] = dataSerializer;

                        if (dataSerializer != null)
                            PrepareSerializer(dataSerializer);

                        return dataSerializer;
                    }

                    return null;
                }

                return dataSerializer;
            }
        }

        private void PrepareSerializer(DataSerializer dataSerializer)
        {
            // Ensure a serialization type ID has been generated (otherwise do so now)
            if (dataSerializer.SerializationTypeId == ObjectId.Empty)
            {
                // Need to generate serialization type id
                var typeName = dataSerializer.SerializationType.FullName;
                dataSerializer.SerializationTypeId = ObjectId.FromBytes(System.Text.Encoding.UTF8.GetBytes(typeName));
            }

            if (dataSerializer is IDataSerializerInitializer)
                ((IDataSerializerInitializer)dataSerializer).Initialize(this);
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <param name="type">The type that you want to (de)serialize.</param>
        /// <returns>The <see cref="DataSerializer{T}"/> for this type if it exists or can be created, otherwise null.</returns>
        public DataSerializer GetSerializer(Type type)
        {
            lock (dataSerializersByType)
            {
                DataSerializer dataSerializer;
                if (!dataSerializersByType.TryGetValue(type, out dataSerializer))
                {
                    // Iterate over IDataSerializerFactory
                    for (int index = dataSerializerFactories.Count - 1; index >= 0; index--)
                    {
                        var dataSerializerFactory = dataSerializerFactories[index];
                        if (!dataSerializerFactory.CanSerialize(type))
                            continue;

                        // Found a serializer, initialize it
                        var dataSerializerWithId = dataSerializerFactory.GetSerializer(type);
                        dataSerializer = dataSerializerWithId.Value;
                        dataSerializersByType[type] = dataSerializer;
                        dataSerializersByTypeId[dataSerializerWithId.Key] = dataSerializer;

                        if (dataSerializer != null)
                            PrepareSerializer(dataSerializer);

                        return dataSerializer;
                    }

                    return null;
                }

                return dataSerializer;
            }
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <typeparam name="T">The type that you want to (de)serialize.</typeparam>
        /// <returns>The <see cref="DataSerializer{T}"/> for this type if it exists or can be created, otherwise null.</returns>
        public DataSerializer<T> GetSerializer<T>()
        {
            return (DataSerializer<T>)GetSerializer(typeof(T));
        }
    }
}
