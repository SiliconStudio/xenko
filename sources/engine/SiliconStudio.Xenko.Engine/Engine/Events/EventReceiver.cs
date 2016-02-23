// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SiliconStudio.Xenko.Engine.Events
{
    public class EventReceiver<T> : IDisposable
    {
        private readonly IDisposable link;

        // ReSharper disable once StaticMemberInGenericType
        private static readonly DataflowBlockOptions CapacityOptions = new DataflowBlockOptions
        {
            BoundedCapacity = 1
        };

        /// <summary>
        /// Creates an event receiver, ready to receive broadcasts from the key
        /// </summary>
        /// <param name="key">The event key to listen from</param>
        /// <param name="buffered">If we want to process things in a deferred way buffering might become necessary, in that case set this parameter to true</param>
        public EventReceiver(EventKey<T> key, bool buffered = false)
        {
            if (buffered)
            {
                BufferBlock = new BufferBlock<T>(CapacityOptions);
            }
            else
            {
                BufferBlock = new BufferBlock<T>();
            }

            link = key.Connect(BufferBlock);
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
        }

        /// <summary>
        /// Exposes the Tasks.Dataflow object, in the case of custom usage
        /// </summary>
        public BufferBlock<T> BufferBlock { get; }
    }
}