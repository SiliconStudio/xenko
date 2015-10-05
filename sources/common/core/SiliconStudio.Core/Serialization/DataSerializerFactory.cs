// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Core.Serialization
{
    /// <summary>
    /// An entry to a serialized object.
    /// </summary>
    public struct AssemblySerializerEntry
    {
        /// <summary>
        /// The id of the object.
        /// </summary>
        public readonly ObjectId Id;

        /// <summary>
        /// The type of the object.
        /// </summary>
        public readonly Type ObjectType;

        /// <summary>
        /// The type of the serialized object.
        /// </summary>
        public readonly Type SerializerType;

        /// <summary>
        /// Initializes a new instance of <see cref="AssemblySerializerEntry"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="objectType"></param>
        /// <param name="serializerType"></param>
        public AssemblySerializerEntry(ObjectId id, Type objectType, Type serializerType)
        {
            Id = id;
            ObjectType = objectType;
            SerializerType = serializerType;
        }
    }

    public class AssemblySerializersPerProfile : Collection<AssemblySerializerEntry>
    {
    }

    public class AssemblySerializers
    {
        public AssemblySerializers(Assembly assembly)
        {
            Assembly = assembly;
            Modules = new List<Module>();
            Profiles = new Dictionary<string, AssemblySerializersPerProfile>();
        }

        public Assembly Assembly { get; private set; }

        public List<Module> Modules { get; private set; }

        public Dictionary<string, AssemblySerializersPerProfile> Profiles { get; private set; }

        public override string ToString()
        {
            return Assembly.ToString();
        }
    }

    public static class DataSerializerFactory
    {
        internal static object Lock = new object();

        // List of all the factories
        private static readonly List<WeakReference<SerializerSelector>> SerializerSelectors = new List<WeakReference<SerializerSelector>>();

        // List of registered assemblies
        private static readonly List<AssemblySerializers> AssemblySerializers = new List<AssemblySerializers>();

        private static readonly Dictionary<Assembly, AssemblySerializers> AvailableAssemblySerializers = new Dictionary<Assembly, AssemblySerializers>();

        // List of serializers per profile
        internal static readonly Dictionary<string, Dictionary<Type, AssemblySerializerEntry>> DataSerializersPerProfile = new Dictionary<string, Dictionary<Type, AssemblySerializerEntry>>();

        public static void RegisterSerializerSelector(SerializerSelector serializerSelector)
        {
            SerializerSelectors.Add(new WeakReference<SerializerSelector>(serializerSelector));
        }

        public static AssemblySerializerEntry GetSerializer(string profile, Type type)
        {
            lock (Lock)
            {
                Dictionary<Type, AssemblySerializerEntry> serializers;
                AssemblySerializerEntry assemblySerializerEntry;
                if (!DataSerializersPerProfile.TryGetValue(profile, out serializers) || !serializers.TryGetValue(type, out assemblySerializerEntry))
                    return default(AssemblySerializerEntry);

                return assemblySerializerEntry;
            }
        }

        public static void RegisterSerializationAssembly(AssemblySerializers assemblySerializers)
        {
            lock (Lock)
            {
                // Register it (so that we can get it back if unregistered)
                if (!AvailableAssemblySerializers.ContainsKey(assemblySerializers.Assembly))
                    AvailableAssemblySerializers.Add(assemblySerializers.Assembly, assemblySerializers);

                // Check if already loaded
                if (AssemblySerializers.Contains(assemblySerializers))
                    return;

                // Update existing SerializerSelector
                AssemblySerializers.Add(assemblySerializers);
            }

            // Run module ctor
            foreach (var module in assemblySerializers.Modules)
            {
                ModuleRuntimeHelpers.RunModuleConstructor(module);
            }

            lock (Lock)
            {
                RegisterSerializers(assemblySerializers);
            }

            foreach (var weakSerializerSelector in SerializerSelectors)
            {
                SerializerSelector serializerSelector;
                if (weakSerializerSelector.TryGetTarget(out serializerSelector))
                {
                    serializerSelector.Invalidate();
                }
            }
        }

        public static void RegisterSerializationAssembly(Assembly assembly)
        {
            lock (Lock)
            {
                AssemblySerializers assemblySerializers;
                if (AvailableAssemblySerializers.TryGetValue(assembly, out assemblySerializers))
                    RegisterSerializationAssembly(assemblySerializers);
            }
        }

        public static void UnregisterSerializationAssembly(Assembly assembly)
        {
            AssemblySerializers removedAssemblySerializer;

            lock (Lock)
            {
                removedAssemblySerializer = AssemblySerializers.FirstOrDefault(x => x.Assembly == assembly);
                if (removedAssemblySerializer == null)
                    return;

                AssemblySerializers.Remove(removedAssemblySerializer);

                // Rebuild serializer list
                // TODO: For now, we simply reregister all assemblies one-by-one, but it can easily be improved if it proves to be unefficient (for now it shouldn't happen often so probably not a big deal)
                DataSerializersPerProfile.Clear();

                foreach (var assemblySerializer in AssemblySerializers)
                {
                    RegisterSerializers(assemblySerializer);
                }
            }

            foreach (var weakSerializerSelector in SerializerSelectors)
            {
                SerializerSelector serializerSelector;
                if (weakSerializerSelector.TryGetTarget(out serializerSelector))
                {
                    serializerSelector.Invalidate();
                }
            }
        }

        private static void RegisterSerializers(AssemblySerializers assemblySerializers)
        {
            // Register serializers
            foreach (var assemblySerializerPerProfile in assemblySerializers.Profiles)
            {
                var profile = assemblySerializerPerProfile.Key;

                Dictionary<Type, AssemblySerializerEntry> dataSerializers;
                if (!DataSerializersPerProfile.TryGetValue(profile, out dataSerializers))
                {
                    dataSerializers = new Dictionary<Type, AssemblySerializerEntry>();
                    DataSerializersPerProfile.Add(profile, dataSerializers);
                }

                foreach (var assemblySerializer in assemblySerializerPerProfile.Value)
                {
                    if (!dataSerializers.ContainsKey(assemblySerializer.ObjectType))
                    {
                        dataSerializers.Add(assemblySerializer.ObjectType, assemblySerializer);
                    }
                }
            }
        }
    }
}