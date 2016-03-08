// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Core.Serialization.Assets
{
    /// <summary>
    /// Keys used for profiling the game class.
    /// </summary>
    public static class ContentProfilingKeys
    {
        public static readonly ProfilingKey Content = new ProfilingKey("Content");

        /// <summary>
        /// Profiling load of an asset.
        /// </summary>
        public static readonly ProfilingKey ContentLoad = new ProfilingKey(Content, "Load", ProfilingKeyFlags.Log);

        /// <summary>
        /// Profiling load of an asset.
        /// </summary>
        public static readonly ProfilingKey ContentReload = new ProfilingKey(Content, "Reload", ProfilingKeyFlags.Log);

        /// <summary>
        /// Profiling save of an asset.
        /// </summary>
        public static readonly ProfilingKey ContentSave = new ProfilingKey(Content, "Save", ProfilingKeyFlags.Log);
    }
}