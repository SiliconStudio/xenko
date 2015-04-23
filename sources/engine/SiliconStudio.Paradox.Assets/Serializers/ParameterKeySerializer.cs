// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Assets.Serializers
{
    /// <summary>
    /// A Yaml serializer for <see cref="ParameterKey"/>
    /// </summary>
    [YamlSerializerFactory]
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
                parameterKey = ParameterKeys.New<object>(null, fromScalar.Value);
                // Dont' throw an exception if keys was not found
                //throw new YamlException(fromScalar.Start, fromScalar.End, "Unable to find registered ParameterKey [{0}]".ToFormat(fromScalar.Value));
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