// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.MicroThreading;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// A script whose <see cref="Update"/> will be called every frame.
    /// </summary>
    public abstract class SyncScript : StartupScript
    {
        internal PriorityQueueNode<SchedulerEntry> UpdateSchedulerNode;

        /// <summary>
        /// Called every frame.
        /// </summary>
        public abstract void Update();
    }
}
