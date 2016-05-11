// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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

        internal abstract bool TryReceiveOneInternal(out object obj);
    }

    /// <summary>
    /// Base type for EventReceiver
    /// </summary>
    /// <typeparam name="T">The type of data the EventKey will send</typeparam>
    public class EventReceiverBase<T> : EventReceiverBase, IDisposable
    {
        private IDisposable link;
        private string receivedDebugString;
        private string receivedManyDebugString;

        internal BufferBlock<T> BufferBlock;

        public EventKeyBase<T> Key { get; private set; }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly DataflowBlockOptions CapacityOptions = new DataflowBlockOptions
        {
            BoundedCapacity = 1
        };

        private void Init(EventKeyBase<T> key, EventReceiverOptions options)
        {
            Key = key;

            BufferBlock = ((options & EventReceiverOptions.Buffered) != 0) ? new BufferBlock<T>() : new BufferBlock<T>(CapacityOptions);

            link = key.Connect(this);

            receivedDebugString = $"Received '{key.EventName}' ({key.EventId})";
            receivedManyDebugString = $"Received All '{key.EventName}' ({key.EventId})";

            T foo;
            InternalTryReceiveOne(out foo); //clear any previous event, we don't want to receive old events, as broadcast block will always send us the last avail event on connect
        }

        /// <summary>
        /// Creates an event receiver, ready to receive broadcasts from the key
        /// </summary>
        /// <param name="key">The event key to listen from</param>
        /// <param name="options">Option flags</param>
        internal EventReceiverBase(EventKeyBase<T> key, EventReceiverOptions options = EventReceiverOptions.None)
        {
            Init(key, options);
        }

        /// <summary>
        /// Awaits a single event
        /// </summary>
        /// <returns></returns>
        protected async Task<T> InternalReceiveAsync()
        {
            var res = await BufferBlock.ReceiveAsync();

            Key.Logger.Debug(receivedDebugString);

            return res;
        }

        public EventReceiverAwaiter<T> GetAwaiter()
        {
            return new EventReceiverAwaiter<T>(InternalReceiveAsync().GetAwaiter());
        }

        /// <summary>
        /// Returns the count of currently buffered events
        /// </summary>
        public int Count => BufferBlock.Count;

        /// <summary>
        /// Receives one event from the buffer, useful specially in Sync scripts
        /// </summary>
        /// <returns></returns>
        protected bool InternalTryReceiveOne(out T data)
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
        protected int InternalTryReceiveMany(ICollection<T> collection)
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
                collection?.Add(e);
            }

            return count;
        }

        /// <summary>
        /// Clears all currently buffered events.
        /// </summary>
        public void Reset()
        {
            //console all in one go
            IList<T> result;
            BufferBlock.TryReceiveAll(out result);
        }

        ~EventReceiverBase()
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

        internal override bool TryReceiveOneInternal(out object obj)
        {
            T res;
            if (!InternalTryReceiveOne(out res))
            {
                obj = null;
                return false;
            }

            obj = res;
            return true;
        }
    }

    /// <summary>
    /// Creates an event receiver that is used to receive T type events from an EventKey
    /// </summary>
    /// <typeparam name="T">The type of data the EventKey will send</typeparam>
    public sealed class EventReceiver<T> : EventReceiverBase<T>
    {
        public EventReceiver(EventKey<T> key, EventReceiverOptions options = EventReceiverOptions.None) : base(key, options)
        {
        }

        public Task<T> ReceiveAsync()
        {
            return InternalReceiveAsync();
        }

        public bool TryReceiveOne(out T data)
        {
            return InternalTryReceiveOne(out data);
        }

        public int TryReceiveMany(ICollection<T> collection)
        {
            return InternalTryReceiveMany(collection);
        }
    }

    /// <summary>
    /// Creates an event receiver that is used to receive events from an EventKey
    /// </summary>
    public sealed class EventReceiver : EventReceiverBase<bool>
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
        public async Task ReceiveAsync()
        {
            await InternalReceiveAsync();
        }

        public bool TryReceiveOne()
        {
            bool foo;
            return InternalTryReceiveOne(out foo);
        }

        public int TryReceiveMany()
        {
            return InternalTryReceiveMany(null);
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
