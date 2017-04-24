// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Core.MicroThreading
{
    [Flags]
    public enum ScriptFlags
    {
        /// <summary>
        /// Empty value.
        /// </summary>
        None = 0,

        /// <summary>
        /// Automatically run on assembly startup.
        /// </summary>
        AssemblyStartup = 1,

        /// <summary>
        /// Automatically run on assembly first startup (not executed if assembly is reloaded).
        /// </summary>
        AssemblyFirstStartup = 2,

        /// <summary>
        /// Automatically run on assembly unload.
        /// </summary>
        AssemblyUnload = 4,

        // TODO: Not implemented yet
        /// <summary>
        /// MicroThread won't be killed if assembly is unloaded (including reload).
        /// </summary>
        KeepAliveWhenUnload = 8,
    }
}
