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

    /// <summary>
    /// When using EventReceiver.ReceiveOne, this structure is used to contain the received data
    /// </summary>
    public struct EventData
    {
        public EventReceiverBase Receiver { get; internal set; }

        public object Data { get; internal set; }
    }

    /// <summary>
    /// Base class for EventReceivers
    /// </summary>
    public abstract class EventReceiverBase
    {
        internal abstract Task<bool> GetPeakTask();

        internal abstract Task GetTask();

        internal abstract bool TryReceiveOneInternal(out object obj);
    }

    /// <summary>
    /// Creates an event receiver that is used to receive T type events from an EventKey
    /// </summary>
    /// <typeparam name="T">The type of data the EventKey will send</typeparam>
    public class EventReceiver<T> : EventReceiverBase, IDisposable
    {
        private IDisposable link;
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
            Init(key, options);
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

        public void Reset()
        {
            //console all in one go
            IList<T> result;
            BufferBlock.TryReceiveAll(out result);
        }

        ~EventReceiver()
        {
            Dispose();
        }

        public void Dispose()
        {
            link.Dispose();

            GC.SuppressFinalize(this);
        }

        internal override Task<bool> GetPeakTask()
        {
            return BufferBlock.OutputAvailableAsync();
        }

        internal override Task GetTask()
        {
            return ReceiveAsync();
        }

        internal override bool TryReceiveOneInternal(out object obj)
        {
            T res;
            if (!TryReceiveOne(out res))
            {
                obj = null;
                return false;
            }

            obj = res;
            return true;
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

        public static async Task<EventData> ReceiveOne(params EventReceiverBase[] events)
        {
            while (true)
            {
                var tasks = new Task[events.Length];
                for (var i = 0; i < events.Length; i++)
                {
                    tasks[i] = events[i].GetPeakTask();
                }

                await Task.WhenAny(tasks);

                for (var i = 0; i < events.Length; i++)
                {
                    if (!tasks[i].IsCompleted) continue;

                    object data;
                    if (!events[i].TryReceiveOneInternal(out data)) continue;

                    var res = new EventData
                    {
                        Data = data,
                        Receiver = events[i]
                    };
                    return res;
                }
            }
        }
    }
}
