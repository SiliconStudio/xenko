// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
