// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Pools input events of a given type
    /// </summary>
    /// <typeparam name="TEventType">The type of event to pool</typeparam>
    public static class InputEventPool<TEventType> where TEventType : InputEvent, new()
    {
        private static Queue<TEventType> eventPool;

        /// <summary>
        /// The number of pointer events in circulation, if this number keeps increasing, Enqueue is possible not called somewhere
        /// </summary>
        public static int PoolSize { get; private set; } = 0;

        static InputEventPool()
        {
            eventPool = new Queue<TEventType>();
        }

        /// <summary>
        /// Retrieves a new event that can be used, either from the pool or a new instance
        /// </summary>
        /// <param name="device">The device that generates this event</param>
        /// <returns>An event</returns>
        public static TEventType GetOrCreate(IInputDevice device)
        {
            TEventType evt;
            if (eventPool.Count > 0)
                evt = eventPool.Dequeue();
            else
            {
                evt = new TEventType();
                PoolSize++;
            }

            evt.Device = device;
            return evt;
        }
        
        /// <summary>
        /// Puts a used event back into the pool to be recycled
        /// </summary>
        /// <param name="evt">The event to reuse</param>
        public static void Enqueue(TEventType evt)
        {
            eventPool.Enqueue(evt);
        }
    }
}