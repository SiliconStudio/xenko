// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Assets
{
    /// <summary>
    /// Interface to create default instance of an asset type.
    /// </summary>
    public interface IAssetFactory
    {
        /// <summary>
        /// Creates a new default instance of an asset.
        /// </summary>
        /// <returns>A new default instance of an asset.</returns>
        Asset New();
    }
}