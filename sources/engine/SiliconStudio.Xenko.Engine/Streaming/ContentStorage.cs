// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Streamable resources content storage containter.
    /// </summary>
    public class ContentStorage
    {
        private readonly ContentChunk[] _chunks;

        /// <summary>
        /// Gets the time when container has been created.
        /// </summary>
        public DateTime PackageTime { get; }

        /// <summary>
        /// Gets the last access time.
        /// </summary>
        public DateTime LastAccessTime
        {
            get
            {
                var result = _chunks[0].LastAccessTime;
                for (int i = 1; i < _chunks.Length; i++)
                {
                    if (result < _chunks[i].LastAccessTime)
                        result = _chunks[i].LastAccessTime;
                }
                return result;
            }
        }

        /// <summary>
        /// Gets the amount of chunks located inside the storage container.
        /// </summary>
        public int ChunksCount => _chunks.Length;

        internal ContentStorage([NotNull] ContentChunk[] chunks, DateTime packageTime)
        {
            _chunks = chunks;
            PackageTime = packageTime;
        }

        /// <summary>
        /// Gets the chunk.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Chunk</returns>
        public ContentChunk GetChunk(int index)
        {
            Debug.Assert(index >= 0 && _chunks.Length > index);

            var chunk = _chunks[index];
            chunk.RegisterUsage();
            return chunk;
        }

        /// <summary>
        /// Creates the new package at the specified location.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="chunksData">The chunks data.</param>
        public static void Create(string path, List<byte[]> chunksData)
        {
            if (chunksData == null || chunksData.Count == 0 || chunksData.Any(x => x == null || x.Length == 0))
                throw new ArgumentException(nameof(chunksData));

            var packageTime = DateTime.UtcNow;

            // TODO: sort chunks (smaller ones should go first)

            // Calculate first chunk location (in file)
            int chunksCount = chunksData.Count;
            int offset =
                // Version
                sizeof(int)
                // Package Time (ticks)
                + sizeof(long)
                // Chunks count
                + sizeof(int)
                // Header hash code
                + sizeof(int)
                // Chunk locations and sizes
                + (sizeof(int) + sizeof(int)) * chunksCount;

            // Calculate header hash code (used to provide simple data verification during loading)
            int hashCode = (int)packageTime.Ticks;
            hashCode = (hashCode * 397) ^ chunksCount;
            for (int i = 0; i < chunksCount; i++)
                hashCode = (hashCode * 397) ^ chunksData[i].Length;

            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var stream = new BinaryWriter(fileStream))
            {
                // Header
                stream.Write(1);
                stream.Write(packageTime.Ticks);
                stream.Write(chunksCount);
                stream.Write(hashCode);
                for (int i = 0; i < chunksCount; i++)
                {
                    int size = chunksData[i].Length;
                    stream.Write(offset);
                    stream.Write(size);
                    offset += size;
                }

                // Chunks Data
                for (int i = 0; i < chunksCount; i++)
                    stream.Write(chunksData[i]);

                // Validate calculated offset
                if (offset != fileStream.Position)
                    throw new DataException("Invalid storage offset.");
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int)PackageTime.Ticks;
                hashCode = (hashCode * 397) ^ _chunks.Length;
                for (int i = 0; i < _chunks.Length; i++)
                    hashCode = (hashCode * 397) ^ _chunks[i].Size;
                return hashCode;
            }
        }
    }
}
