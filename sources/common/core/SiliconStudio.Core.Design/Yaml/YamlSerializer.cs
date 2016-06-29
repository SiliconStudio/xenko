// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using AttributeRegistry = SharpYaml.Serialization.AttributeRegistry;
using IMemberDescriptor = SharpYaml.Serialization.IMemberDescriptor;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// Default Yaml serializer used to serialize assets by default.
    /// </summary>
    public static class YamlSerializer
    {
        private static readonly Logger Log = GlobalLogger.GetLogger(typeof(YamlSerializer).Name);
        private static event Action<SharpYaml.Serialization.Descriptors.ObjectDescriptor, List<IMemberDescriptor>> PrepareMembersEvent;

        // TODO: This code is not robust in case of reloading assemblies into the same process
        private static readonly List<Assembly> RegisteredAssemblies = new List<Assembly>();
        private static readonly object Lock = new object();
        private static Serializer globalSerializer;
        private static Serializer globalSerializerWithoutId;

        public static event Action<SharpYaml.Serialization.Descriptors.ObjectDescriptor, List<IMemberDescriptor>> PrepareMembers
        {
            add
            {
                if (globalSerializer != null)
                    throw new InvalidOperationException("Event handlers cannot be added or removed after the serializer has been initialized.");

                PrepareMembersEvent += value;
            }
            remove
            {
                if (globalSerializer != null)
                    throw new InvalidOperationException("Event handlers cannot be added or removed after the serializer has been initialized.");
                PrepareMembersEvent -= value;
            }
        }

        /// <summary>
        /// Deserializes an object from the specified stream (expecting a YAML string).
        /// </summary>
        /// <param name="stream">A YAML string from a stream.</param>
        /// <returns>An instance of the YAML data.</returns>
        public static object Deserialize(Stream stream)
        {
            var serializer = GetYamlSerializer();
            return serializer.Deserialize(stream);
        }

        /// <summary>
        /// Deserializes an object from the specified stream (expecting a YAML string) into an existing object.
        /// </summary>
        /// <param name="stream">A YAML string from a stream.</param>
        /// <param name="existingObject">The object to deserialize into.</param>
        /// <returns>An instance of the YAML data.</returns>
        public static object Deserialize(Stream stream, object existingObject)
        {
            if (existingObject == null) throw new ArgumentNullException(nameof(existingObject));
            using (var textReader = new StreamReader(stream))
            {
                var serializer = GetYamlSerializer();
                return serializer.Deserialize(textReader, existingObject.GetType(), existingObject);
            }
        }

        /// <summary>
        /// Deserializes an object from the specified stream (expecting a YAML string).
        /// </summary>
        /// <param name="stream">A YAML string from a stream.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="contextSettings">The context settings.</param>
        /// <returns>An instance of the YAML data.</returns>
        public static object Deserialize(Stream stream, Type expectedType, SerializerContextSettings contextSettings)
        {
            var serializer = GetYamlSerializer();
            return serializer.Deserialize(stream, expectedType, contextSettings);
        }

        /// <summary>
        /// Deserializes an object from the specified stream (expecting a YAML string).
        /// </summary>
        /// <param name="stream">A YAML string from a stream .</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="contextSettings">The context settings.</param>
        /// <param name="aliasOccurred">if set to <c>true</c> a class/field/property/enum name has been renamed during deserialization.</param>
        /// <returns>An instance of the YAML data.</returns>
        public static object Deserialize(Stream stream, Type expectedType, SerializerContextSettings contextSettings, out bool aliasOccurred)
        {
            var serializer = GetYamlSerializer();
            SerializerContext context;
            var result = serializer.Deserialize(stream, expectedType, contextSettings, out context);
            aliasOccurred = context.HasRemapOccurred;
            return result;
        }

        /// <summary>
        /// Deserializes an object from the specified stream (expecting a YAML string).
        /// </summary>
        /// <param name="eventReader">A YAML event reader.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <returns>An instance of the YAML data.</returns>
        public static object Deserialize(EventReader eventReader, Type expectedType)
        {
            var serializer = GetYamlSerializer();
            return serializer.Deserialize(eventReader, expectedType);
        }

        /// <summary>
        /// Deserializes an object from the specified stream (expecting a YAML string).
        /// </summary>
        /// <param name="eventReader">A YAML event reader.</param>
        /// <param name="value">The value.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <returns>An instance of the YAML data.</returns>
        public static object Deserialize(EventReader eventReader, object value, Type expectedType)
        {
            var serializer = GetYamlSerializer();
            return serializer.Deserialize(eventReader, expectedType, value);
        }

        /// <summary>
        /// Deserializes an object from the specified stream (expecting a YAML string).
        /// </summary>
        /// <param name="eventReader">A YAML event reader.</param>
        /// <param name="value">The value.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="contextSettings">The context settings.</param>
        /// <returns>An instance of the YAML data.</returns>
        public static object Deserialize(EventReader eventReader, object value, Type expectedType, SerializerContextSettings contextSettings)
        {
            var serializer = GetYamlSerializer();
            return serializer.Deserialize(eventReader, expectedType, value, contextSettings);
        }

        /// <summary>
        /// Deserializes an object from the specified stream (expecting a YAML string).
        /// </summary>
        /// <param name="stream">A YAML string from a stream .</param>
        /// <returns>An instance of the YAML data.</returns>
        public static IEnumerable<T> DeserializeMultiple<T>(Stream stream, bool generateIds = true)
        {
            var serializer = GetYamlSerializer(generateIds);

            var input = new StreamReader(stream);
            var reader = new EventReader(new Parser(input));
            reader.Expect<StreamStart>();

            while (reader.Accept<DocumentStart>())
            {
                // Deserialize the document
                var doc = serializer.Deserialize<T>(reader);

                yield return doc;
            }
        }

        /// <summary>
        /// Serializes an object to specified stream in YAML format.
        /// </summary>
        /// <param name="emitter">The emitter.</param>
        /// <param name="instance">The object to serialize.</param>
        /// <param name="type">The type.</param>
        public static void Serialize(IEmitter emitter, object instance, Type type)
        {
            var serializer = GetYamlSerializer();
            serializer.Serialize(emitter, instance, type);
        }

        /// <summary>
        /// Serializes an object to specified stream in YAML format.
        /// </summary>
        /// <param name="emitter">The emitter.</param>
        /// <param name="instance">The object to serialize.</param>
        /// <param name="type">The type.</param>
        /// <param name="contextSettings">The context settings.</param>
        public static void Serialize(IEmitter emitter, object instance, Type type, SerializerContextSettings contextSettings)
        {
            var serializer = GetYamlSerializer();
            serializer.Serialize(emitter, instance, type, contextSettings);
        }

        /// <summary>
        /// Serializes an object to specified stream in YAML format.
        /// </summary>
        /// <param name="stream">The stream to receive the YAML representation of the object.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="generateIds"><c>true</c> to generate ~Id for class objects</param>
        public static void Serialize(Stream stream, object instance, bool generateIds = true)
        {
            var serializer = GetYamlSerializer(generateIds);
            serializer.Serialize(stream, instance);
        }

        /// <summary>
        /// Serializes an object to specified stream in YAML format.
        /// </summary>
        /// <param name="stream">The stream to receive the YAML representation of the object.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="type">The expected type.</param>
        /// <param name="contextSettings">The context settings.</param>
        /// <param name="generateIds"><c>true</c> to generate ~Id for class objects</param>
        public static void Serialize(Stream stream, object instance, Type type, SerializerContextSettings contextSettings, bool generateIds = true)
        {
            var serializer = GetYamlSerializer();
            serializer.Serialize(stream, instance, type, contextSettings);
        }

        /// <summary>
        /// Serializes an object to a string in YAML format.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="type">The expected type.</param>
        /// <param name="contextSettings">The context settings.</param>
        /// <param name="generateIds"><c>true</c> to generate ~Id for class objects</param>
        /// <returns>a string in YAML format</returns>
        public static string Serialize(object instance, Type type, SerializerContextSettings contextSettings, bool generateIds = true)
        {
            var serializer = GetYamlSerializer(generateIds);
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, instance, type ?? typeof(object), contextSettings);
                stream.Flush();
                stream.Position = 0;
                return new StreamReader(stream).ReadToEnd();
            }
        }

        /// <summary>
        /// Serializes an object to a string in YAML format.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="generateIds"><c>true</c> to generate ~Id for class objects</param>
        /// <returns>a string in YAML format</returns>
        public static string Serialize(object instance, bool generateIds = true)
        {
            return Serialize(instance, null, null, generateIds);
        }

        /// <summary>
        /// Gets the serializer settings.
        /// </summary>
        /// <returns>SerializerSettings.</returns>
        public static SerializerSettings GetSerializerSettings()
        {
            return GetYamlSerializer().Settings;
        }

        /// <summary>
        /// Reset the assembly cache used by this class.
        /// </summary>
        public static void ResetCache()
        {
            lock (Lock)
            {
                // Reset the current serializer as the set of assemblies has changed
                globalSerializer = null;
                globalSerializerWithoutId = null;
            }
        }

        private static Serializer GetYamlSerializer(bool generateIds = true)
        {
            Serializer localSerializer;
            // Cache serializer to improve performance
            lock (Lock)
            {
                if (generateIds)
                    localSerializer = CreateSerializer(ref globalSerializer, true);
                else
                    localSerializer = CreateSerializer(ref globalSerializerWithoutId, false);
            }
            return localSerializer;
        }

        private static Serializer CreateSerializer(ref Serializer localSerializer, bool generateIds)
        {
            if (localSerializer == null)
            {
                // var clock = Stopwatch.StartNew();

                var config = new SerializerSettings()
                    {
                        EmitAlias = false,
                        LimitPrimitiveFlowSequence = 0,
                        Attributes = new AtributeRegistryFilter(),
                        PreferredIndent = 4,
                        EmitShortTypeName = true,
                    };

                if (generateIds)
                    config.Attributes.PrepareMembersCallback += PrepareMembersCallback;

                for (int index = RegisteredAssemblies.Count - 1; index >= 0; index--)
                {
                    var registeredAssembly = RegisteredAssemblies[index];
                    config.RegisterAssembly(registeredAssembly);
                }

                localSerializer = new Serializer(config);
                localSerializer.Settings.ObjectSerializerBackend = new CustomObjectSerializerBackend(TypeDescriptorFactory.Default);

                // Log.Info("New YAML serializer created in {0}ms", clock.ElapsedMilliseconds);
            }

            return localSerializer;
        }

        private static void PrepareMembersCallback(SharpYaml.Serialization.Descriptors.ObjectDescriptor objDesc, List<IMemberDescriptor> memberDescriptors)
        {
            var type = objDesc.Type;

            if (IdentifiableHelper.IsIdentifiable(type) && !typeof(IIdentifiable).IsAssignableFrom(type))
            {
                memberDescriptors.Add(CustomDynamicMemberDescriptor);
            }

            // Call custom callbacks to prepare members
            PrepareMembersEvent?.Invoke(objDesc, memberDescriptors);
        }

        private static readonly CustomDynamicMember CustomDynamicMemberDescriptor = new CustomDynamicMember();
        
        private class CustomDynamicMember : DynamicMemberDescriptorBase
        {
            public CustomDynamicMember() : base(IdentifiableHelper.YamlSpecialId, typeof(Guid))
            {
                Order = -int.MaxValue;
            }

            public override object Get(object thisObject)
            {
                return IdentifiableHelper.GetId(thisObject);
            }

            public override void Set(object thisObject, object value)
            {
                IdentifiableHelper.SetId(thisObject, (Guid)value);
            }
            public override bool HasSet => true;
        }

        /// <summary>
        /// Filters attributes to replace <see cref="DataMemberAttribute"/> by <see cref="YamlMemberAttribute"/>
        /// </summary>
        private class AtributeRegistryFilter : AttributeRegistry
        {
            public AtributeRegistryFilter()
            {
                AttributeRemap = RemapToYaml;
            }

            private Attribute RemapToYaml(Attribute originalAttribute)
            {
                Attribute attribute = null;
                var memberAttribute = originalAttribute as DataMemberAttribute;
                if (memberAttribute != null)
                {
                    SerializeMemberMode mode;
                    switch (memberAttribute.Mode)
                    {
                        case DataMemberMode.Default:
                        case DataMemberMode.ReadOnly: // ReadOnly is better as default or content?
                            mode = SerializeMemberMode.Default;
                            break;
                        case DataMemberMode.Assign:
                            mode = SerializeMemberMode.Assign;
                            break;
                        case DataMemberMode.Content:
                            mode = SerializeMemberMode.Content;
                            break;
                        case DataMemberMode.Binary:
                            mode = SerializeMemberMode.Binary;
                            break;
                        case DataMemberMode.Never:
                            mode = SerializeMemberMode.Never;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    attribute = new YamlMemberAttribute(memberAttribute.Name, mode) { Order = memberAttribute.Order, Mask = memberAttribute.Mask };
                    //Trace.WriteLine(string.Format("Attribute remapped {0}", memberAttribute.Name));
                }
                else if (originalAttribute is DataMemberIgnoreAttribute)
                {
                    attribute = new YamlIgnoreAttribute();
                }
                else if (originalAttribute is DataContractAttribute)
                {
                    var alias = ((DataContractAttribute)originalAttribute).Alias;
                    if (!string.IsNullOrWhiteSpace(alias))
                    {
                        attribute = new YamlTagAttribute(alias);
                    }
                }
                else if (originalAttribute is DataStyleAttribute)
                {
                    switch (((DataStyleAttribute)originalAttribute).Style)
                    {
                        case DataStyle.Any:
                            attribute = new YamlStyleAttribute(YamlStyle.Any);
                            break;
                        case DataStyle.Compact:
                            attribute = new YamlStyleAttribute(YamlStyle.Flow);
                            break;
                        case DataStyle.Normal:
                            attribute = new YamlStyleAttribute(YamlStyle.Block);
                            break;
                    }
                }
                else if (originalAttribute is DataAliasAttribute)
                {
                    attribute = new YamlRemapAttribute(((DataAliasAttribute)originalAttribute).Name);
                }

                return attribute ?? originalAttribute;
            }
        }

        [ModuleInitializer]
        internal static void Initialize()
        {
            AssemblyRegistry.AssemblyRegistered += AssemblyRegistry_AssemblyRegistered;
            AssemblyRegistry.AssemblyUnregistered += AssemblyRegistry_AssemblyUnregistered;
            foreach (var assembly in AssemblyRegistry.FindAll())
            {
                RegisteredAssemblies.Add(assembly);
            }
        }

        private static void AssemblyRegistry_AssemblyRegistered(object sender, AssemblyRegisteredEventArgs e)
        {
            lock (Lock)
            {
                RegisteredAssemblies.Add(e.Assembly);

                // Reset the current serializer as the set of assemblies has changed
                globalSerializer = null;
                globalSerializerWithoutId = null;
            }
        }

        private static void AssemblyRegistry_AssemblyUnregistered(object sender, AssemblyRegisteredEventArgs e)
        {
            lock (Lock)
            {
                RegisteredAssemblies.Remove(e.Assembly);

                // Reset the current serializer as the set of assemblies has changed
                globalSerializer = null;
                globalSerializerWithoutId = null;
            }
        }
    }
}
