// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A Yaml serializer for <see cref="UDirectory"/>
    /// </summary>
    [YamlSerializerFactory(YamlSerializerFactoryAttribute.Default)]
    internal class UDirectorySerializer : AssetScalarSerializerBase
    {
        public override bool CanVisit(Type type)
        {
            return typeof(UDirectory) == type;
        }

        [NotNull]
        public override object ConvertFrom(ref ObjectContext context, [NotNull] Scalar fromScalar)
        {
            return new UDirectory(fromScalar.Value);
        }

        [NotNull]
        public override string ConvertTo(ref ObjectContext objectContext)
        {
            var path = ((UDirectory)objectContext.Instance);
            return path.FullPath;
        }
    }
}
