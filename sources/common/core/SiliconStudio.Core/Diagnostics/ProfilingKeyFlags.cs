// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
