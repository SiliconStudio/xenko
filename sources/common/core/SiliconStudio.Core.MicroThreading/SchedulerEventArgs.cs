// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.Core.MicroThreading
{
    /// <summary>
    /// Provides data for the <see cref="Scheduler.MicroThreadStarted"/>, <see cref="Scheduler.MicroThreadEnded"/>, <see cref="Scheduler.MicroThreadCallbackStart"/> and <see cref="Scheduler.MicroThreadCallbackEnd"/> events.
    /// </summary>
    public class SchedulerThreadEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the <see cref="MicroThread"/> this event concerns.
        /// </summary>
        /// <value>
        /// The micro thread.
        /// </value>
        public MicroThread MicroThread { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Threading.Thread.ManagedThreadId"/> active when this event happened.
        /// </summary>
        /// <value>
        /// The managed thread identifier.
        /// </value>
        public int ThreadId { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulerThreadEventArgs"/> class.
        /// </summary>
        /// <param name="microThread">The micro thread.</param>
        /// <param name="threadId">The managed thread identifier.</param>
        public SchedulerThreadEventArgs(MicroThread microThread, int threadId)
        {
            MicroThread = microThread;
            ThreadId = threadId;
        }
    }
}
