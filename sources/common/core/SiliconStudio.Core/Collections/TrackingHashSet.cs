// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace SiliconStudio.Core.Collections
{
    /// <summary>
    /// Represents a hash set that generates events when items get added or removed.
    /// </summary>
    /// <remarks>
    /// Underlying storage is done with a <see cref="HashSet{T}"/>.
    /// </remarks>
    /// <typeparam name="T">The type of elements in the hash set.</typeparam>
    public class TrackingHashSet<T> : ISet<T>, IReadOnlySet<T>, ITrackingCollectionChanged
    {
        private HashSet<T> innerHashSet = new HashSet<T>();

        /// <inheritdoc/>
        public event EventHandler<TrackingCollectionChangedEventArgs> CollectionChanged;

        /// <inheritdoc/>
        public bool Add(T item)
        {
            if (innerHashSet.Add(item))
            {
                if (CollectionChanged != null)
                    CollectionChanged(this, new TrackingCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, null, -1, true));
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return innerHashSet.IsProperSubsetOf(other);
        }

        /// <inheritdoc/>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return innerHashSet.IsProperSupersetOf(other);
        }

        /// <inheritdoc/>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return innerHashSet.IsSubsetOf(other);
        }

        /// <inheritdoc/>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return innerHashSet.IsSupersetOf(other);
        }

        /// <inheritdoc/>
        public bool Overlaps(IEnumerable<T> other)
        {
            return innerHashSet.Overlaps(other);
        }

        /// <inheritdoc/>
        public bool SetEquals(IEnumerable<T> other)
        {
            return innerHashSet.SetEquals(other);
        }

        /// <inheritdoc/>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        void ICollection<T>.Add(T item)
        {
            innerHashSet.Add(item);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            if (CollectionChanged != null)
            {
                foreach (var item in innerHashSet.ToArray())
                {
                    Remove(item);
                }
            }
            else
            {
                innerHashSet.Clear();
            }
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            return innerHashSet.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            innerHashSet.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public int Count
        {
            get { return innerHashSet.Count; }
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get { return ((ICollection<T>)innerHashSet).IsReadOnly; }
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            var result = innerHashSet.Remove(item);

            if (CollectionChanged != null && result)
                CollectionChanged(this, new TrackingCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, null, -1, true));

            return result;
        }

        /// <inheritdoc/>
        public HashSet<T>.Enumerator GetEnumerator()
        {
            return innerHashSet.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return innerHashSet.GetEnumerator();
        }

        /// <inheritdoc/>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return innerHashSet.GetEnumerator();
        }
    }
}