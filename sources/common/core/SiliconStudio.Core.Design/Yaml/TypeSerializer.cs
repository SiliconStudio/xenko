// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SharpYaml.Events;
using SharpYaml.Serialization;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// A Yaml serializer for <see cref="Type"/>
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
            // Remove tag info (i.e. RuntimeType inherits from Type)
            // TODO: We don't support properly the case where it is stored in an object (we do want the !System.Type tag in this case)
            // this seems to require some changes on SharpYaml side
            return context.SerializerContext.TagFromType((Type)context.Instance);
        }
    }
}