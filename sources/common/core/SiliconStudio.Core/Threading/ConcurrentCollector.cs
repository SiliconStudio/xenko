using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SiliconStudio.Core.Threading
{
    public class ConcurrentCollectorCache<T>
    {
        private readonly int capacity;
        private readonly List<T> cache = new List<T>();
        private ConcurrentCollector<T> currentCollection;

        public ConcurrentCollectorCache(int capacity)
        {
            this.capacity = capacity;
        }

        public void Add(ConcurrentCollector<T> collection, T item)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            if (currentCollection != collection || cache.Count > capacity)
            {
                if (currentCollection != null)
                {
                    currentCollection.AddRange(cache);
                    cache.Clear();
                }
                currentCollection = collection;
            }

            cache.Add(item);
        }

        public void Flush()
        {
            if (currentCollection != null)
            {
                currentCollection.AddRange(cache);
                cache.Clear();
            }
            currentCollection = null;
        }
    }

    public static class ConcurrentCollectorExtensions
    {
        public static void Add<T>(this ConcurrentCollector<T> collection, T item, ConcurrentCollectorCache<T> cache)
        {
            cache.Add(collection, item);
        }
    }

    /// <summary>
    /// A collector that allows for concurrent adding of items, as well as non-thread-safe clearing and accessing of the underlying colletion.
    /// </summary>
    /// <typeparam name="T">The element type in the collection.</typeparam>
    public class ConcurrentCollector<T> : IReadOnlyList<T>
    {
        private const int DefaultCapacity = 16;

        private class Segment
        {
            public T[] Items;
            public int Offset;
            public Segment Previous;
            public Segment Next;
        }

        private readonly object resizeLock = new object();
        private readonly Segment head;
        private Segment tail;
        private int count;

        public ConcurrentCollector()
        {
            tail = head = new Segment { Items = new T[DefaultCapacity] };
        }

        public T[] Items
        {
            get
            {
                // If there are multiple segments, consolidate them
                if (head.Next != null)
                {
                    var newItems = new T[tail.Offset + tail.Items.Length];

                    var segment = head;
                    while (segment != null)
                    {
                        Array.Copy(segment.Items, 0, newItems, segment.Offset, segment.Items.Length);
                        segment = segment.Next;
                    }

                    head.Items = newItems;
                    head.Next = null;

                    tail = head;
                }

                return head.Items;
            }
        }
        
        public int Add(T item)
        {
            var index = Interlocked.Increment(ref count) - 1;

            var segment = tail;
            if (index >= segment.Offset + segment.Items.Length)
            {
                lock (resizeLock)
                {
                    if (index >= tail.Offset + tail.Items.Length)
                    {
                        tail.Next = new Segment
                        {
                            Items = new T[segment.Items.Length * 2],
                            Offset = segment.Offset + segment.Items.Length,
                            Previous = tail
                        };

                        tail = tail.Next;
                    }

                    segment = tail;
                }
            }

            while (index < segment.Offset)
            {
                segment = segment.Previous;
            }

            segment.Items[index - segment.Offset] = item;

            return index;
        }

        public void AddRange(IReadOnlyList<T> collection)
        {
            var newCount = Interlocked.Add(ref count, collection.Count);

            var segment = tail;
            if (newCount >= segment.Offset + segment.Items.Length)
            {
                lock (resizeLock)
                {
                    if (newCount >= tail.Offset + tail.Items.Length)
                    {
                        var capacity = tail.Offset + tail.Items.Length;
                        var size = Math.Max(capacity, newCount - capacity);

                        tail.Next = new Segment
                        {
                            Items = new T[size],
                            Offset = capacity,
                            Previous = tail
                        };

                        tail = tail.Next;
                    }

                    segment = tail;
                }
            }

            // Find the segment containing the last index
            while (newCount <= segment.Offset)
                segment = segment.Previous;
            var destinationIndex = newCount - segment.Offset - 1;

            for (int sourceIndex = collection.Count - 1; sourceIndex >= 0; sourceIndex--)
            {
                if (destinationIndex < 0)
                {
                    segment = segment.Previous;
                    destinationIndex = segment.Items.Length - 1;
                }

                segment.Items[destinationIndex] = collection[sourceIndex];
                destinationIndex--;
            }
        }

        public void Clear(bool fastClear)
        {
            if (!fastClear && count > 0)
            {
                Array.Clear(Items, 0, count);
            }
            count = 0;
        }

        private static void EnsureCapacity(ref T[] items, int min)
        {
            if (items.Length < min)
            {
                int capacity = (items.Length == 0) ? DefaultCapacity : (items.Length * 2);
                if (capacity < min)
                {
                    capacity = min;
                }

                var destinationArray = new T[capacity];
                if (items.Length > 0)
                {
                    Array.Copy(items, 0, destinationArray, 0, items.Length);
                }
                items = destinationArray;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
            {
                yield return Items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => count;

        public T this[int index]
        {
            get
            {
                return Items[index];
            }
            set
            {
                Items[index] = value;
            }
        }
    }
}