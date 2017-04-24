// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Assets.Serializers
{
    [YamlSerializerFactory(YamlAssetProfile.Name)]
    public sealed class IdentifiableAssetPartReferenceSerializer : ScalarOrObjectSerializer
    {
        public override bool CanVisit(Type type)
        {
            return type == typeof(IdentifiableAssetPartReference);
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            Guid guid;
            if (!Guid.TryParse(fromScalar.Value, out guid))
            {
                throw new YamlException(fromScalar.Start, fromScalar.End, "Unable to decode asset part reference [{0}]. Expecting an ENTITY_GUID".ToFormat(fromScalar.Value));
            }

            var result = context.Instance as IdentifiableAssetPartReference ?? (IdentifiableAssetPartReference)(context.Instance = new IdentifiableAssetPartReference());
            result.Id = guid;

            return result;
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            return ((IdentifiableAssetPartReference)objectContext.Instance).Id.ToString();
        }
    }
}
