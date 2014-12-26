// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Serializers
{
    [YamlSerializerFactory]
    internal class ContentReferenceSerializer : AssetScalarSerializerBase
    {
        public static readonly ContentReferenceSerializer Default = new ContentReferenceSerializer();

        public override bool CanVisit(Type type)
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
            if (!AssetReference.TryParse(fromScalar.Value, out guid, out location))
            {
                throw new YamlException(fromScalar.Start, fromScalar.End, "Unable to decode asset reference [{0}]. Expecting format GUID:LOCATION".ToFormat(fromScalar.Value));
            }

            return AttachedReferenceManager.CreateSerializableVersion(context.Descriptor.Type, guid, location);
        }
        public override string ConvertTo(ref ObjectContext objectContext)
        {
            var attachedReference = AttachedReferenceManager.GetAttachedReference(objectContext.Instance);
            if (attachedReference == null)
                throw new YamlException(string.Format("Unable to extract asset reference from object [{0}]", objectContext.Instance));

            return string.Format("{0}:{1}", attachedReference.Id, attachedReference.Url);
        }
    }
}