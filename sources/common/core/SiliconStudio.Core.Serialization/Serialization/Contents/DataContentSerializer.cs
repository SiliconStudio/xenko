// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Core.Serialization.Contents
{
    /// <summary>
    /// ContentSerializer that simply defers serialization to low level serialization.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    public class DataContentSerializer<T> : ContentSerializerBase<T> where T : new()
    {
        public override void Serialize(ContentSerializerContext context, SerializationStream stream, ref T obj)
        {
            // Get serializer
            // TODO: Cache it? Need to make sure it doesn't end up being different between different serialization mode.
            var dataSerializer = stream.Context.SerializerSelector.GetSerializer<T>();

            // Serialize object
            stream.SerializeExtended(ref obj, context.Mode, dataSerializer);
        }
    }
}
