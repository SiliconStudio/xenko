// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
