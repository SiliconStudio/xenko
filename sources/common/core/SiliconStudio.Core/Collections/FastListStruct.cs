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
            if (capacity <= 0) throw new ArgumentOutOfRangeException("capacity", "Must be > 0");
            this.Count = 0;
            this.Items = new T[capacity];
        }

        public void Add(T item)
        {
            if (this.Count == this.Items.Length)
                this.EnsureCapacity(this.Count + 1);
            this.Items[this.Count++] = item;
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

        public void Clear()
        {
            this.Count = 0;
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