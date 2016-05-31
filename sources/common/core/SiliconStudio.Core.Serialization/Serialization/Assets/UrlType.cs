// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Core.Serialization.Assets
{
    [DataContract]
    public enum UrlType
    {
        None,

        /// <summary>
        /// The location is a file on the disk
        /// </summary>
        File,

        /// <summary>
        /// The location is a IReference just used as a link
        /// </summary>
        ContentLink,

        /// <summary>
        /// The location is a IReference and the content is used by the command
        /// </summary>
        Content,

        /// <summary>
        /// TODO: Is it something still used?
        /// </summary>
        Virtual,
    }
}
