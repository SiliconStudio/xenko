using System;
using System.Collections.Generic;
using System.IO;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Serializers;
using SerializerContext = SiliconStudio.Core.Yaml.Serialization.SerializerContext;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// Default Yaml serializer used to serialize assets by default.
    /// </summary>
    public class AssetYamlSerializer : YamlSerializerBase
    {
        private readonly Logger Log = GlobalLogger.GetLogger(typeof(YamlSerializer).Name);
        private event Action<ObjectDescriptor, List<IMemberDescriptor>> PrepareMembersEvent;

        private Serializer globalSerializer;
        private Serializer globalSerializerWithoutId;

        public static AssetYamlSerializer Default { get; set; } = new AssetYamlSerializer();

        public event Action<ObjectDescriptor, List<IMemberDescriptor>> PrepareMembers
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
        /// <param name="expectedType">The expected type.</param>
        /// <param name="contextSettings">The context settings.</param>
        /// <returns>An instance of the YAML data.</returns>
        public object Deserialize(Stream stream, Type expectedType = null, SerializerContextSettings contextSettings = null)
        {
            bool aliasOccurred;
            PropertyContainer contextProperties;
            return Deserialize(stream, expectedType, contextSettings, out aliasOccurred, out contextProperties);
        }

        /// <summary>
        /// Deserializes an object from the specified stream (expecting a YAML string).
        /// </summary>
        /// <param name="stream">A YAML string from a stream .</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="contextSettings">The context settings.</param>
        /// <param name="aliasOccurred">if set to <c>true</c> a class/field/property/enum name has been renamed during deserialization.</param>
        /// <param name="contextProperties">A dictionary or properties that were generated during deserialization.</param>
        /// <returns>An instance of the YAML data.</returns>
        public object Deserialize(Stream stream, Type expectedType, SerializerContextSettings contextSettings, out bool aliasOccurred, out PropertyContainer contextProperties)
        {
            var serializer = GetYamlSerializer(true);
            SerializerContext context;
            var result = serializer.Deserialize(stream, expectedType, contextSettings, out context);
            aliasOccurred = context.HasRemapOccurred;
            contextProperties = context.Properties;
            return result;
        }

        /// <summary>
        /// Deserializes an object from the specified stream (expecting a YAML string).
        /// </summary>
        /// <param name="eventReader">A YAML event reader.</param>
        /// <param name="value">The value.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="contextProperties">A dictionary or properties that were generated during deserialization.</param>
        /// <param name="contextSettings">The context settings.</param>
        /// <returns>An instance of the YAML data.</returns>
        public object Deserialize(EventReader eventReader, object value, Type expectedType, out PropertyContainer contextProperties, SerializerContextSettings contextSettings = null)
        {
            var serializer = GetYamlSerializer();
            SerializerContext context;
            var result = serializer.Deserialize(eventReader, expectedType, value, contextSettings, out context);
            contextProperties = context.Properties;
            return result;
        }

        /// <summary>
        /// Deserializes an object from the specified stream (expecting a YAML string).
        /// </summary>
        /// <param name="stream">A YAML string from a stream .</param>
        /// <returns>An instance of the YAML data.</returns>
        public IEnumerable<T> DeserializeMultiple<T>(Stream stream)
        {
            var serializer = GetYamlSerializer();

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
        /// <param name="contextSettings">The context settings.</param>
        public void Serialize(IEmitter emitter, object instance, Type type, SerializerContextSettings contextSettings = null)
        {
            var serializer = GetYamlSerializer();
            serializer.Serialize(emitter, instance, type, contextSettings);
        }

        /// <summary>
        /// Serializes an object to specified stream in YAML format.
        /// </summary>
        /// <param name="stream">The stream to receive the YAML representation of the object.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="type">The expected type.</param>
        /// <param name="contextSettings">The context settings.</param>
        public void Serialize(Stream stream, object instance, Type type = null, SerializerContextSettings contextSettings = null)
        {
            var serializer = GetYamlSerializer();
            serializer.Serialize(stream, instance, type, contextSettings);
        }

        /// <summary>
        /// Gets the serializer settings.
        /// </summary>
        /// <returns>SerializerSettings.</returns>
        public SerializerSettings GetSerializerSettings()
        {
            return GetYamlSerializer().Settings;
        }

        /// <summary>
        /// Reset the assembly cache used by this class.
        /// </summary>
        public override void ResetCache()
        {
            lock (Lock)
            {
                // Reset the current serializer as the set of assemblies has changed
                globalSerializer = null;
                globalSerializerWithoutId = null;
            }
        }

        private Serializer GetYamlSerializer(bool generateIds = false)
        {
            // Cache serializer to improve performance
            var localSerializer = generateIds ? CreateSerializer(ref globalSerializer, true) : CreateSerializer(ref globalSerializerWithoutId, false);
            return localSerializer;
        }

        private Serializer CreateSerializer(ref Serializer localSerializer, bool generateIds)
        {
            // Early exit if already initialized
            if (localSerializer != null)
                return localSerializer;

            lock (Lock)
            {
                if (localSerializer == null)
                {
                    // var clock = Stopwatch.StartNew();

                    var config = new SerializerSettings
                    {
                        EmitAlias = false,
                        LimitPrimitiveFlowSequence = 0,
                        Attributes = new AttributeRegistry(),
                        PreferredIndent = 4,
                        EmitShortTypeName = true,
                        ComparerForKeySorting = new DefaultMemberComparer(),
                        PreSerializer = new ContextAttributeSerializer(),
                        PostSerializer = new ErrorRecoverySerializer(),
                        SerializerFactorySelector = new ProfileSerializerFactorySelector(YamlSerializerFactoryAttribute.Default, "Assets")
                    };

                    config.ChainedSerializerFactory = serializer =>
                    {
                        var routingSerializer = serializer.FindNext<RoutingSerializer>();
                        if (routingSerializer == null) throw new InvalidOperationException("RoutingSerializer expected in the chain of serializers");
                        // Prepend the ContextAttributeSerializer just before the routing serializer
                        routingSerializer.Prepend(new ContextAttributeSerializer());
                        // Prepend the ErrorRecoverySerializer at the beginning
                        routingSerializer.First.Prepend(new ErrorRecoverySerializer());
                    };
                    config.Attributes.PrepareMembersCallback += (objDesc, members) => PrepareMembersCallback(generateIds, objDesc, members);

                    for (int index = RegisteredAssemblies.Count - 1; index >= 0; index--)
                    {
                        var registeredAssembly = RegisteredAssemblies[index];
                        config.RegisterAssembly(registeredAssembly);
                    }

                    var newSerializer = new Serializer(config);
                    newSerializer.Settings.ObjectSerializerBackend = new AssetObjectSerializerBackend(TypeDescriptorFactory.Default);

                    // Log.Info("New YAML serializer created in {0}ms", clock.ElapsedMilliseconds);
                    localSerializer = newSerializer;
                }
            }

            return localSerializer;
        }

        private void PrepareMembersCallback(bool generateIds, ObjectDescriptor objDesc, List<IMemberDescriptor> memberDescriptors)
        {
            var type = objDesc.Type;

            if (generateIds)
            {
                if (ShadowId.IsTypeIdentifiable(type) && !typeof(IIdentifiable).IsAssignableFrom(type))
                {
                    memberDescriptors.Add(customDynamicMemberDescriptor);
                }
            }

            // Call custom callbacks to prepare members
            PrepareMembersEvent?.Invoke(objDesc, memberDescriptors);
        }

        private readonly CustomDynamicMember customDynamicMemberDescriptor = new CustomDynamicMember();

        // This class exists only for backward compatibility with previous ~Id. It can be removed once we drop backward support
        private class CustomDynamicMember : DynamicMemberDescriptorBase
        {
            public CustomDynamicMember() : base(ShadowId.YamlSpecialId, typeof(Guid), typeof(object))
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
    }
}
