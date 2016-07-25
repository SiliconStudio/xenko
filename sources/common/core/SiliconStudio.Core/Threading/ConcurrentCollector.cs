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
        private const int DefaultCapacity = 4;

        private T[] items = new T[0];
        private int count;

        public T[] Items => items;
        
        public int Add(T item)
        {
            var index = Interlocked.Increment(ref count) - 1;
  
            T[] newItems = null;
            T[] oldItems;

            // Try until we wrote to the correct array
            while ((oldItems = items) != newItems)
            {
                newItems = oldItems;

                if (oldItems.Length < index + 1)
                {
                    EnsureCapacity(ref newItems, index + 1);
                    
                    // Early exit, if we are late to resize the array
                    if (Interlocked.CompareExchange(ref items, newItems, oldItems) != oldItems)
                        continue;
                }

                newItems[index] = item;
            }

            return index;
        }

        public void AddRange(IReadOnlyList<T> collection)
        {
            var newCount = Interlocked.Add(ref count, collection.Count);

            T[] newItems = null;
            T[] oldItems;

            // Try until we wrote to the correct array
            while ((oldItems = items) != newItems)
            {
                newItems = oldItems;

                if (oldItems.Length < newCount)
                {
                    EnsureCapacity(ref newItems, newCount);

                    // Early exit, if we are late to resize the array
                    if (Interlocked.CompareExchange(ref items, newItems, oldItems) != oldItems)
                        continue;
                }

                for (int sourceIndex = 0, destinationIndex = newCount - collection.Count; sourceIndex < collection.Count; sourceIndex++, destinationIndex++)
                {
                    newItems[destinationIndex] = collection[sourceIndex];
                }
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