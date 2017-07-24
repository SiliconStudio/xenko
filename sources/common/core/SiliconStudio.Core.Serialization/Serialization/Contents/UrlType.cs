// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.Serialization.Contents
{
    /// <summary>
    /// An enum representing the type of an url.
    /// </summary>
    [DataContract]
    public enum UrlType
    {
        /// <summary>
        /// The location is not valid.
        /// </summary>
        None,

        /// <summary>
        /// The location is a file on the disk.
        /// </summary>
        File,

        /// <summary>
        /// The location is a content url.
        /// </summary>
        Content
    }
}
