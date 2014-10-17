// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.BuildEngine
{
    /// <summary>
    /// Interface for object metadata
    /// </summary>
    public interface IObjectMetadata
    {
        /// <summary>
        /// The url of the object which uses this metadata
        /// </summary>
        string ObjectUrl { get; }

        /// <summary>
        /// The key of the metadata
        /// </summary>
        MetadataKey Key { get; }

        /// <summary>
        /// The value of the metadata. Its type must match the <see cref="Key"/> type.
        /// </summary>
        object Value { get; set; }
    }
}