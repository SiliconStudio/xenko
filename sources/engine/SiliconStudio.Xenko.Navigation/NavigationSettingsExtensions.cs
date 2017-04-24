// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Xenko.Navigation
{
    public static class NavigationSettingsExtensions
    {
        /// <summary>
        /// Computes the hash of the <see cref="NavigationSettings.Groups"/> field
        /// </summary>
        public static ObjectId ComputeGroupsHash(this NavigationSettings settings)
        {
            using (DigestStream stream = new DigestStream(Stream.Null))
            {
                BinarySerializationWriter writer = new BinarySerializationWriter(stream);
                writer.Write(settings.Groups);
                return stream.CurrentHash;
            }
        }
    }
}
