// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Core.IO
{
    /// <summary>
    /// Data serializer for Guid.
    /// </summary>
    [DataSerializerGlobal(typeof(UFileSerializer))]
    internal class UFileSerializer : DataSerializer<UFile>
    {
        /// <inheritdoc/>
        public override void Serialize(ref UFile obj, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                var path = obj?.FullPath;
                stream.Serialize(ref path);
            }
            else if (mode == ArchiveMode.Deserialize)
            {
                string path = null;
                stream.Serialize(ref path);
                obj = new UFile(path);
            }
        }
    }
}
