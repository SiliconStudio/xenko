// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SiliconStudio.Xenko.Engine.Events
{
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
        /// <param name="attachedScript">The script from where this receiver is created, useful if we have the ClearEveryFrame option set</param>
        /// <param name="options">Option flags</param>
        public EventReceiver(EventKey key, ScriptComponent attachedScript, EventReceiverOptions options = EventReceiverOptions.None) : base(key, attachedScript, options)
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

    /// <summary>
    /// Creates an event receiver that is used to receive T type events from an EventKey
    /// </summary>
    /// <typeparam name="T">The type of data the EventKey will send</typeparam>
    public class EventReceiver<T> : IDisposable
    {
        private IDisposable link;
        private readonly ScriptComponent attachedScript;
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
        }

        /// <summary>
        /// Creates an event receiver, ready to receive broadcasts from the key
        /// </summary>
        /// <param name="key">The event key to listen from</param>
        /// <param name="options">Option flags</param>
        public EventReceiver(EventKey<T> key, EventReceiverOptions options = EventReceiverOptions.None)
        {
            Init(Key, options);
        }

        /// <summary>
        /// Creates an event receiver, ready to receive broadcasts from the key
        /// </summary>
        /// <param name="key">The event key to listen from</param>
        /// <param name="attachedScript">The script from where this receiver is created, useful if we have the ClearEveryFrame option set</param>
        /// <param name="options">Option flags</param>
        public EventReceiver(EventKey<T> key, ScriptComponent attachedScript, EventReceiverOptions options = EventReceiverOptions.None)
        {
            Init(Key, options);

            this.attachedScript = attachedScript;
            var clearEveryFrame = ((options & EventReceiverOptions.ClearEveryFrame) != 0) && attachedScript != null;
            if (!clearEveryFrame) return;

            cancellationTokenSource = new CancellationTokenSource();
            attachedScript.Script.AddTask(async () =>
            {
                while(!cancellationTokenSource.IsCancellationRequested)
                {
                    //Todo this is not really optimal probably but its the only proper way with dataflow
                    IList<T> result;
                    BufferBlock.TryReceiveAll(out result);
                        
                    await this.attachedScript.Script.NextFrame();
                }
            }, attachedScript.Priority + 1);
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

        /// <summary>
        /// Removes the link between this receiver and the broadcaster
        /// </summary>
        public void Dispose()
        {
            link.Dispose();
            cancellationTokenSource.Cancel();
        }     
    }
}
