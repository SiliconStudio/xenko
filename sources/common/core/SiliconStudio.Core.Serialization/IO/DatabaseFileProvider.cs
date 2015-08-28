// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using SiliconStudio.Core.Storage;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.Core.IO
{
    public sealed class DatabaseFileProvider : VirtualFileProviderBase
    {
        private readonly IAssetIndexMap assetIndexMap;
        private readonly ObjectDatabase objectDatabase;

        public DatabaseFileProvider(ObjectDatabase objectDatabase, string mountPoint = null) : this(objectDatabase.AssetIndexMap, objectDatabase, mountPoint)
        {
        }

        public DatabaseFileProvider(IAssetIndexMap assetIndexMap, ObjectDatabase objectDatabase, string mountPoint = null) : base(mountPoint)
        {
            this.assetIndexMap = assetIndexMap;
            this.objectDatabase = objectDatabase;
        }

        public IAssetIndexMap AssetIndexMap
        {
            get { return assetIndexMap; }
        }

        public ObjectDatabase ObjectDatabase
        {
            get { return objectDatabase; }
        }

        /// <inheritdoc/>
        public override Stream OpenStream(string url, VirtualFileMode mode, VirtualFileAccess access, VirtualFileShare share = VirtualFileShare.Read, StreamFlags streamFlags = StreamFlags.None)
        {
            // Open or create the file through the underlying (IAssetIndexMap, IOdbBackend) couple.
            // Also read/write a ObjectHeader.
            if (mode == VirtualFileMode.Open)
            {
                ObjectId objectId;
                if (url.StartsWith("obj/"))
                    ObjectId.TryParse(url.Substring(4), out objectId);
                else if (!assetIndexMap.TryGetValue(url, out objectId))
                    throw new FileNotFoundException(string.Format("Unable to find the file [{0}]", url));

                var result = objectDatabase.OpenStream(objectId, mode, access, share);

                // copy the stream into a memory stream in order to make it seek-able
                if (streamFlags == StreamFlags.Seekable && !result.CanSeek)
                {
                    var buffer = new byte[result.Length - result.Position];
                    result.Read(buffer, 0, buffer.Length);
                    return new DatabaseReadFileStream(objectId, new MemoryStream(buffer), 0);
                }

                return new DatabaseReadFileStream(objectId, result, result.Position);
            }

            if (mode == VirtualFileMode.Create)
            {
                if (url.StartsWith("obj/"))
                    throw new NotSupportedException();

                var stream = objectDatabase.CreateStream();

                // Header will be written by DatabaseWriteFileStream
                var result = new DatabaseWriteFileStream(stream, stream.Position);

                stream.Disposed += x =>
                    {
                        // Commit index changes
                        assetIndexMap[url] = x.CurrentHash;
                    };

                return result;
            }

            throw new ArgumentException("mode");
        }

        public override string[] ListFiles(string url, string searchPattern, VirtualSearchOption searchOption)
        {
            url = Regex.Escape(url);
            searchPattern = Regex.Escape(searchPattern).Replace(@"\*", "[^/]*").Replace(@"\?", "[^/]");
            string recursivePattern = searchOption == VirtualSearchOption.AllDirectories ? "(.*/)*" : "/?";
            var regex = new Regex("^" + url + recursivePattern + searchPattern + "$");

            return assetIndexMap.SearchValues(x => regex.IsMatch(x.Key)).Select(x => x.Key).ToArray();
        }

        public override bool FileExists(string url)
        {
            ObjectId objectId;
            return assetIndexMap.TryGetValue(url, out objectId)
                && objectDatabase.Exists(objectId);
        }

        public override long FileSize(string url)
        {
            ObjectId objectId;
            if (!assetIndexMap.TryGetValue(url, out objectId))
                throw new FileNotFoundException();

            return objectDatabase.GetSize(objectId);
        }

        public override string GetAbsolutePath(string url)
        {
            ObjectId objectId;
            if (!assetIndexMap.TryGetValue(url, out objectId))
                throw new FileNotFoundException();

            return objectDatabase.GetFilePath(objectId);
        }


        /// <summary>
        /// Resolves the given VFS URL into a ObjectId and its DatabaseFileProvider.
        /// </summary>
        /// <param name="url">The URL to resolve.</param>
        /// <param name="objectId">The object id.</param>
        /// <returns>The <see cref="DatabaseFileProvider"/> containing this object if it could be found; [null] otherwise.</returns>
        public static DatabaseFileProvider ResolveObjectId(string url, out ObjectId objectId)
        {
            var resolveProviderResult = VirtualFileSystem.ResolveProvider(url, false);
            var provider = resolveProviderResult.Provider as DatabaseFileProvider;
            if (provider == null)
            {
                objectId = ObjectId.Empty;
                return null;
            }
            return provider.AssetIndexMap.TryGetValue(resolveProviderResult.Path, out objectId) ? provider : null;
        }

        abstract class DatabaseFileStream : VirtualFileStream, IDatabaseStream
        {
            protected DatabaseFileStream(Stream internalStream, long startPosition, bool seekToBeginning = true)
                : base(internalStream, startPosition, seekToBeginning: seekToBeginning)
            {
            }

            public abstract ObjectId ObjectId { get; }
        }

        class DatabaseReadFileStream : DatabaseFileStream
        {
            private ObjectId id;
            public DatabaseReadFileStream(ObjectId id, Stream internalStream, long startPosition)
                : base(internalStream, startPosition, false)
            {
                this.id = id;
            }

            public override ObjectId ObjectId
            {
                get
                {
                    return id;
                }
            }
        }

        class DatabaseWriteFileStream : DatabaseFileStream
        {
            public DatabaseWriteFileStream(Stream internalStream, long startPosition)
                : base(internalStream, startPosition, false)
            {
            }

            public override ObjectId ObjectId
            {
                get
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}