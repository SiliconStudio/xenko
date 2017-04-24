// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Assets.Serializers
{
    /// <summary>
    /// A Yaml serializer for <see cref="ParameterKey"/>
    /// </summary>
    [YamlSerializerFactory(YamlAssetProfile.Name)]
    internal class ParameterKeySerializer : AssetScalarSerializerBase
    {
        public override bool CanVisit(Type type)
        {
            return typeof(ParameterKey).IsAssignableFrom(type);
        }

        public override object ConvertFrom(ref ObjectContext objectContext, Scalar fromScalar)
        {
            var parameterKey = ParameterKeys.FindByName(fromScalar.Value);
            if (parameterKey == null)
            {
                throw new YamlException(fromScalar.Start, fromScalar.End, "Unable to find registered ParameterKey [{0}]".ToFormat(fromScalar.Value));
            }
            return parameterKey;
        }

        protected override void WriteScalar(ref ObjectContext objectContext, ScalarEventInfo scalar)
        {
            // TODO: if ParameterKey is written to an object, It will not serialized a tag
            scalar.Tag = null;
            scalar.IsPlainImplicit = true;
            base.WriteScalar(ref objectContext, scalar);
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            return ((ParameterKey)objectContext.Instance).Name;
        }
    }
}
