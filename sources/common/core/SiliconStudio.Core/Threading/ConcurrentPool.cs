// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading;

namespace SiliconStudio.Core.Threading
{
    /// <summary>
    /// A lockless, allocation-free concurrent object pool.
    /// </summary>
    /// <typeparam name="T">The pooled item type</typeparam>
    public class ConcurrentPool<T>
        where T : class
    {
        private class Segment
        {
            /// <summary>
            /// The array of items. Length must be a power of two.
            /// </summary>
            public readonly T[] Items;

            /// <summary>
            /// A bit mask for calculation of (Low % Items.Length) and (High % Items.Length)
            /// </summary>
            public readonly int Mask;

            /// <summary>
            /// The read index for Release. It is only ever incremented and safe to overflow.
            /// </summary>
            public int Low;

            /// <summary>
            /// The write index for Acquire. It is only ever incremented and safe to overflow.
            /// </summary>
            public int High;

            /// <summary>
            /// The current number of stored items, used to check when to change head and tail segments.
            /// </summary>
            public int Count;

            /// <summary>
            /// The next segment to draw from, after this one is emptied.
            /// </summary>
            public Segment Next;

            public Segment(int size)
            {
                Items = new T[size];
                Mask = size - 1;
            }
        }

        private const int DefaultCapacity = 4;

        private readonly object resizeLock = new object();
        private readonly Func<T> factory;
        private Segment head;
        private Segment tail;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="factory">The factory method for creating new items, should the pool be empty.</param>
        public ConcurrentPool(Func<T> factory)
        {
            head = tail = new Segment(DefaultCapacity);
            this.factory = factory;
        }

        /// <summary>
        /// Draws an item from the pool.
        /// </summary>
        public T Acquire()
        {
            while (true)
            {
                var localHead = head;
                var count = localHead.Count;

                if (count == 0)
                {
                    // If first segment is empty, but there is at least one other, move the head forward.
                    if (localHead.Next != null)
                    {
                        lock (resizeLock)
                        {
                            if (head.Next != null && head.Count == 0)
                            {
                                head = head.Next;
                            }
                        }
                    }
                    else
                    {
                        // If there was only one segment and it was empty, create a new item.
                        return factory();
                    }
                }
                else if (Interlocked.CompareExchange(ref localHead.Count, count - 1, count) == count)
                {
                    // If there were any items and we could reserve one of them, move the
                    // read index forward and get the index of the item we can acquire.
                    var localLow = Interlocked.Increment(ref localHead.Low) - 1;
                    var index = localLow & localHead.Mask;

                    // Take the item. Spin until the slot has been written by pending calls to Release.
                    T item;
                    var spinWait = new SpinWait();
                    while ((item = Interlocked.Exchange(ref localHead.Items[index], null)) == null)
                    {
                        spinWait.SpinOnce();
                    }

                    return item;
                }
            }
        }

        /// <summary>
        /// Releases an item back to the pool.
        /// </summary>
        /// <param name="item">The item to release to the pool.</param>
        public void Release(T item)
        {
            while (true)
            {
                var localTail = tail;
                var count = localTail.Count;

                // If the segment was full, allocate and append a new, bigger one.
                if (count == localTail.Items.Length)
                {
                    lock (resizeLock)
                    {
                        if (tail.Next == null && count == localTail.Items.Length)
                        {
                            tail = tail.Next = new Segment(tail.Items.Length << 1);
                        }
                    }
                }
                else if (Interlocked.CompareExchange(ref localTail.Count, count + 1, count) == count)
                {
                    // TODO: Is it possible that we write to the head-segment after it was discarded, because it was empty at the time?

                    // If there was space for another item and we were able to reserve it, move the
                    // write index forward and get the index of the slot we can write into.
                    var localHigh = Interlocked.Increment(ref localTail.High) - 1;
                    var index = localHigh & localTail.Mask;

                    // Write the item. Spin until the slot has been cleared by pending calls to Acquire.
                    var spinWait = new SpinWait();
                    while (Interlocked.CompareExchange(ref localTail.Items[index], item, null) != null)
                    {
                        spinWait.SpinOnce();
                    }

                    return;
                }
            }
        }
    }
}