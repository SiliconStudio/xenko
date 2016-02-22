// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks.Dataflow;

namespace SiliconStudio.Xenko.Engine.Events
{
    public class EventKey<T>
    {
        internal IDisposable Connect(ITargetBlock<T> target)
        {
            return BroadcastBlock.LinkTo(target);
        }

        /// <summary>
        /// Broadcasts the event data to all the receivers
        /// </summary>
        /// <param name="data"></param>
        public void Broadcast(T data)
        {
            BroadcastBlock.Post(data);
        }

        /// <summary>
        /// Exposes the Tasks.Dataflow object, useful in the case of custom usage
        /// </summary>
        public BroadcastBlock<T> BroadcastBlock { get; } = new BroadcastBlock<T>(null);
    }
}