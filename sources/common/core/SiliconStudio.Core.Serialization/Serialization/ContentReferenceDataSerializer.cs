// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Core.Serialization
{
    internal sealed class ContentReferenceDataSerializer<T> : DataSerializer<ContentReference<T>> where T : class
    {
        public override void Serialize(ref ContentReference<T> reference, ArchiveMode mode, SerializationStream stream)
        {
            var contentSerializerContext = stream.Context.Get(ContentSerializerContext.ContentSerializerContextProperty);
            if (contentSerializerContext != null)
            {
                if (mode == ArchiveMode.Serialize)
                {
                    int index = contentSerializerContext.AddContentReference(reference);
                    stream.Write(index);
                }
                else
                {
                    int index = stream.ReadInt32();
                    reference = contentSerializerContext.GetContentReference<T>(index);
                }
            }
            else
            {
                // This case will happen when serializing build engine command hashes: we still want Location to still be written
                if (mode == ArchiveMode.Serialize)
                {
                    {
                        // This case will happen when serializing build engine command hashes: we still want Location to still be written
                        stream.Write(reference.Location);
                    }
                }
                else
                {
                    // No real case yet
                    throw new NotSupportedException();
                }
            }
        }
    }
}
