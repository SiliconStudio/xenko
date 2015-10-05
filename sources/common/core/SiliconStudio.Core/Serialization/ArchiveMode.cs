// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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