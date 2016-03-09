// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SiliconStudio.Xenko.Engine.Events
{
    [Flags]
    public enum EventReceiverOptions
    {
        None,
        Buffered = 1 << 0,
        ClearEveryFrame = 1 << 1
    }

    public class EventReceiver<T> : IDisposable
    {
        private readonly IDisposable link;
        private readonly ScriptComponent attachedScript;
        private readonly CancellationTokenSource cancellationTokenSource;
        internal readonly BufferBlock<T> BufferBlock;

        // ReSharper disable once StaticMemberInGenericType
        private static readonly DataflowBlockOptions CapacityOptions = new DataflowBlockOptions
        {
            BoundedCapacity = 1
        };

        /// <summary>
        /// Creates an event receiver, ready to receive broadcasts from the key
        /// </summary>
        /// <param name="key">The event key to listen from</param>
        /// <param name="options"></param>
        public EventReceiver(EventKey<T> key, EventReceiverOptions options = EventReceiverOptions.None)
        {
            BufferBlock = ((options & EventReceiverOptions.Buffered) != 0) ? new BufferBlock<T>(CapacityOptions) : new BufferBlock<T>();
            link = key.Connect(this);
        }

        /// <summary>
        /// Creates an event receiver, ready to receive broadcasts from the key
        /// </summary>
        /// <param name="key">The event key to listen from</param>
        /// <param name="attachedScript"></param>
        /// <param name="options"></param>
        public EventReceiver(EventKey<T> key, ScriptComponent attachedScript, EventReceiverOptions options = EventReceiverOptions.None)
        {
            BufferBlock = ((options & EventReceiverOptions.Buffered) != 0) ? new BufferBlock<T>(CapacityOptions) : new BufferBlock<T>();
            link = key.Connect(this);
            this.attachedScript = attachedScript;
            var clearEveryFrame = ((options & EventReceiverOptions.ClearEveryFrame) != 0) && attachedScript != null;
            if (!clearEveryFrame) return;

            cancellationTokenSource = new CancellationTokenSource();
            attachedScript.Script.AddTask(async () =>
            {
                while(!cancellationTokenSource.IsCancellationRequested)
                {
                    //Todo this is not really optimal probably
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
            return await BufferBlock.ReceiveAsync();
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
            return !HasEvents() ? default(T) : BufferBlock.Receive();
        }

        /// <summary>
        /// Receives all the events from the queue (if buffered was true during creations), useful mostly only in Sync scripts
        /// </summary>
        /// <returns></returns>
        public IList<T> ReceiveMany()
        {
            IList<T> result;
            return !BufferBlock.TryReceiveAll(out result) ? null : result;
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