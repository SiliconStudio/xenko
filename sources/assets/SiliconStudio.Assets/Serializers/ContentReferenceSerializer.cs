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
            return (typeof(ContentReference).IsAssignableFrom(type));
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            var contentReference = (ContentReference)context.Instance ?? (ContentReference)Activator.CreateInstance(context.Descriptor.Type);

            Guid guid;
            UFile location;
            if (!AssetReference.TryParse(fromScalar.Value, out guid, out location))
            {
                throw new YamlException(fromScalar.Start, fromScalar.End, "Unable to decode asset reference [{0}]. Expecting format GUID:LOCATION".ToFormat(fromScalar.Value));
            }

            contentReference.Id = guid;
            contentReference.Location = location;
            return contentReference;
        }
        public override string ConvertTo(ref ObjectContext objectContext)
        {
            var contentReference = ((ContentReference)objectContext.Instance);
            return string.Format("{0}:{1}", contentReference.Id, contentReference.Location);
        }
    }
}