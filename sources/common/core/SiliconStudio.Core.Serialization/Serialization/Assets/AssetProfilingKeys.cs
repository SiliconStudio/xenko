// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Core.Serialization.Assets
{
    /// <summary>
    /// Keys used for profiling the game class.
    /// </summary>
    public static class AssetProfilingKeys
    {
        public static readonly ProfilingKey Asset = new ProfilingKey("Asset");

        /// <summary>
        /// Profiling load of an asset.
        /// </summary>
        public static readonly ProfilingKey AssetLoad = new ProfilingKey(Asset, "Load", ProfilingKeyFlags.Log);

        /// <summary>
        /// Profiling load of an asset.
        /// </summary>
        public static readonly ProfilingKey AssetReload = new ProfilingKey(Asset, "Reload", ProfilingKeyFlags.Log);

        /// <summary>
        /// Profiling save of an asset.
        /// </summary>
        public static readonly ProfilingKey AssetSave = new ProfilingKey(Asset, "Save", ProfilingKeyFlags.Log);
    }
}