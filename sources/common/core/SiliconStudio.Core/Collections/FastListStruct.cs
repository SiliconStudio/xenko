// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SiliconStudio.Core.Collections
{
    public struct FastListStruct<T> : IEnumerable<T>
    {
        private static readonly T[] emptyArray = new T[0];

        public int Count;

        /// <summary>
        /// Gets the items.
        /// </summary>
        public T[] Items;

        public T this[int index]
        {
            get { return Items[index]; }
            set
            {
                Items[index] = value;
            }
        }

        public FastListStruct(FastList<T> fastList)
        {
            this.Count = fastList.Count;
            this.Items = fastList.Items;
        }

        public FastListStruct(T[] array)
        {
            this.Count = array.Length;
            this.Items = array;
        }

        public FastListStruct(int capacity)
        {
            this.Count = 0;
            this.Items = capacity == 0 ? emptyArray : new T[capacity];
        }

        public void Add(T item)
        {
            if (this.Count == this.Items.Length)
                this.EnsureCapacity(this.Count + 1);
            this.Items[this.Count++] = item;
        }

        public void AddRange(FastListStruct<T> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                Add(items[i]);
            }
        }

        public void Insert(int index, T item)
        {
            if (Count == Items.Length)
            {
                EnsureCapacity(Count + 1);
            }
            if (index < Count)
            {
                for (int i = Count; i > index; --i)
                {
                    Items[i] = Items[i - 1];
                }
            }
            Items[index] = item;
            Count++;
        }

        public void RemoveAt(int index)
        {
            Count--;
            if (index < Count)
            {
                Array.Copy(Items, index + 1, Items, index, Count - index);
            }
            Items[Count] = default(T);
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public void Clear()
        {
            this.Count = 0;
        }

        public T[] ToArray()
        {
            var destinationArray = new T[Count];
            Array.Copy(Items, 0, destinationArray, 0, Count);
            return destinationArray;            
        }

        public void EnsureCapacity(int newCapacity)
        {
            if (this.Items.Length < newCapacity)
            {
                int newSize = this.Items.Length * 2;
                if (newSize < newCapacity)
                    newSize = newCapacity;

                var destinationArray = new T[newSize];
                Array.Copy(this.Items, 0, destinationArray, 0, this.Count);
                this.Items = destinationArray;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(Items, Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(Items, Count);
        }
        
        public Enumerator GetEnumerator()
        {
            return new Enumerator(Items, Count);
        }

        public static implicit operator FastListStruct<T>(FastList<T> fastList)
        {
            return new FastListStruct<T>(fastList);
        }

        public static implicit operator FastListStruct<T>(T[] array)
        {
            return new FastListStruct<T>(array);
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(Items, item, 0, Count);
        }

        /// <summary>
        /// Remove an item by swapping it with the last item and removing it from the last position. This function prevents to shift values from the list on removal but does not maintain order.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="index">Index of the item to remove.</param>
        public void SwapRemoveAt(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException("index");

            if (index < Count - 1)
            {
                Items[index] = Items[Count - 1];
            }

            RemoveAt(Count - 1);
        }

        #region Nested type: Enumerator

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private T[] items;
            private int count;
            private int index;
            private T current;

            internal Enumerator(T[] items, int count)
            {
                this.items = items;
                this.count = count;
                index = 0;
                current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (index < count)
                {
                    current = items[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                index = count + 1;
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