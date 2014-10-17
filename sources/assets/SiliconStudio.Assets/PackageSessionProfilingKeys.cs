// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Keys used for profiling the game class.
    /// </summary>
    public static class PackageSessionProfilingKeys
    {
        public static readonly ProfilingKey Session = new ProfilingKey("PackageSession");

        /// <summary>
        /// Profiling load of a session.
        /// </summary>
        public static readonly ProfilingKey Loading = new ProfilingKey(Session, "Load", ProfilingKeyFlags.Log);

        /// <summary>
        /// Profiling save of a session.
        /// </summary>
        public static readonly ProfilingKey Saving = new ProfilingKey(Session, "Save", ProfilingKeyFlags.Log);
    }
}