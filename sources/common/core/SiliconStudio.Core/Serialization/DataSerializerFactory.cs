// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Core.Serialization
{
    public struct AssemblySerializerEntry
    {
        public ObjectId Id;
        public Type ObjectType;
        public Type SerializerType;

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

    public class AssemblySerializers : Dictionary<string, AssemblySerializersPerProfile>
    {
        public AssemblySerializers(Assembly assembly)
        {
            Assembly = assembly;
            Modules = new List<Module>();
        }

        public Assembly Assembly { get; private set; }

        public List<Module> Modules { get; private set; }
    }

    public static class DataSerializerFactory
    {
        internal static object Lock = new object();

        // List of all the factories
        private static readonly List<WeakReference<SerializerSelector>> SerializerSelectors = new List<WeakReference<SerializerSelector>>();

        // List of registered assemblies
        private static readonly List<AssemblySerializers> AssemblySerializers = new List<AssemblySerializers>();

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
            // Run module ctor
            foreach (var module in assemblySerializers.Modules)
            {
                ModuleRuntimeHelpers.RunModuleConstructor(module);
            }

            lock (Lock)
            {
                // Update existing SerializerSelector
                AssemblySerializers.Add(assemblySerializers);

                // Register serializers
                foreach (var assemblySerializerPerProfile in assemblySerializers)
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

            foreach (var weakSerializerSelector in SerializerSelectors)
            {
                SerializerSelector serializerSelector;
                if (weakSerializerSelector.TryGetTarget(out serializerSelector))
                {
                    serializerSelector.Invalidate();
                }
            }
        }

        public static AssemblySerializers UnregisterSerializationAssembly(Assembly assembly)
        {
            AssemblySerializers removedAssemblySerializer;

            lock (Lock)
            {
                removedAssemblySerializer = AssemblySerializers.FirstOrDefault(x => x.Assembly == assembly);
                if (removedAssemblySerializer == null)
                    return null;

                AssemblySerializers.Remove(removedAssemblySerializer);

                // Rebuild serializer list
                // TODO: For now, we simply reregister all assemblies one-by-one, but it can easily be improved if it proves to be unefficient (for now it shouldn't happen often so probably not a big deal)
                DataSerializersPerProfile.Clear();

                foreach (var assemblySerializer in AssemblySerializers)
                {
                    RegisterSerializationAssembly(assemblySerializer);
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

            return removedAssemblySerializer;
        }
    }
}