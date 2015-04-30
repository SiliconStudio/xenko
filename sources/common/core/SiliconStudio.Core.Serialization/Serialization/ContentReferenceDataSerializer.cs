// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Core.Serialization
{
    /// <summary>
    /// Serialize object with its underlying Id and Location, and use <see cref="Assets.AssetManager"/> to generate a separate chunk.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ReferenceSerializer<T> : DataSerializer<T> where T : class
    {
        public override void Serialize(ref T obj, ArchiveMode mode, SerializationStream stream)
        {
            var referenceSerialization = stream.Context.Get(ContentSerializerContext.SerializeAttachedReferenceProperty);
            var contentSerializerContext = stream.Context.Get(ContentSerializerContext.ContentSerializerContextProperty);
            if (contentSerializerContext != null)
            {
                if (mode == ArchiveMode.Serialize)
                {
                    var contentReference = new ContentReference<T> { Value = obj };
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
                        var assetReference = contentSerializerContext.AssetManager.FindDeserializedObject(contentReference.Location, typeof(T));
                        if (assetReference != null)
                        {
                            obj = (T)assetReference.Object;
                            if (obj != null)
                                contentReference.Value = obj;
                        }
                    }

                    if (obj == null)
                    {
                        // First time, let's create it
                        obj = (T)AttachedReferenceManager.CreateSerializableVersion(typeof(T), contentReference.Id, contentReference.Location);
                        contentSerializerContext.AssetManager.RegisterDeserializedObject(contentReference.Location, obj);
                        contentReference.Value = obj;
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
                    if (attachedReference == null || attachedReference.Url == null)
                        throw new InvalidOperationException("Error when serializing reference.");

                    // TODO: Do not use string
                    stream.Write(obj.GetType().AssemblyQualifiedName);
                    stream.Write(attachedReference.Id);
                    stream.Write(attachedReference.Url);
                }
                else
                {
                    var type = Type.GetType(stream.ReadString());
                    var id = stream.Read<Guid>();
                    var url = stream.ReadString();

                    obj = (T)AttachedReferenceManager.CreateSerializableVersion(type, id, url);
                }
            }
            else
            {
                // This case will happen when serializing build engine command hashes: we still want Location to still be written
                if (mode == ArchiveMode.Serialize)
                {
                    // This case will happen when serializing build engine command hashes: we still want Location to still be written
                    var attachedReference = AttachedReferenceManager.GetAttachedReference(obj);
                    if (attachedReference == null || attachedReference.Url == null)
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

    public sealed class ContentReferenceDataSerializer<T> : DataSerializer<ContentReference<T>> where T : class
    {
        public override void Serialize(ref ContentReference<T> contentReference, ArchiveMode mode, SerializationStream stream)
        {
            var contentSerializerContext = stream.Context.Get(ContentSerializerContext.ContentSerializerContextProperty);
            if (contentSerializerContext != null)
            {
                if (mode == ArchiveMode.Serialize)
                {
                    int index = contentSerializerContext.AddContentReference(contentReference);
                    stream.Write(index);
                }
                else
                {
                    int index = stream.ReadInt32();
                    contentReference = contentSerializerContext.GetContentReference<T>(index);
                }
            }
            else
            {
                // This case will happen when serializing build engine command hashes: we still want Location to still be written
                if (mode == ArchiveMode.Serialize)
                {
                    {
                        // This case will happen when serializing build engine command hashes: we still want Location to still be written
                        stream.Write(contentReference.Location);
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