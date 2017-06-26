// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.Assets.Serializers
{
    /// <summary>
    /// A Yaml serializer for <see cref="ItemId"/> without associated data.
    /// </summary>
    [YamlSerializerFactory(YamlAssetProfile.Name)]
    internal class SceneCameraSlotIdSerializer : AssetScalarSerializerBase
    {
        public override bool CanVisit(Type type)
        {
            return type == typeof(SceneCameraSlotId);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            Guid id;
            Guid.TryParse(fromScalar.Value, out id);
            return new SceneCameraSlotId(id);
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            var slot = (SceneCameraSlotId)objectContext.Instance;
            return slot.Id.ToString();
        }
    }
}
