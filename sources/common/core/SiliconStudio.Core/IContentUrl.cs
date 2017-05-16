// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core
{
    /// <summary>
    /// Interface for serializable object having an url (so referenceable by other assets and saved into a single blob file)
    /// </summary>
    public interface IContentUrl
    {
        /// <summary>
        /// The URL of this asset.
        /// </summary>
        string Url { get; set; }
    }
}
