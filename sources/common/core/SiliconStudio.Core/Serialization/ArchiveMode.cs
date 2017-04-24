// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.Serialization
{
    /// <summary>
    /// Enumerates the different mode of serialization (either serialization or deserialization).
    /// </summary>
    public enum ArchiveMode
    {
        /// <summary>
        /// The serializer is in serialize mode.
        /// </summary>
        Serialize,

        /// <summary>
        /// The serializer is in deserialize mode.
        /// </summary>
        Deserialize,
    }
}
