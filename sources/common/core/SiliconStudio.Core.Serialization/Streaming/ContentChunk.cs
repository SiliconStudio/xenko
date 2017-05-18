// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Core.Streaming
{
    /// <summary>
    /// Content storage data chunk.
    /// </summary>
    public class ContentChunk
    {
        private byte[] data;

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
        /// Gets the last access time.
        /// </summary>
        public DateTime LastAccessTime { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this chunk is loaded.
        /// </summary>
        public bool IsLoaded => data != null;

        /// <summary>
        /// Gets a value indicating whether this chunk is not loaded.
        /// </summary>
        public bool IsMissing => data == null;

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

        /// <summary>
        /// Loads chunk data from the storage container.
        /// </summary>
        /// <param name="fileProvider">Database file provider.</param>
        public byte[] GetData(DatabaseFileProvider fileProvider)
        {
            if (IsLoaded)
                return data;

            if (fileProvider == null)
                throw new ContentStreamingException("Missing file provider.", Storage);

            using (var stream = fileProvider.OpenStream(Storage.Url, VirtualFileMode.Open, VirtualFileAccess.Read, VirtualFileShare.Read, StreamFlags.Seekable))
            {
                stream.Position = Location;
                var bytes = new byte[Size];
                stream.Read(bytes, 0, Size);
                data = bytes;
            }

            RegisterUsage();

            return data;
        }

        internal void Unload()
        {
            data = null;
        }
    }
}
