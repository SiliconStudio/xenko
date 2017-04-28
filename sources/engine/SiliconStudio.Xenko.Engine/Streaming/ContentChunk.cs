// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.IO;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Content storage data chunk.
    /// </summary>
    public class ContentChunk
    {
        /// <summary>
        /// Gets the parent storage container.
        /// </summary>
        public ContentStorage Storage { get; }

        /// <summary>
        /// Gets the chunk location in file (adress of the first byte).
        /// </summary>
        public int Location { get; }

        /// <summary>
        /// Gets the chunk size in file (in bytes).
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets the data. Can be null if hasn't been laoded yet.
        /// </summary>
        public byte[] Data { get; protected set; }

        /// <summary>
        /// Gets the last access time.
        /// </summary>
        public DateTime LastAccessTime { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this chunk is loaded.
        /// </summary>
        public bool IsLoaded => Data != null;

        /// <summary>
        /// Gets a value indicating whether this chunk is not loaded.
        /// </summary>
        public bool IsMissing => Data == null;

        /// <summary>
        /// Gets a value indicating whether this exists in file.
        /// </summary>
        public bool ExistsInFile => Size > 0;

        internal ContentChunk(ContentStorage storage, int location, int size)
        {
            Storage = storage;
            Location = location;
            Size = size;
        }
        
        /// <summary>
        /// Registers the usage operation of chunk data.
        /// </summary>
        public void RegisterUsage()
        {
            LastAccessTime = DateTime.UtcNow;
        }

        public void Load()
        {
            if (IsLoaded)
                return;

            var getFP = ContentManager.GetFileProvider;
            var FP = ContentManager.FileProvider;

            //using (var stream = Storage.FileProvider.OpenStream(Storage.Url, VirtualFileMode.Open, VirtualFileAccess.Read, VirtualFileShare.Read, StreamFlags.Seekable))
            using (var stream = ContentManager.FileProvider.OpenStream(Storage.Url, VirtualFileMode.Open, VirtualFileAccess.Read, VirtualFileShare.Read, StreamFlags.Seekable))
            {
                stream.Position = Location;
                var data = new byte[Size];
                stream.Read(data, 0, Size);
                Data = data;
            }

            RegisterUsage();
        }
    }
}
