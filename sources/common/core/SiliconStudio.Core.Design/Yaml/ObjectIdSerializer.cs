// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Storage;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A Yaml serializer for <see cref="ObjectId"/>
    /// </summary>
    [YamlSerializerFactory(YamlSerializerFactoryAttribute.Default)]
    internal class ObjectIdSerializer : AssetScalarSerializerBase
    {
        public override bool CanVisit(Type type)
        {
            return type == typeof(ObjectId);
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            ObjectId objectId;
            ObjectId.TryParse(fromScalar.Value, out objectId);
            return objectId;
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            return ((ObjectId)objectContext.Instance).ToString();
        }
    }
}