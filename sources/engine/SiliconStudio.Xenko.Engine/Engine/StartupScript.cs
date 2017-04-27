// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Collections;
using SiliconStudio.Core.MicroThreading;

namespace SiliconStudio.Xenko.Engine
{
    public abstract class StartupScript : ScriptComponent
    {
        internal PriorityQueueNode<SchedulerEntry> StartSchedulerNode;

        /// <summary>
        /// Called before the script enters it's update loop.
        /// </summary>
        public virtual void Start()
        {
        }
    }
}
