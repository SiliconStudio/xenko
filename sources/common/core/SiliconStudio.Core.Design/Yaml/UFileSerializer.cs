// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A Yaml serializer for <see cref="UFile"/>.
    /// </summary>
    [YamlSerializerFactory(YamlSerializerFactoryAttribute.Default)]
    internal class UFileSerializer : AssetScalarSerializerBase
    {
        public override bool CanVisit(Type type)
        {
            return typeof(UFile) == type;
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            return new UFile(fromScalar.Value);
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            var path = ((UFile)objectContext.Instance);
            return path.FullPath;
        }
    }
}
