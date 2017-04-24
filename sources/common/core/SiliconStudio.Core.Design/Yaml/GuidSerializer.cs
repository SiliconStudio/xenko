// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A Yaml serializer for <see cref="Guid"/>
    /// </summary>
    [YamlSerializerFactory(YamlSerializerFactoryAttribute.Default)]
    internal class GuidSerializer : AssetScalarSerializerBase
    {
        static GuidSerializer()
        {
            TypeDescriptorFactory.Default.AttributeRegistry.Register(typeof(Guid), new DataContractAttribute("Guid"));
        }

        public override bool CanVisit(Type type)
        {
            return type == typeof(Guid);
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            Guid guid;
            Guid.TryParse(fromScalar.Value, out guid);
            return guid;
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            return ((Guid)objectContext.Instance).ToString();
        }
    }
}
