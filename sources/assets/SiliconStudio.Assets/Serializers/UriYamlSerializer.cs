// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// A Yaml serializer for <see cref="PackageVersion"/>
    /// </summary>
    [YamlSerializerFactory(YamlAssetProfile.Name)]
    internal class UriYamlSerializer : AssetScalarSerializerBase
    {
        public override bool CanVisit(Type type)
        {
            return typeof(System.Uri) == type;
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            return new System.Uri(fromScalar.Value);
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            return objectContext.Instance.ToString();
        }
    }
}
