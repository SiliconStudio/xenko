// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Runtime.Serialization;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Core.Streaming
{
    /// <summary>
    /// Header with description of streamable resource data storage.
    /// </summary>
    public struct ContentStorageHeader
    {
        public struct ChunkEntry
        {
            public int Location;
            public int Size;
        }

        public string DataUrl;
        public DateTime PackageTime;
        public int HashCode;
        public ChunkEntry[] Chunks;

        public int ChunksCount => Chunks.Length;

        /// <summary>
        /// Writes this instance to a stream.
        /// </summary>
        /// <param name="stream">The destination stream.</param>
        public void Write(SerializationStream stream)
        {
            stream.Write(1);
            stream.Write(DataUrl);
            stream.Write(PackageTime.Ticks);
            stream.Write(ChunksCount);
            for (int i = 0; i < Chunks.Length; i++)
            {
                var e = Chunks[i];
                stream.Write(e.Location);
                stream.Write(e.Size);
            }
            stream.Write(HashCode);
        }

        /// <summary>
        /// Reads header instance from a stream.
        /// </summary>
        /// <param name="stream">The source stream.</param>
        /// <param name="result">Result data</param>
        public static void Read(SerializationStream stream, out ContentStorageHeader result)
        {
            result = new ContentStorageHeader();
            var version = stream.ReadInt32();
            if (version == 1)
            {
                result.DataUrl = stream.ReadString();
                result.PackageTime = new DateTime(stream.ReadInt64());
                int chunksCount = stream.ReadInt32();
                result.Chunks = new ChunkEntry[chunksCount];
                for (int i = 0; i < chunksCount; i++)
                {
                    result.Chunks[i].Location = stream.ReadInt32();
                    result.Chunks[i].Size = stream.ReadInt32();
                }
                result.HashCode = stream.ReadInt32();

                return;
            }

            throw new SerializationException($"Invald {nameof(ContentStorageHeader)} version.");
        }
    }
}
