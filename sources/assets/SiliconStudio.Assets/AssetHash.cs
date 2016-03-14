// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Helper to compute a stable hash from an asset including all meta informations (ids, overrides).
    /// </summary>
    public static class AssetHash
    {
        /// <summary>
        /// Computes a stable hash from an asset including all meta informations (ids, overrides).
        /// </summary>
        /// <param name="asset">An object instance</param>
        /// <param name="flags">Flags used to control the serialization process</param>
        /// <returns>a stable hash</returns>
        public static ObjectId Compute(object asset, AssetClonerFlags flags = AssetClonerFlags.None)
        {
            return AssetCloner.ComputeHash(asset, flags);
        }
    }
}