// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.IO;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Core.Storage
{
    /// <summary>
    /// A hash to uniquely identify data.
    /// </summary>
    public partial struct ObjectId
    {
        /// <summary>
        /// Computes a hash from an object using <see cref="BinarySerializationWriter"/>.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The hash of the object.</returns>
        public static ObjectId FromObject<T>(T obj)
        {
            byte[] buffer;
            return FromObject(obj, out buffer);
        }

        /// <summary>
        /// Computes a hash from an object using <see cref="BinarySerializationWriter" />.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="buffer">The buffer containing the serialized object.</param>
        /// <returns>The hash of the object.</returns>
        public static ObjectId FromObject<T>(T obj, [NotNull] out byte[] buffer)
        {
            var stream = new MemoryStream();
            var writer = new BinarySerializationWriter(stream);
            writer.Context.SerializerSelector = SerializerSelector.Asset;
            writer.Serialize(ref obj, ArchiveMode.Serialize);
            stream.Position = 0;
            buffer = stream.ToArray();
            return FromBytes(buffer);
        }
    }
}