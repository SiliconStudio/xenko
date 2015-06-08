// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SiliconStudio.Core.Reflection;
using AttributeRegistry = SharpYaml.Serialization.AttributeRegistry;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// Default Yaml serializer used to serialize assets by default.
    /// </summary>
    public static class YamlSerializer
    {
        // TODO: This code is not robust in case of reloading assemblies into the same process
        private static readonly HashSet<Assembly> RegisteredAssemblies = new HashSet<Assembly>();
        private static readonly object Lock = new object();
        private static Serializer globalSerializer;
        private static Serializer globalSerializerKeepOnlySealedOverrides;

        /// <summary>
        /// Deserializes an object from the specified stream (expecting a YAML string).
        /// </summary>
        /// <param name="stream">A YAML string from a stream .</param>
        /// <returns>An instance of the YAML data.</returns>
        public static object Deserialize(Stream stream)
        {
            var serializer = GetYamlSerializer(false);
            return serializer.Deserialize(stream);
        }

        /// <summary>
        /// Deserializes an object from the specified stream (expecting a YAML string).
        /// </summary>
        /// <param name="eventReader">A YAML event reader.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <returns>An instance of the YAML data.</returns>
        public static object Deserialize(EventReader eventReader, Type expectedType)
        {
            var serializer = GetYamlSerializer(false);
            return serializer.Deserialize(eventReader, expectedType);
        }

        /// <summary>
        /// Deserializes an object from the specified stream (expecting a YAML string).
        /// </summary>
        /// <param name="stream">A YAML string from a stream .</param>
        /// <returns>An instance of the YAML data.</returns>
        public static IEnumerable<T> DeserializeMultiple<T>(Stream stream)
        {
            var serializer = GetYamlSerializer(false);

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
        /// <param name="keepOnlySealedOverrides">if set to <c>true</c> [keep only sealed overrides].</param>
        public static void Serialize(IEmitter emitter, object instance, Type type, bool keepOnlySealedOverrides = false)
        {
            var serializer = GetYamlSerializer(keepOnlySealedOverrides);
            serializer.Serialize(emitter, instance, type);
        }


        /// <summary>
        /// Serializes an object to specified stream in YAML format.
        /// </summary>
        /// <param name="stream">The stream to receive the YAML representation of the object.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="keepOnlySealedOverrides">if set to <c>true</c> [keep only sealed overrides].</param>
        public static void Serialize(Stream stream, object instance, bool keepOnlySealedOverrides = false)
        {
            var serializer = GetYamlSerializer(keepOnlySealedOverrides);
            serializer.Serialize(stream, instance);
        }

        public static SerializerSettings GetSerializerSettings()
        {
            return GetYamlSerializer(false).Settings;
        }

        private static Serializer GetYamlSerializer(bool keepOnlySealedOverrides)
        {
            Serializer localSerializer;
            // Cache serializer to improve performance
            lock (Lock)
            {
                localSerializer = keepOnlySealedOverrides ? CreateSerializer(true, ref globalSerializerKeepOnlySealedOverrides) : CreateSerializer(false, ref globalSerializer);
            }
            return localSerializer;
        }

        private static Serializer CreateSerializer(bool keepOnlySealedOverrides, ref Serializer localSerializer)
        {
            if (localSerializer == null)
            {
                var config = new SerializerSettings()
                    {
                        EmitAlias = false,
                        LimitPrimitiveFlowSequence = 0,
                        Attributes = new AtributeRegistryFilter(),
                        PreferredIndent = 4,
                        EmitShortTypeName = true,
                    };

                foreach (var registeredAssembly in RegisteredAssemblies)
                {
                    config.RegisterAssembly(registeredAssembly);
                }

                localSerializer = new Serializer(config);
                localSerializer.Settings.ObjectSerializerBackend = new OverrideKeyMappingTransform(TypeDescriptorFactory.Default, keepOnlySealedOverrides);
            }

            return localSerializer;
        }

        /// <summary>
        /// Filters attributes to replace <see cref="DataMemberAttribute"/> by <see cref="YamlMemberAttribute"/>
        /// </summary>
        private class AtributeRegistryFilter : AttributeRegistry
        {
            public override List<Attribute> GetAttributes(System.Reflection.MemberInfo memberInfo, bool inherit = true)
            {
                var attributes = base.GetAttributes(memberInfo, inherit);
                for (int i = attributes.Count - 1; i >= 0; i--)
                {
                    var attribute = attributes[i] as DataMemberAttribute;
                    if (attribute != null)
                    {
                        SerializeMemberMode mode;
                        switch (attribute.Mode)
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
                        attributes[i] = new YamlMemberAttribute(attribute.Name, mode) { Order = attribute.Order };
                    }
                    else if (attributes[i] is DataMemberIgnoreAttribute)
                    {
                        attributes[i] = new YamlIgnoreAttribute();
                    }
                    else if (attributes[i] is DataContractAttribute)
                    {
                        var alias = ((DataContractAttribute)attributes[i]).Alias;
                        if (!string.IsNullOrWhiteSpace(alias))
                        {
                            attributes[i] = new YamlTagAttribute(alias);
                        }
                    }
                    else if (attributes[i] is DataStyleAttribute)
                    {
                        switch (((DataStyleAttribute)attributes[i]).Style)
                        {
                            case DataStyle.Any:
                                attributes[i] = new YamlStyleAttribute(YamlStyle.Any);
                                break;
                            case DataStyle.Compact:
                                attributes[i] = new YamlStyleAttribute(YamlStyle.Flow);
                                break;
                            case DataStyle.Normal:
                                attributes[i] = new YamlStyleAttribute(YamlStyle.Block);
                                break;
                        }
                    }
                }
                return attributes;
            }
        }

        [ModuleInitializer]
        internal static void Initialize()
        {
            AssemblyRegistry.AssemblyRegistered += AssemblyRegistry_AssemblyRegistered;
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
                globalSerializerKeepOnlySealedOverrides = null;
            }
        }
    }
}