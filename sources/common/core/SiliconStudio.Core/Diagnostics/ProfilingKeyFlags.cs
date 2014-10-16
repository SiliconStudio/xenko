// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.Diagnostics
{
    [Flags]
    public enum ProfilingKeyFlags
    {
        /// <summary>
        /// Empty flag.
        /// </summary>
        None = 0,

        /// <summary>
        /// Output message to log right away.
        /// </summary>
        Log = 1,
    }
}