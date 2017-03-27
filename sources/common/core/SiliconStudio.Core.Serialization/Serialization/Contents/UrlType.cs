// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
