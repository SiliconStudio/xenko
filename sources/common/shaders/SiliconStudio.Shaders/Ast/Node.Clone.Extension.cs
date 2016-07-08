// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Shaders.Utility;

namespace SiliconStudio.Shaders.Ast
{
    public class CloneContext : Dictionary<object, object>
    {
        private MemoryStream memoryStream;
        private BinarySerializationWriter writer;
        private BinarySerializationReader reader;
        private Dictionary<object, int> serializeReferences;
        private List<object> deserializeReferences;

        public CloneContext(CloneContext parent = null) : base(MemberSerializer.ObjectReferenceEqualityComparer.Default)
        {
            if (parent != null)
            {
                foreach (var item in parent)
                {
                    Add(item.Key, item.Value);
                }
            }

            // Setup
            memoryStream = new MemoryStream(4096);
            writer = new BinarySerializationWriter(memoryStream);
            reader = new BinarySerializationReader(memoryStream);

            writer.Context.SerializerSelector = SerializerSelector.AssetWithReuse;
            reader.Context.SerializerSelector = SerializerSelector.AssetWithReuse;

            serializeReferences = writer.Context.Tags.Get(MemberSerializer.ObjectSerializeReferences);
            deserializeReferences = reader.Context.Tags.Get(MemberSerializer.ObjectDeserializeReferences);
        }

        internal void DeepCollect<T>(T obj)
        {
            // Collect
            writer.SerializeExtended(obj, ArchiveMode.Serialize);

            // Register each reference found
            foreach (var serializeReference in serializeReferences)
            {
                this[serializeReference.Key] = serializeReference.Key;
            }

            // Reset stream and references
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.SetLength(0);

            serializeReferences.Clear();
        }

        internal T DeepClone<T>(T obj)
        {
            // Prepare previously collected references
            foreach (var reference in this)
            {
                serializeReferences.Add(reference.Key, deserializeReferences.Count);
                deserializeReferences.Add(reference.Value);
            }

            // Serialize
            writer.SerializeExtended(obj, ArchiveMode.Serialize);

            // Deserialize
            obj = default(T);
            memoryStream.Seek(0, SeekOrigin.Begin);
            reader.SerializeExtended(ref obj, ArchiveMode.Deserialize);

            // Reset stream and references
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.SetLength(0);

            serializeReferences.Clear();
            deserializeReferences.Clear();

            return obj;
        }
    }

    public static class DeepCloner
    {
        public static void DeepCollect<T>(T obj, CloneContext context)
        {
            context.DeepCollect(obj);
        }

        public static T DeepClone<T>(this T obj, CloneContext context = null)
        {
            // Setup contexts
            if (context == null)
                context = new CloneContext();

            return context.DeepClone(obj);
        }
    }
}