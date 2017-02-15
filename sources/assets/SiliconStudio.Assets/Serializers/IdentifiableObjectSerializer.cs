using System;
using System.Collections.Generic;
using SiliconStudio.Assets.Yaml;
using SiliconStudio.Core;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// A serializer for <see cref="IIdentifiable"/> instances, that can either serialize them directly or as an object reference.
    /// </summary>
    public class IdentifiableObjectSerializer : ScalarOrObjectSerializer
    {
        public const string Prefix = "ref!! ";

        public override bool CanVisit(Type type)
        {
            return typeof(IIdentifiable).IsAssignableFrom(type);
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            Guid identifier;
            if (!TryParse(fromScalar.Value, out identifier))
            {
                throw new YamlException($"Unable to deserialize reference: [{fromScalar.Value}]");
            }

            // Add the path to the currently deserialized object to the list of object references
            YamlAssetMetadata<Guid> objectReferences;
            if (!context.SerializerContext.Properties.TryGetValue(AssetObjectSerializerBackend.ObjectReferencesKey, out objectReferences))
            {
                objectReferences = new YamlAssetMetadata<Guid>();
                context.SerializerContext.Properties.Add(AssetObjectSerializerBackend.ObjectReferencesKey, objectReferences);
            }
            var path = AssetObjectSerializerBackend.GetCurrentPath(ref context, true);
            objectReferences.Set(path, identifier);

            // Return default(T)
            return !context.Descriptor.Type.IsValueType ? null : Activator.CreateInstance(context.Descriptor.Type);
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            var identifiable = (IIdentifiable)objectContext.Instance;
            return $"{Prefix}{identifiable.Id}";
        }

        protected override void WriteScalar(ref ObjectContext objectContext, ScalarEventInfo scalar)
        {
            // Remove the tag if one was added, which might happen if the concrete type is different from the container type.
            scalar.Tag = null;
            scalar.IsPlainImplicit = true;
            base.WriteScalar(ref objectContext, scalar);
        }

        protected override bool ShouldSerializeAsScalar(ref ObjectContext objectContext)
        {
            YamlAssetMetadata<Guid> objectReferences;
            if (!objectContext.SerializerContext.Properties.TryGetValue(AssetObjectSerializerBackend.ObjectReferencesKey, out objectReferences))
                return false;

            var path = AssetObjectSerializerBackend.GetCurrentPath(ref objectContext, true);
            return objectReferences.TryGet(path) != Guid.Empty;
        }

        private static bool TryParse(string text, out Guid identifier)
        {
            if (!text.StartsWith(Prefix))
            {
                identifier = Guid.Empty;
                return false;
            }
            return Guid.TryParse(text.Substring(Prefix.Length), out identifier);
        }
    }
}
