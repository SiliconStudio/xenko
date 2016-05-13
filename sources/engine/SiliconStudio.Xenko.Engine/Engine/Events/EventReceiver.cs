// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SiliconStudio.Xenko.Engine.Processors;

namespace SiliconStudio.Xenko.Engine.Events
{
    public struct EventReceiverAwaiter<T> : INotifyCompletion
    {
        private TaskAwaiter<T> task;

        public EventReceiverAwaiter(TaskAwaiter<T> task)
        {
            this.task = task;
        }

        public void OnCompleted(Action continuation)
        {
            task.OnCompleted(continuation);
        }

        public bool IsCompleted => task.IsCompleted;

        public T GetResult()
        {
            return task.GetResult();
        }
    }

    public abstract class EventReceiverBase
    {
        internal abstract void CancelReceive();

        internal abstract Task GetTask();
    }

    /// <summary>
    /// Creates an event receiver that is used to receive T type events from an EventKey
    /// </summary>
    /// <typeparam name="T">The type of data the EventKey will send</typeparam>
    public class EventReceiver<T> : EventReceiverBase, IDisposable
    {
        private IDisposable link;
        private readonly CancellationTokenSource cancellationTokenSource;
        private CancellationTokenSource receiveCancellationTokenSource = new CancellationTokenSource();
        private string receivedDebugString;
        private string receivedManyDebugString;

        internal BufferBlock<T> BufferBlock;

        public EventKey<T> Key { get; private set; }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly DataflowBlockOptions CapacityOptions = new DataflowBlockOptions
        {
            BoundedCapacity = 1
        };

        private void Init(EventKey<T> key, EventReceiverOptions options)
        {
            Key = key;

            BufferBlock = ((options & EventReceiverOptions.Buffered) != 0) ? new BufferBlock<T>() : new BufferBlock<T>(CapacityOptions);

            link = key.Connect(this);

            receivedDebugString = $"Received '{key.EventName}' ({key.EventId})";
            receivedManyDebugString = $"Received All '{key.EventName}' ({key.EventId})";

            T foo;
            TryReceiveOne(out foo); //clear any previous event, we don't want to receive old events, as broadcast block will always send us the last avail event on connect
        }

        /// <summary>
        /// Creates an event receiver, ready to receive broadcasts from the key
        /// </summary>
        /// <param name="key">The event key to listen from</param>
        /// <param name="options">Option flags</param>
        public EventReceiver(EventKey<T> key, EventReceiverOptions options = EventReceiverOptions.None)
        {
            if (((options & EventReceiverOptions.ClearEveryFrame) != 0))
            {
                throw new InvalidOperationException("If the options ClearEveryFrame is present a valid script scheduler must be passed to the EventReceiver constructor");
            }

            Init(key, options);
        }

