// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Serializers
{
    [YamlSerializerFactory]
    public class ContentReferenceSerializer : AssetScalarSerializerBase
    {
        public static readonly ContentReferenceSerializer Default = new ContentReferenceSerializer();

        public override bool CanVisit(Type type)
        {
            return IsReferenceType(type);
        }

        public static bool IsReferenceType(Type type)
        {
            // TODO: Quite inefficient, probably need an attribute
            var serializer = SerializerSelector.AssetWithReuse.GetSerializer(type);
            if (serializer == null)
                return false;

            var serializerType = serializer.GetType();
            return serializerType.IsGenericType && serializerType.GetGenericTypeDefinition() == typeof(ReferenceSerializer<>);
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            Guid guid;
            UFile location;
            Guid referenceId;
            if (!AssetReference.TryParse(fromScalar.Value, out referenceId, out guid, out location))
            {
                throw new YamlException(fromScalar.Start, fromScalar.End, "Unable to decode asset reference [{0}]. Expecting format GUID:LOCATION".ToFormat(fromScalar.Value));
            }

            var instance = AttachedReferenceManager.CreateProxyObject(context.Descriptor.Type, guid, location);

            // If the referenceId is empty, force its creation, else attach it to the reference
            if (referenceId == Guid.Empty)
            {

                IdentifiableHelper.GetId(instance);
            }
            else
            {
                IdentifiableHelper.SetId(instance, referenceId);
            }
            return instance;
        }
        public override string ConvertTo(ref ObjectContext objectContext)
        {
            var attachedReference = AttachedReferenceManager.GetAttachedReference(objectContext.Instance);
            if (attachedReference == null)
                throw new YamlException($"Unable to extract asset reference from object [{objectContext.Instance}]");

            var referenceId = IdentifiableHelper.GetId(objectContext.Instance);
            return $"{referenceId}/{attachedReference.Id}:{attachedReference.Url}";
        }
    }
}
