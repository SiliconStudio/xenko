// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Core.Serialization.Assets
{
    public sealed partial class AssetManager : IAssetManager
    {
        public static DatabaseFileProvider FileProvider { get { return GetFileProvider(); } }

        public static Func<DatabaseFileProvider> GetFileProvider { get; set; }

        public AssetSerializer Serializer { get; private set; }

        // If multiple object shares the same Url, they will be stored as a linked list (AssetReference.Next).
        private readonly Dictionary<ObjectId, AssetReference> loadedAssetsByUrl = new Dictionary<ObjectId, AssetReference>();

        private readonly Dictionary<object, AssetReference> loadedAssetsUrl = new Dictionary<object, AssetReference>();

        public AssetManager()
        {
            Serializer = new AssetSerializer { LowLevelSerializerSelector = SerializerSelector.Default };
        }

        public void Save(string url, object asset)
        {
            if (url == null) throw new ArgumentNullException("url");
            if (asset == null) throw new ArgumentNullException("asset");

            lock (loadedAssetsByUrl)
            {
                using (var profile = Profiler.Begin(AssetProfilingKeys.AssetSave))
                {
                    SerializeObject(url, asset, true);
                }
            }
        }

        public Stream OpenAsStream(string url, StreamFlags streamFlags)
        {
            return FileProvider.OpenStream(url, VirtualFileMode.Open, VirtualFileAccess.Read, streamFlags:streamFlags);
        }

        public T Load<T>(string url, AssetManagerLoaderSettings settings = null) where T : class
        {
            if (settings == null)
                settings = AssetManagerLoaderSettings.Default;

            if (url == null) throw new ArgumentNullException("url");

            lock (loadedAssetsByUrl)
            {
                using (var profile = Profiler.Begin(AssetProfilingKeys.AssetLoad, url))
                {
                    AssetReference assetReference;
                    return (T)DeserializeObject(null, out assetReference, url, typeof(T), settings);
                }
            }
        }

        public Task<T> LoadAsync<T>(string url, AssetManagerLoaderSettings settings = null) where T : class
        {
            return Task.Factory.StartNew(() => Load<T>(url, settings));
        }

        public bool TryGetAssetUrl(object obj, out string url)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            lock (loadedAssetsByUrl)
            {
                AssetReference assetReference;
                if (!loadedAssetsUrl.TryGetValue(obj, out assetReference))
                {
                    url = null;
                    return false;
                }

                url = assetReference.Url;
                return true;
            }
        }

        public void Unload(object obj)
        {
            lock (loadedAssetsByUrl)
            {
                AssetReference assetReference;
                if (!loadedAssetsUrl.TryGetValue(obj, out assetReference))
                    throw new InvalidOperationException("Asset not loaded through this AssetManager.");

                // Release reference
                DecrementReference(assetReference, true);
            }
        }

        private void PrepareSerializerContext(ContentSerializerContext contentSerializerContext, SerializerContext context)
        {
            context.Set(ContentSerializerContext.ContentSerializerContextProperty, contentSerializerContext);

            // Duplicate context from SerializerContextTags
            foreach (var property in Serializer.SerializerContextTags)
            {
                context.Tags.SetObject(property.Key, property.Value);
            }
        }

        internal object DeserializeObject(AssetReference parentAssetReference, out AssetReference assetReference, string url, Type objType, AssetManagerLoaderSettings settings, ConverterContext converterContext = null)
        {
            // Resolve URL
            ObjectId objectId;
            if (!FileProvider.AssetIndexMap.TryGetValue(url, out objectId))
                throw new InvalidOperationException(string.Format("Asset [{0}] not found.", url));

            // Try to find already loaded object
            if (loadedAssetsByUrl.TryGetValue(objectId, out assetReference))
            {
                while (assetReference != null && !objType.GetTypeInfo().IsAssignableFrom(assetReference.Object.GetType().GetTypeInfo()))
                {
                    assetReference = assetReference.Next;
                }

                if (assetReference != null)
                {
                    // Add reference
                    bool isRoot = parentAssetReference == null;
                    if (isRoot || parentAssetReference.References.Add(assetReference))
                    {
                        IncrementReference(assetReference, isRoot);
                    }

                    return assetReference.Object;
                }
            }

            if (!FileProvider.FileExists(url))
                throw new InvalidOperationException(string.Format("Asset [{0}] not found.", url));

            ContentSerializerContext contentSerializerContext;
            object result;

            // Open asset binary stream
            using (var stream = FileProvider.OpenStream(url, VirtualFileMode.Open, VirtualFileAccess.Read))
            {
                // File does not exist
                // TODO/Benlitz: Add a log entry for that, it's not expected to happen
                if (stream == null)
                    return null;

                Type headerObjType = null;

                // Read header
                var streamReader = new BinarySerializationReader(stream);
                var chunkHeader = ChunkHeader.Read(streamReader);
                if (chunkHeader != null)
                {
                    headerObjType = Type.GetType(chunkHeader.Type);
                }

                // Find serializer
                var serializer = Serializer.GetSerializer(headerObjType, objType);
                if (serializer == null)
                    throw new InvalidOperationException(string.Format("Content serializer for {0}/{1} could not be found.", headerObjType, objType));
                contentSerializerContext = new ContentSerializerContext(url, ArchiveMode.Deserialize, this);

                // Read chunk references
                if (chunkHeader != null && chunkHeader.OffsetToReferences != -1)
                {
                    // Seek to where references are stored and deserialize them
                    streamReader.NativeStream.Seek(chunkHeader.OffsetToReferences, SeekOrigin.Begin);
                    contentSerializerContext.SerializeReferences(streamReader);
                    streamReader.NativeStream.Seek(chunkHeader.OffsetToObject, SeekOrigin.Begin);
                }

                // Create AssetReference
                assetReference = new AssetReference(objectId, url, parentAssetReference == null);
                contentSerializerContext.AssetReference = assetReference;

                result = serializer.Construct(contentSerializerContext);

                PrepareSerializerContext(contentSerializerContext, streamReader.Context);
                contentSerializerContext.ConverterContext = converterContext;

                result = contentSerializerContext.SerializeContent(streamReader, serializer, result);

                SetAssetObject(assetReference, result);

                // Add reference
                if (parentAssetReference != null)
                {
                    parentAssetReference.References.Add(assetReference);
                }
            }

            if (settings.LoadContentReferences)
            {
                // Process content references
                // TODO: Should we work at ChunkReference level?
                foreach (var contentReference in contentSerializerContext.ContentReferences)
                {
                    bool shouldBeLoaded = true;

                    AssetReference childReference;

                    if (settings.ContentFilter != null)
                        settings.ContentFilter(contentReference, ref shouldBeLoaded);

                    if (shouldBeLoaded)
                    {
                        contentReference.ObjectValue = DeserializeObject(assetReference, out childReference, contentReference.Location, contentReference.Type, settings);
                    }
                }
            }

            return result;
        }

        internal object DeserializeObjectRecursive(AssetReference parentAssetReference, out AssetReference assetReference,
            string url, Type objType, AssetManagerLoaderSettings settings, ContentSerializerContext otherContext, Stream stream, Type headerObjType, ConverterContext converterContext = null)
        {
            // Resolve URL
            ObjectId objectId;
            if (!FileProvider.AssetIndexMap.TryGetValue(url, out objectId))
                throw new InvalidOperationException(string.Format("Asset [{0}] not found.", url));

            // Find serializer
            var serializer = Serializer.GetSerializer(headerObjType, objType);
            if (serializer == null)
                throw new InvalidOperationException(string.Format("Content serializer for {0}/{1} could not be found.", headerObjType, objType));
            var contentSerializerContext = new ContentSerializerContext(url, ArchiveMode.Deserialize, this);

            contentSerializerContext.chunkReferences.AddRange(otherContext.chunkReferences);

            // Create AssetReference
            assetReference = new AssetReference(objectId, url, parentAssetReference == null);
            contentSerializerContext.AssetReference = assetReference;

            var result = serializer.Construct(contentSerializerContext);

            var streamReader = new BinarySerializationReader(stream);

            PrepareSerializerContext(contentSerializerContext, streamReader.Context);
            contentSerializerContext.ConverterContext = converterContext;

            result = contentSerializerContext.SerializeContent(streamReader, serializer, result);

            SetAssetObject(assetReference, result);

            // Add reference
            if (parentAssetReference != null)
            {
                parentAssetReference.References.Add(assetReference);
            }
            
            return result;
        }

        private void SerializeObject(string url, object obj, bool publicReference)
        {
            // Don't create context in case we don't want to serialize referenced objects
            //if (!SerializeReferencedObjects && obj != RootObject)
            //    return null;

            // Already saved?
            // TODO: Ref counting? Should we change it on save? Probably depends if we cache or not.
            if (loadedAssetsUrl.ContainsKey(obj))
                return;

            var serializer = Serializer.GetSerializer(null, obj.GetType());
            if (serializer == null)
                throw new InvalidOperationException(string.Format("Content serializer for {0} could not be found.", obj.GetType()));

            var contentSerializerContext = new ContentSerializerContext(url, ArchiveMode.Serialize, this);

            using (var stream = FileProvider.OpenStream(url, VirtualFileMode.Create, VirtualFileAccess.Write))
            {
                var streamWriter = new BinarySerializationWriter(stream);
                PrepareSerializerContext(contentSerializerContext, streamWriter.Context);

                ChunkHeader header = null;

                // Allocate space in the stream, and also include header version in the hash computation, which is better
                // If serialization type is null, it means there should be no header.
                var serializationType = serializer.SerializationType;
                if (serializationType != null)
                {
                    header = new ChunkHeader();
                    header.Type = serializer.SerializationType.AssemblyQualifiedName;
                    header.Write(streamWriter);
                    header.OffsetToObject = (int)streamWriter.NativeStream.Position;
                }

                contentSerializerContext.SerializeContent(streamWriter, serializer, obj);

                // Write references and updated header
                if (header != null)
                {
                    header.OffsetToReferences = (int)streamWriter.NativeStream.Position;
                    contentSerializerContext.SerializeReferences(streamWriter);

                    // Move back to the pre-allocated header position in the steam
                    stream.Seek(0, SeekOrigin.Begin);

                    // Write actual header.
                    header.Write(new BinarySerializationWriter(stream));
                }
            }

            // Resolve URL
            ObjectId objectId;
            if (!FileProvider.AssetIndexMap.TryGetValue(url, out objectId))
                throw new InvalidOperationException(string.Format("Asset [{0}] not found.", url));

            var assetReference = new AssetReference(objectId, url, publicReference);
            contentSerializerContext.AssetReference = assetReference;
            SetAssetObject(assetReference, obj);

            // Process content references
            // TODO: Should we work at ChunkReference level?
            foreach (var contentReference in contentSerializerContext.ContentReferences)
            {
                if (contentReference.ObjectValue != null)
                    SerializeObject(contentReference.Location, contentReference.ObjectValue, false);
            }
        }

        /// <summary>
        /// Sets AssetReference.Object, and updates loadedAssetByUrl collection.
        /// </summary>
        internal void SetAssetObject(AssetReference assetReference, object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            if (assetReference.Object != null)
            {
                if (assetReference.Object != obj)
                    throw new InvalidOperationException("SetAssetObject has already been called with a different object");

                return;
            }

            var objectId = assetReference.ObjectId;
            assetReference.Object = obj;

            lock (loadedAssetsByUrl)
            {
                AssetReference previousAssetReference;

                if (loadedAssetsByUrl.TryGetValue(objectId, out previousAssetReference))
                {
                    assetReference.Next = previousAssetReference.Next;
                    assetReference.Prev = previousAssetReference;

                    if (previousAssetReference.Next != null)
                        previousAssetReference.Next.Prev = assetReference;
                    previousAssetReference.Next = assetReference;
                }
                else
                {
                    loadedAssetsByUrl[objectId] = assetReference;
                }

                loadedAssetsUrl[obj] = assetReference;

                // TODO: Currently here so that ContentReference.ObjectValue later keeps its Url.
                // Need some reorganization?
                UrlServices.SetUrl(obj, assetReference.Url);
            }
        }
    }
}
