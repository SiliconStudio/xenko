// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SiliconStudio.Xenko.Engine.Processors;

namespace SiliconStudio.Xenko.Engine.Events
{
    /// <summary>
    /// Creates an event receiver that is used to receive T type events from an EventKey
    /// </summary>
    /// <typeparam name="T">The type of data the EventKey will send</typeparam>
    public class EventReceiver<T> : IDisposable
    {
        private IDisposable link;
        private readonly CancellationTokenSource cancellationTokenSource;
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

            ReceiveOne(); //clear any previous event, we don't want to receive old events, as broadcast block will always send us the last avail event on connect
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
                    //consume all events at the end of every frame
                    IList<T> result;
                    BufferBlock.TryReceiveAll(out result);

                    await scheduler.NextFrame();
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

        /// <summary>
        /// Checks if there is any event waiting to be received, useful in Sync scripts
        /// </summary>
        /// <returns></returns>
        public bool HasEvents()
        {
            return BufferBlock.Count > 0;
        }

        /// <summary>
        /// Receives one event from the buffer, useful specially in Sync scripts
        /// </summary>
        /// <returns></returns>
        public T ReceiveOne()
        {
            if (!HasEvents())
            {
                return default(T);
            }

            Key.Logger.Debug(receivedDebugString);

            return BufferBlock.Receive();
        }

        /// <summary>
        /// Receives all the events from the queue (if buffered was true during creations), useful mostly only in Sync scripts
        /// </summary>
        /// <returns></returns>
        public IList<T> ReceiveMany()
        {
            IList<T> result;
            if (!BufferBlock.TryReceiveAll(out result))
            {
                return null;
            }

            Key.Logger.Debug(receivedManyDebugString);

            return result;
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
            await BufferBlock.ReceiveAsync();
        }
    }
}
