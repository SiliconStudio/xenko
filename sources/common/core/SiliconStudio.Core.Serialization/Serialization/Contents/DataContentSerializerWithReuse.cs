// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core.Serialization.Contents
{
    /// <summary>
    /// ContentSerializer that simply defers serialization to low level serialization, with <see cref="SerializerSelector.ReuseReferences"/> set to true.
    /// </summary>
    /// <typeparam name="T">The type to serialize.</typeparam>
    public class DataContentSerializerWithReuse<T> : DataContentSerializer<T> where T : new()
    {
        public override void Serialize(ContentSerializerContext context, SerializationStream stream, T obj)
        {
            // Save and change serializer selector to the optimized one
            var previousSerializerSelector = stream.Context.SerializerSelector;
            stream.Context.SerializerSelector = context.ContentManager.Serializer.LowLevelSerializerSelectorWithReuse;

            // Serialize
            base.Serialize(context, stream, obj);

            // Restore serializer selector
            stream.Context.SerializerSelector = previousSerializerSelector;
        }
    }
}