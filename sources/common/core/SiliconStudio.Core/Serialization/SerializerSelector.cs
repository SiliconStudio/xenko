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
        private object Lock = new object();
        private readonly bool reuseReferences;
        private readonly string[] profiles;
        private Dictionary<Type, DataSerializer> dataSerializersByType = new Dictionary<Type, DataSerializer>();
        private Dictionary<ObjectId, DataSerializer> dataSerializersByTypeId = new Dictionary<ObjectId, DataSerializer>();

        /// <summary>
        /// Gets the default instance of Serializer.
        /// </summary>
        /// <value>
        /// The default instance.
        /// </value>
        public static SerializerSelector Default { get; internal set; }

        public static SerializerSelector Asset { get; internal set; }
        public static SerializerSelector AssetWithReuse { get; internal set; }

        public IEnumerable<string> Profiles { get { return profiles; } }

        private bool invalidated;

        static SerializerSelector()
        {
            // Do a two step initialization to make sure field is set and accessible during construction
            Default = new SerializerSelector(false, true, "Default");
            Default.Initialize();

            Asset = new SerializerSelector(false, true, "Default", "Content");
            Asset.Initialize();

            AssetWithReuse = new SerializerSelector(true, true, "Default", "Content");
            AssetWithReuse.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializerSelector"/> class.
        /// </summary>
        /// <param name="reuseReferences">if set to <c>true</c> [reuse references] (allow cycles in the object graph).</param>
        /// <param name="profiles">The profiles.</param>
        public SerializerSelector(bool reuseReferences, params string[] profiles)
        {
            this.reuseReferences = reuseReferences;
            this.profiles = profiles;
            Initialize();
        }

        public SerializerSelector(params string[] profiles) : this(false, profiles)
        {
        }


        /// <summary>
        /// Checks if this instance supports the specified serialization profile.
        /// </summary>
        /// <param name="profile">Name of the profile</param>
        /// <returns><c>true</c> if this instance supports the specified serialization profile</returns>
        public bool HasProfile(string profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            for (int i = 0; i < profiles.Length; i++)
            {
                if (profile == profiles[i])
                {
                    return true;
                }
            }
            return false;
        }

        private SerializerSelector(bool reuseReferences, bool unusedPrivateCtor, params string[] profiles)
        {
            this.reuseReferences = reuseReferences;
            this.profiles = profiles;
        }

        private void Initialize()
        {
            invalidated = true;
            DataSerializerFactory.RegisterSerializerSelector(this);
            UpdateDataSerializers();
        }

        /// <summary>
        /// Gets or sets a value indicating whether [serialization reuses references]
        /// (that is, each reference gets assigned an ID and if it is serialized again, same instance will be reused).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [serialization reuses references]; otherwise, <c>false</c>.
        /// </value>
        public bool ReuseReferences { get { return reuseReferences; } }

        public DataSerializer GetSerializer(ref ObjectId typeId)
        {
            lock (Lock)
            {
                if (invalidated)
                    UpdateDataSerializers();

                DataSerializer dataSerializer;
                dataSerializersByTypeId.TryGetValue(typeId, out dataSerializer);
                if (dataSerializer != null)
                    EnsureInitialized(dataSerializer);
                return dataSerializer;
            }
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <param name="type">The type that you want to (de)serialize.</param>
        /// <returns>The <see cref="DataSerializer{T}"/> for this type if it exists or can be created, otherwise null.</returns>
        public DataSerializer GetSerializer(Type type)
        {
            lock (Lock)
            {
                if (invalidated)
                    UpdateDataSerializers();

                DataSerializer dataSerializer;
                dataSerializersByType.TryGetValue(type, out dataSerializer);
                if (dataSerializer != null)
                    EnsureInitialized(dataSerializer);
                return dataSerializer;
            }
        }

        private void EnsureInitialized(DataSerializer dataSerializer)
        {
            if (!dataSerializer.Initialized)
            {
                // Mark it as initialized first (avoid infinite recursion)
                dataSerializer.Initialized = true;

                // Initialize (if necessary)
                (dataSerializer as IDataSerializerInitializer)?.Initialize(this);
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

        internal void Invalidate()
        {
            lock (Lock)
            {
                invalidated = true;
            }
        }

        private void UpdateDataSerializers()
        {
            if (invalidated)
            {
                var oldDataSerializersByType = dataSerializersByType;
                var oldDataSerializersByTypeId = dataSerializersByTypeId;

                dataSerializersByType = new Dictionary<Type, DataSerializer>();
                dataSerializersByTypeId = new Dictionary<ObjectId, DataSerializer>();

                invalidated = false;

                // Create list of combined serializers
                var combinedSerializers = new Dictionary<Type, AssemblySerializerEntry>();

                lock (DataSerializerFactory.Lock)
                {
                    foreach (var profile in profiles)
                    {
                        Dictionary<Type, AssemblySerializerEntry> serializersPerProfile;
                        if (DataSerializerFactory.DataSerializersPerProfile.TryGetValue(profile, out serializersPerProfile))
                        {
                            foreach (var serializer in serializersPerProfile)
                            {
                                combinedSerializers[serializer.Key] = serializer.Value;
                            }
                        }
                    }
                }

                var newSerializers = new List<DataSerializer>();

                // Create new list of serializers (it will create new ones, and remove unused ones)
                foreach (var serializer in combinedSerializers)
                {
                    DataSerializer dataSerializer;
                    if (!oldDataSerializersByType.TryGetValue(serializer.Key, out dataSerializer))
                    {
                        if (serializer.Value.SerializerType != null)
                        {
                            // New serializer, let's create it
                            dataSerializer = (DataSerializer)Activator.CreateInstance(serializer.Value.SerializerType);
                            dataSerializer.SerializationTypeId = serializer.Value.Id;

                            newSerializers.Add(dataSerializer);
                        }
                    }

                    dataSerializersByType[serializer.Key] = dataSerializer;
                    dataSerializersByTypeId[serializer.Value.Id] = dataSerializer;
                }

                // Prepare all serializers
                foreach (var dataSerializer in newSerializers)
                {
                    PrepareSerializer(dataSerializer);
                }
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
        }
    }
}