        /// <summary>
        /// Creates an event receiver, ready to receive broadcasts from the key
        /// </summary>
        /// <param name="key">The event key to listen from</param>
        /// <param name="scheduler">The scheduler where the event is awaited</param>
        /// <param name="options">Option flags</param>
        public EventReceiver(EventKey<T> key, ScriptSystem scheduler, EventReceiverOptions options = EventReceiverOptions.None)
        {
            Init(key, options);

            if (((options & EventReceiverOptions.ClearEveryFrame) != 0) && scheduler == null)
            {
                throw new InvalidOperationException("If the options ClearEveryFrame is present a valid script scheduler must be passed to the EventReceiver constructor");
            }

            var clearEveryFrame = ((options & EventReceiverOptions.ClearEveryFrame) != 0) && scheduler != null;
            if (!clearEveryFrame) return;

            cancellationTokenSource = new CancellationTokenSource();
            scheduler.AddTask(async () =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    //consume all events at the end of every next frame

                    var toRemove = BufferBlock.Count;

                    await scheduler.NextFrame();

                    for (var i = 0; i < toRemove; i++)
                    {
                        BufferBlock.Receive();
                    }
                }
            }, 0xfffffff);
        }

        /// <summary>
        /// Awaits a single event
        /// </summary>
        /// <returns></returns>
        public async Task<T> ReceiveAsync()
        {
            var res = await BufferBlock.ReceiveAsync();

            Key.Logger.Debug(receivedDebugString);

            return res;
        }

        private async Task<T> ReceiveAsyncWithToken()
        {
            T res;

            if (receiveCancellationTokenSource.IsCancellationRequested)
            {
                //we were canceled previously so we actually need to recreate the cancelation source
                receiveCancellationTokenSource.Dispose();
                receiveCancellationTokenSource = new CancellationTokenSource();
            }

            try
            {
                res = await BufferBlock.ReceiveAsync(receiveCancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                return default(T);
            }

            Key.Logger.Debug(receivedDebugString);

            return res;
        }

        public EventReceiverAwaiter<T> GetAwaiter()
        {
            return new EventReceiverAwaiter<T>(ReceiveAsync().GetAwaiter());
        }

        /// <summary>
        /// Returns the count of currently buffered events
        /// </summary>
        public int Count => BufferBlock.Count;

        /// <summary>
        /// Receives one event from the buffer, useful specially in Sync scripts
        /// </summary>
        /// <returns></returns>
        public bool TryReceiveOne(out T data)
        {
            if (BufferBlock.Count == 0)
            {
                data = default(T);
                return false;
            }

            data = BufferBlock.Receive();
            Key.Logger.Debug(receivedDebugString);
            return true;
        }

        /// <summary>
        /// Receives all the events from the queue (if buffered was true during creations), useful mostly only in Sync scripts
        /// </summary>
        /// <returns></returns>
        public int TryReceiveMany(ICollection<T> collection)
        {
            IList<T> result;
            if (!BufferBlock.TryReceiveAll(out result))
            {
                return 0;
            }

            Key.Logger.Debug(receivedManyDebugString);

            var count = 0;
            foreach (var e in result)
            {
                count++;
                collection.Add(e);
            }

            return count;
        }

        ~EventReceiver()
        {
            Dispose();
        }

        public void Dispose()
        {
            link.Dispose();
            cancellationTokenSource?.Dispose();

            GC.SuppressFinalize(this);
        }

        internal override Task GetTask()
        {
            return ReceiveAsyncWithToken();
        }

        internal override void CancelReceive()
        {
            receiveCancellationTokenSource.Cancel(true);
        }
    }

    /// <summary>
    /// Creates an event receiver that is used to receive events from an EventKey
    /// </summary>
    public class EventReceiver : EventReceiver<bool>
    {
        /// <summary>
        /// Creates an event receiver, ready to receive broadcasts from the key
        /// </summary>
        /// <param name="key">The event key to listen from</param>
        /// <param name="options">Option flags</param>
        public EventReceiver(EventKey key, EventReceiverOptions options = EventReceiverOptions.None) : base(key, options)
        {
            
        }

        /// <summary>
        /// Creates an event receiver, ready to receive broadcasts from the key
        /// </summary>
        /// <param name="key">The event key to listen from</param>
        /// <param name="scheduler">The scheduler where the event is awaited</param>
        /// <param name="options">Option flags</param>
        public EventReceiver(EventKey key, ScriptSystem scheduler, EventReceiverOptions options = EventReceiverOptions.None) : base(key, scheduler, options)
        {
        }

        /// <summary>
        /// Awaits a single event
        /// </summary>
        /// <returns></returns>
        public new async Task ReceiveAsync()
        {
            await base.ReceiveAsync();
        }

        public bool TryReceiveOne()
        {
            bool foo;
            return TryReceiveOne(out foo);
        }

        public static async Task<EventReceiverBase> ReceiveFirst(params EventReceiverBase[] events)
        {
            var tasks = new Task[events.Length];
            for (var i = 0; i < events.Length; i++)
            {
                var @event = events[i];
                tasks[i] = @event.GetTask();
            }

            await Task.WhenAny(tasks);

            EventReceiverBase completed = null;

            for (var i = 0; i < events.Length; i++)
            {
                if (tasks[i].IsCompleted)
                {
                    completed = events[i];
                }
                else
                {
                    events[i].CancelReceive();
                }
            }

            return completed;
        }
    }
}
