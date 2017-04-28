// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Pools input events of a given type
    /// </summary>
    /// <typeparam name="TEventType">The type of event to pool</typeparam>
    public static class InputEventPool<TEventType> where TEventType : InputEvent, new()
    {
        private static PoolListStruct<TEventType> eventPool;

        /// <summary>
        /// The number of events in circulation, if this number keeps increasing, Enqueue is possible not called somewhere
        /// </summary>
        public static int ActiveObjects => eventPool.Count;

        static InputEventPool()
        {
            eventPool = new PoolListStruct<TEventType>(8, CreateEvent);
        }

        private static TEventType CreateEvent()
        {
            return new TEventType();
        }

        /// <summary>
        /// Retrieves a new event that can be used, either from the pool or a new instance
        /// </summary>
        /// <param name="device">The device that generates this event</param>
        /// <returns>An event</returns>
        public static TEventType GetOrCreate(IInputDevice device)
        {
            TEventType item = eventPool.Add();
            item.Device = device;
            return item;
        }
        
        /// <summary>
        /// Puts a used event back into the pool to be recycled
        /// </summary>
        /// <param name="item">The event to reuse</param>
        public static void Enqueue(TEventType item)
        {
            eventPool.Remove(item);
        }
    }
}