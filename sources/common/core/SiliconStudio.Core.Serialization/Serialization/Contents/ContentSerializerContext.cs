// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Serializers;

// Serializer for ContentSerializerContext.SerializeReferences()
namespace SiliconStudio.Core.Serialization.Contents
{
    // TODO: Many simplifications/cleaning (lot of leftover from old system)
    [DataSerializerGlobal(typeof(ListSerializer<ChunkReference>))]
    public class ContentSerializerContext
    {
        internal readonly List<ChunkReference> chunkReferences = new List<ChunkReference>();
        private readonly Dictionary<Type, int> objectsPerType = new Dictionary<Type, int>();
        private readonly HashSet<object> generatedUrlObjects = new HashSet<object>();
        private string generatedUrlPrefix;

        public enum AttachedReferenceSerialization
        {
            Unset,
            AsSerializableVersion,
            AsNull,
        }

        public static PropertyKey<ContentSerializerContext> ContentSerializerContextProperty = new PropertyKey<ContentSerializerContext>("ContentSerializerContext", typeof(ContentSerializerContext));
        public static PropertyKey<AttachedReferenceSerialization> SerializeAttachedReferenceProperty = new PropertyKey<AttachedReferenceSerialization>("SerializeAttachedReference", typeof(ContentSerializerContext));

        public ContentManager ContentManager { get; private set; }
        public string Url { get; protected set; }
        public ArchiveMode Mode { get; protected set; }

        public List<ContentReference> ContentReferences { get; set; }

        public bool RegenerateUrls { get; set; }

        internal ContentManager.AssetReference AssetReference { get; set; }
        public bool LoadContentReferences { get; set; }

        internal ContentSerializerContext(string url, ArchiveMode mode, ContentManager contentManager)
        {
            Url = url;
            Mode = mode;
            ContentManager = contentManager;
            ContentReferences = new List<ContentReference>();
            generatedUrlPrefix = Url + "/gen/";
        }

        public int AddContentReference(ContentReference contentReference)
        {
            if (contentReference == null)
                return ChunkReference.NullIdentifier;

            // TODO: This behavior should be controllable
            if (contentReference.State != ContentReferenceState.NeverLoad && contentReference.ObjectValue != null)
            {
                // Auto-generate URL if necessary
                BuildUrl(contentReference);
                //Executor.ProcessObject(this, contentReference.Type, contentReference);
                ContentReferences.Add(contentReference);
            }

            return AddChunkReference(contentReference.Location, contentReference.Type);
        }

        public ContentReference<T> GetContentReference<T>(int index) where T : class
        {
            if (index == ChunkReference.NullIdentifier)
                return null;

            var chunkReference = GetChunkReference(index);

            var contentReference = new ContentReference<T>(Guid.Empty, chunkReference.Location);

            ContentReferences.Add(contentReference);

            return contentReference;
        }

        public ChunkReference GetChunkReference(int index)
        {
            return chunkReferences[index];
        }

        public int AddChunkReference(string url, Type type)
        {
            // Starting search from the end is maybe more likely to hit quickly (and cache friendly)?
            for (int i = chunkReferences.Count - 1; i >= 0; --i)
            {
                var currentReference = chunkReferences[i];
                if (currentReference.Location == url && currentReference.ObjectType == type)
                {
                    return i;
                }
            }

            var reference = new ChunkReference(type, url);
            var index = chunkReferences.Count;
            chunkReferences.Add(reference);
            return index;
        }


        public void BuildUrl(ContentReference contentReference)
        {
            var content = contentReference.ObjectValue;
            string url = contentReference.Location;

            if (content == null)
                return;

            // If URL has been auto-generated previously, regenerates it (so that no collision occurs if item has been modified)
            if (url == null || (RegenerateUrls && url.StartsWith(generatedUrlPrefix) && !generatedUrlObjects.Contains(content)))
            {
                // Already registered?
                if (ContentManager.TryGetAssetUrl(content, out url))
                {
                    contentReference.Location = url;
                    return;
                }

                generatedUrlObjects.Add(content);

                // No URL, need to generate one.
                // Try to be as deterministic as possible (generated from root URL, type and index).
                var contentType = content.GetType();

                // Get and update current count
                int currentCount;
                objectsPerType.TryGetValue(contentType, out currentCount);
                objectsPerType[contentType] = ++currentCount;

                contentReference.Location = string.Format("{0}{1}_{2}", generatedUrlPrefix, content.GetType().Name, currentCount);
            }

            // Register it
            //if (contentReference.Location != null)
            //    ContentManager.RegisterAsset(contentReference.Location, contentReference.ObjectValue, serializationType, false);
        }

        public void SerializeContent(SerializationStream stream, IContentSerializer serializer, object objToSerialize)
        {
            stream.Context.SerializerSelector = ContentManager.Serializer.LowLevelSerializerSelector;
            serializer.Serialize(this, stream, objToSerialize);
        }

        public void SerializeReferences(SerializationStream stream)
        {
            var references = chunkReferences;
            stream.Context.SerializerSelector = ContentManager.Serializer.LowLevelSerializerSelector;
            stream.Serialize(ref references, Mode);
        }
    }
}
