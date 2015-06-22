// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Debugger.Target
{
    /// <summary>
    /// When serializing/deserializing Yaml for live objects, this serializer will handle those objects as reference (similar to Clone serializer).
    /// </summary>
    [YamlSerializerFactory]
    public class CloneReferenceSerializer : ContentReferenceSerializer
    {
        /// <summary>
        /// The list of live references during that serialization/deserialization cycle.
        /// </summary>
        [ThreadStatic] internal static List<object> References;

        public override bool CanVisit(Type type)
        {
            // TODO: Special case will be needed for Script (two-pass deserialization required)
            return base.CanVisit(type) || type == typeof(Entity) || typeof(Entity).IsAssignableFrom(type) || typeof(EntityComponent).IsAssignableFrom(type);
        }

        public override object ConvertFrom(ref ObjectContext context, Scalar fromScalar)
        {
            int index;
            if (!int.TryParse(fromScalar.Value, out index))
            {
                throw new YamlException(fromScalar.Start, fromScalar.End, "Unable to decode asset reference [{0}]. Expecting format GUID:LOCATION".ToFormat(fromScalar.Value));
            }
            return References[index];
        }

        public override string ConvertTo(ref ObjectContext objectContext)
        {
            // Add to Objects and return index
            var result = References.Count.ToString();
            References.Add(objectContext.Instance);
            return result;
        }
    }
}