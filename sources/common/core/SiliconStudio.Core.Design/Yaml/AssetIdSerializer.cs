// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Assets;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A Yaml serializer for <see cref="Guid"/>
    /// </summary>
    [YamlSerializerFactory(YamlSerializerFactoryAttribute.Default)]
    internal class AssetIdSerializer : AssetScalarSerializerBase
    {
        public override bool CanVisit(Type type)
        {
            return type == typeof(AssetId);
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            AssetId assetId;
            AssetId.TryParse(fromScalar.Value, out assetId);
            return assetId;
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            return ((AssetId)objectContext.Instance).ToString();
        }
    }
}