// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Engine.Events
{
    /// <summary>
    /// Simple passthru scheduler to avoid the default dataflow TaskScheduler.Default usage
    /// This also makes sure we fire events at proper required order/timing
    /// </summary>
    internal class EventTaskScheduler : TaskScheduler
    {
        public static readonly EventTaskScheduler Scheduler = new EventTaskScheduler();

        protected override void QueueTask(Task task)
        {
            TryExecuteTask(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return null;
        }
    }

    /// <summary>
    /// Used mostly for debug, to identify events
    /// </summary>
    internal static class EventKeyCounter
    {
        private static long eventKeysCounter;

        public static ulong New()
        {
            return (ulong)Interlocked.Increment(ref eventKeysCounter);
        }
    }

    /// <summary>
    /// Creates a new EventKey used to broadcast T type events.
    /// </summary>
    /// <typeparam name="T">The data type of the event you wish to send</typeparam>
    public class EventKeyBase<T> : IDisposable
    {
        internal readonly Logger Logger;
        internal readonly ulong EventId = EventKeyCounter.New();
        internal readonly string EventName;

        private readonly string broadcastDebug;

        private readonly BroadcastBlock<T> broadcastBlock;

        internal EventKeyBase(string category = "General", string eventName = "Event")
        {
            broadcastBlock = new BroadcastBlock<T>(null, new DataflowBlockOptions { TaskScheduler = EventTaskScheduler.Scheduler });

            EventName = eventName;
            Logger = GlobalLogger.GetLogger($"Event - {category}");
            broadcastDebug = $"Broadcasting '{eventName}' ({EventId})";
        }

        ~EventKeyBase()
        {
            Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        internal IDisposable Connect(EventReceiverBase<T> target)
        {
            return broadcastBlock.LinkTo(target.BufferBlock);
        }

        /// <summary>
        /// Broadcasts the event data to all the receivers
        /// </summary>
        /// <param name="data"></param>
        protected void InternalBroadcast(T data)
        {
            Logger.Debug(broadcastDebug);
            broadcastBlock.Post(data);
        }
    }

    public sealed class EventKey<T> : EventKeyBase<T>
    {
        public EventKey(string category = "General", string eventName = "Event") : base(category, eventName)
        {
        }

        public void Broadcast(T data)
        {
            InternalBroadcast(data);
        }
    }

    /// <summary>
    /// Creates a new EventKey used to broadcast events.
    /// </summary>
    public sealed class EventKey : EventKeyBase<bool>
    {
        public EventKey(string category = "General", string eventName = "Event") : base(category, eventName)
        {       
        }

        /// <summary>
        /// Broadcasts the event to all the receivers
        /// </summary>
        public void Broadcast()
        {
            InternalBroadcast(true);
        }
    }
}
