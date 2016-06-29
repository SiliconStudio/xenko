// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Core.Serialization
{
    /// <summary>
    /// Serialize object with its underlying Id and Location, and use <see cref="ContentManager"/> to generate a separate chunk.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ReferenceSerializer<T> : DataSerializer<T> where T : class
    {
        private IContentSerializer cachedContentSerializer;

        public override void Serialize(ref T obj, ArchiveMode mode, SerializationStream stream)
        {
            var referenceSerialization = stream.Context.Get(ContentSerializerContext.SerializeAttachedReferenceProperty);
            var contentSerializerContext = stream.Context.Get(ContentSerializerContext.ContentSerializerContextProperty);
            if (contentSerializerContext != null)
            {
                if (mode == ArchiveMode.Serialize)
                {
                    var contentReference = new ContentReference<T>(obj);
                    int index = contentSerializerContext.AddContentReference(contentReference);
                    stream.Write(index);
                }
                else
                {
                    int index = stream.ReadInt32();
                    var contentReference = contentSerializerContext.GetContentReference<T>(index);
                    obj = contentReference.Value;
                    if (obj == null)
                    {
                        // Check if already deserialized
                        var assetReference = contentSerializerContext.ContentManager.FindDeserializedObject(contentReference.Location, typeof(T));
                        if (assetReference != null)
                        {
                            obj = (T)assetReference.Object;
                            if (obj != null)
                                contentReference.Value = obj;
                        }
                    }

                    if (obj == null && contentSerializerContext.LoadContentReferences)
                    {
                        var contentSerializer = cachedContentSerializer ?? (cachedContentSerializer = contentSerializerContext.ContentManager.Serializer.GetSerializer(null, typeof(T)));
                        if (contentSerializer == null)
                        {
                            // Need to read chunk header to know actual type (note that we can't cache it in cachedContentSerializer as it depends on content)
                            var chunkHeader = contentSerializerContext.ContentManager.ReadChunkHeader(contentReference.Location);
                            if (chunkHeader == null || (contentSerializer = contentSerializerContext.ContentManager.Serializer.GetSerializer(AssemblyRegistry.GetType(chunkHeader.Type), typeof(T))) == null)
                                throw new InvalidOperationException($"Could not find a valid content serializer for {typeof(T)} when loading {contentReference.Location}");
                        }

                        // First time, let's create it
                        if (contentSerializerContext.ContentManager.Exists(contentReference.Location))
                        {
                            obj = (T)contentSerializer.Construct(contentSerializerContext);
                            contentSerializerContext.ContentManager.RegisterDeserializedObject(contentReference.Location, obj);
                            contentReference.Value = obj;
                        }
                    }
                }
            }
            else if (referenceSerialization == ContentSerializerContext.AttachedReferenceSerialization.AsNull)
            {
                if (mode == ArchiveMode.Deserialize)
                {
                    obj = default(T);
                }
            }
            else if (referenceSerialization == ContentSerializerContext.AttachedReferenceSerialization.AsSerializableVersion)
            {
                if (mode == ArchiveMode.Serialize)
                {
                    // This case will happen when serializing build engine command hashes: we still want Location to still be written
                    var attachedReference = AttachedReferenceManager.GetAttachedReference(obj);
                    if (attachedReference?.Url == null)
                        throw new InvalidOperationException("Error when serializing reference.");

                    // TODO: Do not use string
                    stream.Write(obj.GetType().AssemblyQualifiedName);
                    stream.Write(attachedReference.Id);
                    stream.Write(attachedReference.Url);
                }
                else
                {
                    var type = AssemblyRegistry.GetType(stream.ReadString());
                    var id = stream.Read<Guid>();
                    var url = stream.ReadString();

                    obj = (T)AttachedReferenceManager.CreateProxyObject(type, id, url);
                }
            }
            else
            {
                // This case will happen when serializing build engine command hashes: we still want Location to still be written
                if (mode == ArchiveMode.Serialize)
                {
                    // This case will happen when serializing build engine command hashes: we still want Location to still be written
                    var attachedReference = AttachedReferenceManager.GetAttachedReference(obj);
                    if (attachedReference?.Url == null)
                        throw new InvalidOperationException("Error when serializing reference.");

                    stream.Write(attachedReference.Url);
                }
                else
                {
                    // No real case yet
                    throw new NotSupportedException();
                }
            }
        }
    }

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
