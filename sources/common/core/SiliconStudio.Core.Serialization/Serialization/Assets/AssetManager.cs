// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Core.Serialization.Assets
{
    public sealed partial class AssetManager : IAssetManager
    {
        private static readonly Logger Log = GlobalLogger.GetLogger("AssetManager");

        public static DatabaseFileProvider FileProvider
        {
            get
            {
                // Don't try to call GetFileProvider if it is null
                if (GetFileProvider == null)
                {
                    return null;
                }
                return GetFileProvider();
            }
        }

        public static Func<DatabaseFileProvider> GetFileProvider { get; set; }

        public AssetSerializer Serializer { get; private set; }

        // If multiple object shares the same Url, they will be stored as a linked list (AssetReference.Next).
        // TODO: Check how to expose this publicly in a nice way
        public readonly Dictionary<ObjectId, AssetReference> loadedAssetsByUrl = new Dictionary<ObjectId, AssetReference>();

        // TODO: Check how to expose this publicly in a nice way
        public readonly Dictionary<object, AssetReference> loadedAssetsUrl = new Dictionary<object, AssetReference>();

        public AssetManager() : this(null)
        {
        }

        public AssetManager(IServiceRegistry services)
        {
            Serializer = new AssetSerializer();
            if (services != null)
            {
                services.AddService(typeof(IAssetManager), this);
                services.AddService(typeof(AssetManager), this);
                Serializer.SerializerContextTags.Set(ServiceRegistry.ServiceRegistryKey, services);
            }
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

        public bool Exists(string url)
        {
            ObjectId objectId;
            return FileProvider.AssetIndexMap.TryGetValue(url, out objectId);
        }

        public Stream OpenAsStream(string url, StreamFlags streamFlags)
        {
            return FileProvider.OpenStream(url, VirtualFileMode.Open, VirtualFileAccess.Read, streamFlags:streamFlags);
        }

        public T Load<T>(string url, AssetManagerLoaderSettings settings = null) where T : class
        {
            return (T)Load(typeof(T), url, settings);
        }

        public object Load(Type type, string url, AssetManagerLoaderSettings settings = null)
        {
            if (settings == null)
                settings = AssetManagerLoaderSettings.Default;

            if (url == null) throw new ArgumentNullException("url");

            lock (loadedAssetsByUrl)
            {
                using (var profile = Profiler.Begin(AssetProfilingKeys.AssetLoad, url))
                {
                    return DeserializeObject(url, type, settings);
                }
            }
        }

        public Task<T> LoadAsync<T>(string url, AssetManagerLoaderSettings settings = null) where T : class
        {
            return Task.Factory.StartNew(() => Load<T>(url, settings));
        }

        public Task<object> LoadAsync(Type type, string url, AssetManagerLoaderSettings settings = null)
        {
            return Task.Factory.StartNew(() => Load(type, url, settings));
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

        struct DeserializeOperation
        {
            public readonly AssetReference ParentAssetReference;
            public readonly string Url;
            public readonly Type ObjectType;
            public readonly Object Object;

            public DeserializeOperation(AssetReference parentAssetReference, string url, Type objectType, object obj)
            {
                ParentAssetReference = parentAssetReference;
                Url = url;
                ObjectType = objectType;
                Object = obj;
            }
        }

        private object DeserializeObject(string url, Type type, AssetManagerLoaderSettings settings)
        {
            AssetReference assetReference;

            var serializeOperations = new Queue<DeserializeOperation>();
            serializeOperations.Enqueue(new DeserializeOperation(null, url, type, null));

            bool isFirstOperation = true;
            object result = null;

            while (serializeOperations.Count > 0)
            {
                var serializeOperation = serializeOperations.Dequeue();
                var deserializedObject = DeserializeObject(serializeOperations, serializeOperation.ParentAssetReference, serializeOperation.Url, serializeOperation.ObjectType, serializeOperation.Object, settings);
                if (isFirstOperation)
                {
                    result = deserializedObject;
                    isFirstOperation = false;
                }
            }

            return result;
        }

        internal object FindDeserializedObject(string url, Type objType)
        {
            // Resolve URL
            ObjectId objectId;
            if (!FileProvider.AssetIndexMap.TryGetValue(url, out objectId))
            {
                HandleAssetNotFound(url);
                return null;
            }

            // Try to find already loaded object
            AssetReference assetReference;
            if (loadedAssetsByUrl.TryGetValue(objectId, out assetReference))
            {
                while (assetReference != null && !objType.GetTypeInfo().IsAssignableFrom(assetReference.Object.GetType().GetTypeInfo()))
                {
                    assetReference = assetReference.Next;
                }

                if (assetReference != null)
                {
                    // TODO: Currently ReferenceSerializer creates a ContentReference, so we will go through DeserializeObject later to add the reference
                    // This should be unified at some point

                    // Add reference
                    //bool isRoot = parentAssetReference == null;
                    //if (isRoot || parentAssetReference.References.Add(assetReference))
                    //{
                    //    IncrementReference(assetReference, isRoot);
                    //}

                    return assetReference.Object;
                }
            }

            return null;
        }

        internal void RegisterDeserializedObject<T>(string url, T obj)
        {
            // Resolve URL
            ObjectId objectId;
            if (!FileProvider.AssetIndexMap.TryGetValue(url, out objectId))
            {
                HandleAssetNotFound(url);
                return;
            }

            var assetReference = new AssetReference(objectId, url, false);
            SetAssetObject(assetReference, obj);
        }

        private object DeserializeObject(Queue<DeserializeOperation> serializeOperations, AssetReference parentAssetReference, string url, Type objType, object obj, AssetManagerLoaderSettings settings)
        {
            // Resolve URL
            ObjectId objectId;
            if (!FileProvider.AssetIndexMap.TryGetValue(url, out objectId))
            {
                HandleAssetNotFound(url);
                return null;
            }

            // Try to find already loaded object
            AssetReference assetReference;
            if (loadedAssetsByUrl.TryGetValue(objectId, out assetReference))
            {
                while (assetReference != null && !objType.GetTypeInfo().IsAssignableFrom(assetReference.Object.GetType().GetTypeInfo()))
                {
                    assetReference = assetReference.Next;
                }

                if (assetReference != null && assetReference.Deserialized)
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

                if (assetReference == null)
                {
                    // Create AssetReference
                    assetReference = new AssetReference(objectId, url, parentAssetReference == null);
                    contentSerializerContext.AssetReference = assetReference;
                    result = obj ?? serializer.Construct(contentSerializerContext);
                    SetAssetObject(assetReference, result);
                }
                else
                {
                    result = assetReference.Object;
                    contentSerializerContext.AssetReference = assetReference;
                }

                assetReference.Deserialized = true;

                PrepareSerializerContext(contentSerializerContext, streamReader.Context);

                contentSerializerContext.SerializeContent(streamReader, serializer, result);

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

                    //AssetReference childReference;

                    if (settings.ContentFilter != null)
                        settings.ContentFilter(contentReference, ref shouldBeLoaded);

                    if (shouldBeLoaded)
                    {
                        serializeOperations.Enqueue(new DeserializeOperation(assetReference, contentReference.Location, contentReference.Type, contentReference.ObjectValue));
                    }
                }
            }

            return result;
        }

        struct SerializeOperation
        {
            public readonly string Url;
            public readonly object Object;
            public readonly bool PublicReference;

            public SerializeOperation(string url, object obj, bool publicReference)
            {
                Url = url;
                Object = obj;
                PublicReference = publicReference;
            }
        }

        private void SerializeObject(string url, object obj, bool publicReference)
        {
            var serializeOperations = new Queue<SerializeOperation>();
            serializeOperations.Enqueue(new SerializeOperation(url, obj, publicReference));

            while (serializeOperations.Count > 0)
            {
                var serializeOperation = serializeOperations.Dequeue();
                SerializeObject(serializeOperations, serializeOperation.Url, serializeOperation.Object, serializeOperation.PublicReference);
            }
        }

        private void SerializeObject(Queue<SerializeOperation> serializeOperations, string url, object obj, bool publicReference)
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
                {
                    var attachedReference = AttachedReferenceManager.GetAttachedReference(contentReference.ObjectValue);
                    if (attachedReference == null || attachedReference.IsProxy)
                        continue;

                    serializeOperations.Enqueue(new SerializeOperation(contentReference.Location, contentReference.ObjectValue, false));
                }
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
                AttachedReferenceManager.SetUrl(obj, assetReference.Url);
            }
        }

        /// <summary>
        /// Notify debugger and logging when an asset could not be found.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <exception cref="SiliconStudio.Core.Serialization.Assets.AssetManagerException"></exception>
        private static void HandleAssetNotFound(string url)
        {
            // If a debugger is attached, throw an exception (we do that instead of Debugger.Break so that user can easily ignore this specific type of exception)
            if (Debugger.IsAttached)
            {
                try
                {
                    throw new AssetManagerException(string.Format("Asset [{0}] not found.", url));
                }
                catch (Exception)
                {
                }
            }

            // Log error
            Log.Error("Asset [{0}] could not be found.");
        }
    }
}
