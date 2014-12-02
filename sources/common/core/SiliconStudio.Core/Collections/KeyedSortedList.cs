// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Core.Collections
{
    /// <summary>
    /// List of items, stored sequentially and sorted by an implicit invariant key that are extracted from items by implementing <see cref="GetKeyForItem"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="T"></typeparam>
    public abstract class KeyedSortedList<TKey, T> : IList<T>, IList
    {
        private readonly IComparer<TKey> comparer;
        protected FastListStruct<T> items = new FastListStruct<T>(1);

        protected KeyedSortedList() : this(null)
        {
        }

        protected KeyedSortedList(IComparer<TKey> comparer)
        {
            if (comparer == null)
                comparer = Comparer<TKey>.Default;

            this.comparer = comparer;
        }

        /// <summary>
        /// Extracts the key for the specified element.
        /// </summary>
        /// <param name="item">The element from which to extract the key.</param>
        /// <returns></returns>
        protected abstract TKey GetKeyForItem(T item);

        /// <summary>
        /// Called every time an item should be added at a given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected virtual void InsertItem(int index, T item)
        {
            items.Insert(index, item);
        }

        /// <summary>
        /// Called every time an item should be removed at a given index.
        /// </summary>
        /// <param name="index">The index.</param>
        protected virtual void RemoveItem(int index)
        {
            items.RemoveAt(index);
        }

        /// <summary>
        /// Sorts again this list (in case keys were mutated).
        /// </summary>
        public void Sort()
        {
            Array.Sort(items.Items, 0, items.Count, new Comparer(this));
        }

        /// <inheritdoc/>
        public void Add(T item)
        {
            var key = GetKeyForItem(item);

            var index = BinarySearch(key);
            if (index >= 0)
                throw new InvalidOperationException("An item with the same key has already been added.");

            InsertItem(~index, item);
        }

        public bool ContainsKey(TKey key)
        {
            return BinarySearch(key) >= 0;
        }

        public bool Remove(TKey key)
        {
            var index = BinarySearch(key);
            if (index < 0)
                return false;

            RemoveItem(index);

            return true;
        }

        /// <inheritdoc/>
        public T this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }

        public T this[TKey key]
        {
            get
            {
                int index = BinarySearch(key);
                if (index < 0)
                    throw new KeyNotFoundException();
                return items[index];
            }
            set
            {
                int index = BinarySearch(key);
                if (index >= 0)
                    items[index] = value;
                else
                    items.Insert(~index, value);
            }
        }

        public bool TryGetValue(TKey key, out T value)
        {
            int index = BinarySearch(key);
            if (index < 0)
            {
                value = default(T);
                return false;
            }

            value = items[index];
            return true;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            items.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        /// <inheritdoc/>
        int IList.Add(object value)
        {
            int index = items.Count;
            Add((T)value);
            return index;
        }

        /// <inheritdoc/>
        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        /// <inheritdoc/>
        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        /// <inheritdoc/>
        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        bool ICollection<T>.Remove(T item)
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public int Count
        {
            get { return items.Count; }
        }

        public object SyncRoot { get; private set; }

        public bool IsSynchronized { get; private set; }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsFixedSize { get; private set; }

        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            return items.IndexOf(item);
        }

        /// <inheritdoc/>
        void IList<T>.Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get { return items[index]; }
            set { items[index] = (T)value; }
        }

        /// <inheritdoc/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(items);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(items);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(items);
        }

        public int BinarySearch(TKey searchKey)
        {
            var values = items.Items;
            int start = 0;
            int end = items.Count - 1;

            while (start <= end)
            {
                int middle = start + ((end - start) >> 1);
                var itemKey = GetKeyForItem(values[middle]);

                var compareResult = comparer.Compare(itemKey, searchKey);

                if (compareResult == 0)
                {
                    return middle;
                }
                if (compareResult < 0)
                {
                    start = middle + 1;
                }
                else
                {
                    end = middle - 1;
                }
            }
            return ~start;
        }

        struct Comparer : IComparer<T>
        {
            private KeyedSortedList<TKey, T> list;

            internal Comparer(KeyedSortedList<TKey, T> list)
            {
                this.list = list;
            }

            public int Compare(T x, T y)
            {
                return list.comparer.Compare(list.GetKeyForItem(x), list.GetKeyForItem(y));
            }
        }
 
        #region Nested type: Enumerator

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private readonly FastListStruct<T> list;
            private int index;
            private T current;

            internal Enumerator(FastListStruct<T> list)
            {
                this.list = list;
                index = 0;
                current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (index < list.Count)
                {
                    current = list.Items[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                index = list.Count + 1;
                current = default(T);
                return false;
            }

            public T Current
            {
                get { return current; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            void IEnumerator.Reset()
            {
                index = 0;
                current = default(T);
            }
        }

        #endregion
    }
}