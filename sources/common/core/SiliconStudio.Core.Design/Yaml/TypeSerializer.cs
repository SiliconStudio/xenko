// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SharpYaml.Events;
using SharpYaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A Yaml serializer for <see cref="UDirectory"/>
    /// </summary>
    [YamlSerializerFactory]
    internal class TypeSerializer : AssetScalarSerializerBase
    {
        public override bool CanVisit(Type type)
        {
            return typeof(Type).IsAssignableFrom(type);
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            bool remapped;
            return context.SerializerContext.TypeFromTag(fromScalar.Value, out remapped);
        }

        public override string ConvertTo(ref ObjectContext context)
        {
            return context.SerializerContext.TagFromType((Type)context.Instance);
        }
    }
}