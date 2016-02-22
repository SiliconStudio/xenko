// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks.Dataflow;

namespace SiliconStudio.Xenko.Engine.Events
{
    public class EventKey<T>
    {
        private readonly BroadcastBlock<T> broadcastBlock = new BroadcastBlock<T>(null);

        internal IDisposable Connect(ITargetBlock<T> target)
        {
            return broadcastBlock.LinkTo(target);
        }

        public void Broadcast(T data)
        {
            broadcastBlock.Post(data);
        }
    }
}