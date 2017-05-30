// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.Serialization.Contents
{
    [DataContract]
    public enum UrlType
    {
        None,

        /// <summary>
        /// The location is a file on the disk and just the file name and the existence of the file will be taken into account
        /// </summary>
        File,

        /// <summary>
        /// The location is an asset url just used in its asset representation (no need to be compiled)
        /// </summary>
        ContentLink,

        /// <summary>
        /// The location is an asset url and the content is used by the command (the asset needs to be compiled)
        /// </summary>
        Content
    }
}
