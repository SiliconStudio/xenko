// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Collections
{
    /// <summary>
    /// A pool of objects allocated and can be clear without loosing previously allocated instance.
    /// </summary>
    /// <typeparam name="T">Type of the pooled object</typeparam>

    public struct PoolListStruct<T> : IEnumerable<T> where T : class
    {
        // TODO: Add method to return to the pool a specific instance/index

        /// <summary>
        /// The list of allocated objects.
        /// </summary>
        private FastListStruct<T> allocated;

        /// <summary>
        /// The number of objects in use, readonly.
        /// </summary>
        public int Count;

        /// <summary>
        /// A factory to allocate new objects.
        /// </summary>
        private readonly Func<T> factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PoolListStruct{T}"/> struct.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        /// <param name="factory">The factory.</param>
        /// <exception cref="System.ArgumentNullException">factory</exception>
        public PoolListStruct(int capacity, [NotNull] Func<T> factory)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            this.factory = factory;
            allocated = new FastListStruct<T>(capacity);
            Count = 0;
        }

        /// <summary>
        /// Clears objects in use and keep allocated objects.
        /// </summary>
        public void Clear()
        {
            Count = 0;
        }

        /// <summary>
        /// Resets this instance by releasing allocated objects.
        /// </summary>
        public void Reset()
        {
            Clear();
            for (var i = 0; i < allocated.Count; i++)
            {
                allocated[i] = null;
            }
            allocated.Clear();
        }

        /// <summary>
        /// Adds a new object in use to this instance.
        /// </summary>
        /// <returns>An instance of T</returns>
        public T Add()
        {
            T result;
            if (Count < allocated.Count)
            {
                result = allocated[Count];
            }
            else
            {
                result = factory();
                allocated.Add(result);
            }
            Count++;

            return result;
        }

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>An instance of T</returns>
        public T this[int index]
        {
            get { return allocated.Items[index]; }
            set
            {
                allocated.Items[index] = value;
            }
        }

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>An instance of T</returns>
        public T this[uint index]
        {
            get { return allocated.Items[index]; }
            set
            {
                allocated.Items[index] = value;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new FastListStruct<T>.Enumerator(allocated.Items, Count);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new FastListStruct<T>.Enumerator(allocated.Items, Count);
        }

        public FastListStruct<T>.Enumerator GetEnumerator()
        {
            return new FastListStruct<T>.Enumerator(allocated.Items, Count);
        }
    }
}
