// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Games
{
    /// <summary>
    /// Profiling keys for <see cref="GraphicsDeviceManager"/>.
    /// </summary>
    public static class GraphicsDeviceManagerProfilingKeys
    {
        public static readonly ProfilingKey GraphicsDeviceManager = new ProfilingKey("GraphicsDeviceManager");

        /// <summary>
        /// Profiling graphics device initialization.
        /// </summary>
        public static readonly ProfilingKey CreateDevice = new ProfilingKey(GraphicsDeviceManager, "CreateGraphicsDevice");
    }
}