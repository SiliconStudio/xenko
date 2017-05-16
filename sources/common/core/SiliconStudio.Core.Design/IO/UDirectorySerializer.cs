// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Core.IO
{
    [DataSerializerGlobal(typeof(UDirectorySerializer))]
    internal class UDirectorySerializer : DataSerializer<UDirectory>
    {
        /// <inheritdoc/>
        public override void Serialize(ref UDirectory obj, ArchiveMode mode, SerializationStream stream)
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
                obj = new UDirectory(path);
            }
        }
    }
}
