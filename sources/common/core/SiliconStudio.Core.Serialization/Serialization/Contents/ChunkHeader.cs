// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.IO;

namespace SiliconStudio.Core.Serialization.Contents
{
    /// <summary>
    /// This class describes the header of an asset serialized in a blob file. Its (serialized) size has to remain constant
    /// </summary>
    public sealed class ChunkHeader
    {
        public const int CurrentVersion = 1;

        public const int Magic = 0x43484E4B; // "CHNK"
        public int Version { get; set; }
        public string Type { get; set; }
        public int OffsetToObject { get; set; }
        public int OffsetToReferences { get; set; }

        public ChunkHeader()
        {
            Version = CurrentVersion;
            OffsetToObject = -1;
            OffsetToReferences = -1;
        }

        private ChunkHeader(int version)
        {
            Version = version;
            OffsetToObject = -1;
            OffsetToReferences = -1;
        }

        public void Write(SerializationStream stream)
        {
            stream.Write(Magic);
            stream.Write(Version);
            if (Version == 1)
            {
                stream.Write(Type);
                stream.Write(OffsetToObject);
                stream.Write(OffsetToReferences);
            }
        }

        public static ChunkHeader Read(SerializationStream stream)
        {
            int magic = stream.ReadInt32();
            if (magic != Magic)
            {
                // Rewind
                stream.NativeStream.Seek(-4, SeekOrigin.Current);
                return null;
            }

            int version = stream.ReadInt32();
            var header = new ChunkHeader(version);
            if (version == 1)
            {
                header.Type = stream.ReadString();
                header.OffsetToObject = stream.ReadInt32();
                header.OffsetToReferences = stream.ReadInt32();
            }
            return header;
        }
    }
}
