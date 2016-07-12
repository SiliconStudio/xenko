using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SiliconStudio.Core.Threading
{
    /// <summary>
    /// A collector that allows for concurrent adding of items, as well as non-thread-safe clearing and accessing of the underlying colletion.
    /// </summary>
    /// <typeparam name="T">The element type in the collection.</typeparam>
    public class ConcurrentCollector<T> : IReadOnlyList<T>
    {
        private const int DefaultCapacity = 4;

        private int count;

        public T[] Items { get; private set; } = new T[0];
        
        public int Add(T item)
        {
            var index = Interlocked.Increment(ref count) - 1;

            if (Items.Length < index + 1)
            {
                lock (Items)
                {
                    EnsureCapacity(index + 1);
                }
            }

            Items[index] = item;

            return index;
        }

        public void Clear(bool fastClear)
        {
            if (!fastClear && count > 0)
            {
                Array.Clear(Items, 0, count);
            }
            count = 0;
        }

        private void EnsureCapacity(int min)
        {
            if (Items.Length < min)
            {
                int capacity = (Items.Length == 0) ? DefaultCapacity : (Items.Length * 2);
                if (capacity < min)
                {
                    capacity = min;
                }

                var destinationArray = new T[capacity];
                if (Items.Length > 0)
                {
                    Array.Copy(Items, 0, destinationArray, 0, Items.Length);
                }
                Items = destinationArray;
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