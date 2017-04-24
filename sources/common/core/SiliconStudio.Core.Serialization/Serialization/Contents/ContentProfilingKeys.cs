// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Core.Serialization.Contents
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
