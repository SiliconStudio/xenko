// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Core.Serialization
{
    public class DataSerializerFactory
    {
        private readonly List<Action<DataSerializerFactory>> serializationProfileInitializers;
        private int serializationProfileInitializerCount; // Cached count so that we know if items are added

        private readonly Dictionary<ObjectId, DataSerializer> dataSerializersById = new Dictionary<ObjectId, DataSerializer>();
        private readonly Dictionary<Type, KeyValuePair<ObjectId, DataSerializer>> dataSerializersByType = new Dictionary<Type, KeyValuePair<ObjectId, DataSerializer>>();

        public static readonly Dictionary<string, List<Action<DataSerializerFactory>>> SerializationProfileInitializers = new Dictionary<string, List<Action<DataSerializerFactory>>>();

        public string ProfileName { get; private set; }

        public static void RegisterSerializationProfile(string profileName, Action<DataSerializerFactory> serializationProfileInitializer)
        {
            lock (SerializationProfileInitializers)
            {
                List<Action<DataSerializerFactory>> existingSerializationProfileInitializers;

                // If there was already existing delegate, combine it at the end
                if (!SerializationProfileInitializers.TryGetValue(profileName, out existingSerializationProfileInitializers))
                {
                    existingSerializationProfileInitializers = new List<Action<DataSerializerFactory>>();
                    SerializationProfileInitializers.Add(profileName, existingSerializationProfileInitializers);
                }

                // Register new combined delegate
                lock (existingSerializationProfileInitializers)
                {
                    existingSerializationProfileInitializers.Add(serializationProfileInitializer);
                }
            }
        }

        public static DataSerializerFactory CreateDataSerializerFactory(string profileName)
        {
            lock (SerializationProfileInitializers)
            {
                // Try to find the profile.
                // If not existing yet, create an empty one.
                List<Action<DataSerializerFactory>> serializationProfileInitializers;
                if (!SerializationProfileInitializers.TryGetValue(profileName, out serializationProfileInitializers))
                {
                    serializationProfileInitializers = new List<Action<DataSerializerFactory>>();
                    SerializationProfileInitializers.Add(profileName, serializationProfileInitializers);
                }
                var dataSerializerFactory = new DataSerializerFactory(profileName, serializationProfileInitializers);

                return dataSerializerFactory;
            }
        }

        public DataSerializerFactory(string profileName, List<Action<DataSerializerFactory>> serializationProfileInitializers)
        {
            this.ProfileName = profileName;
            this.serializationProfileInitializers = serializationProfileInitializers;
            this.serializationProfileInitializerCount = 0;
        }

        /// <summary>
        /// Registers the serializer.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        public void RegisterSerializer(ObjectId id, DataSerializer serializer)
        {
            lock (dataSerializersByType)
            {
                serializer.SerializationTypeId = id;
                dataSerializersByType[serializer.SerializationType] = new KeyValuePair<ObjectId, DataSerializer>(id, serializer);
                dataSerializersById[id] = serializer;
            }
        }

        /// <summary>
        /// Registers the type has having no serializer (useful for abstract types, interfaces and special types such as object).
        /// </summary>
        /// <param name="type">The type.</param>
        public void RegisterNullSerializer(ObjectId id, Type type)
        {
            lock (dataSerializersByType)
            {
                dataSerializersByType[type] = new KeyValuePair<ObjectId, DataSerializer>(id, null);
                dataSerializersById[id] = null;
            }
        }

        /// <summary>
        /// Should return true when factory is able to create a <see cref="DataSerializer{T}"/> for this type.
        /// </summary>
        /// <param name="type">The type to create a <see cref="DataSerializer{T}"/> for.</param>
        /// <returns>
        ///   <c>true</c> if this instance can serialize the specified type; otherwise, <c>false</c>.
        /// </returns>
        public bool CanSerialize(Type type)
        {
            CheckForNewAssemblies();

            lock (dataSerializersByType)
            {
                return dataSerializersByType.ContainsKey(type);
            }
        }

        /// <summary>
        /// Should return true when factory is able to create a <see cref="DataSerializer{T}"/> for this type.
        /// </summary>
        /// <param name="typeId">The type ID to create a <see cref="DataSerializer{T}"/> for.</param>
        /// <returns>
        ///   <c>true</c> if this instance can serialize the specified type; otherwise, <c>false</c>.
        /// </returns>
        public bool CanSerialize(ref ObjectId typeId)
        {
            CheckForNewAssemblies();

            lock (dataSerializersByType)
            {
                return dataSerializersById.ContainsKey(typeId);
            }
        }

        private void CheckForNewAssemblies()
        {
            // New assemblies loaded?
            lock (serializationProfileInitializers)
            {
                if (serializationProfileInitializers.Count > serializationProfileInitializerCount)
                {
                    var start = serializationProfileInitializerCount;
                    serializationProfileInitializerCount = serializationProfileInitializers.Count;

                    // Execute new serializer profile initializers appended at the end
                    for (int i = start; i < serializationProfileInitializers.Count; ++i)
                    {
                        serializationProfileInitializers[i](this);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the (de)serializer. It should only be called if CanSerialize(type) is true.
        /// </summary>
        /// <param name="type">The type to create a serializer from.</param>
        /// <returns>A <see cref="DataSerializer{T}"/> that can serialize this specific type.</returns>
        public KeyValuePair<ObjectId, DataSerializer> GetSerializer(Type type)
        {
            lock (dataSerializersByType)
            {
                return dataSerializersByType[type];
            }
        }

        /// <summary>
        /// Gets the (de)serializer. It should only be called if CanSerialize(type) is true.
        /// </summary>
        /// <param name="type">The type to create a serializer from.</param>
        /// <returns>A <see cref="DataSerializer{T}"/> that can serialize this specific type.</returns>
        public DataSerializer GetSerializer(ref ObjectId typeId)
        {
            lock (dataSerializersByType)
            {
                return dataSerializersById[typeId];
            }
        }
    }
}