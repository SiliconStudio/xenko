// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;

using SiliconStudio.Core;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// A Yaml serializer for <see cref="AssetReference"/>
    /// </summary>
    [YamlSerializerFactory]
    internal class AssetReferenceSerializer : AssetScalarSerializerBase
    {
        public override bool CanVisit(Type type)
        {
            return typeof(AssetReference).IsAssignableFrom(type);
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            AssetReference assetReference;
            if (!AssetReference.TryParse(context.Descriptor.Type, fromScalar.Value, out assetReference))
            {
                throw new YamlException(fromScalar.Start, fromScalar.End, "Unable to decode asset reference [{0}]. Expecting format GUID:LOCATION".ToFormat(fromScalar.Value));
            }
            return assetReference;
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            return objectContext.Instance.ToString();
        }
    }
}
