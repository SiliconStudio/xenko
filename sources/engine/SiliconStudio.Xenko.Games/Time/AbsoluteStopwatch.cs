// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SiliconStudio.Xenko.Games.Time
{
    /// <summary>
    /// Represent an absolute time measurement stopwatch. (with as few internal overhead as possible)
    /// It measures elapsed time in seconds between calls to Start method and Elapsed property.
    /// </summary>
    public class AbsoluteStopwatch
    {
        private long startTicks;

        /// <summary>
        /// Start the stopwatch. (use this method also to restart stopwatching)
        /// </summary>
        public void Start()
        {
            startTicks = Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// Gets the time elapsed since previous call to Start method, in seconds.
        /// </summary>
        public double Elapsed
        {
            get
            {
                long elapsed = Stopwatch.GetTimestamp() - startTicks;
                if (elapsed < 0)
                    return 0.0;
                return (double)elapsed / (Stopwatch.Frequency);
            }
        }
    }
}
