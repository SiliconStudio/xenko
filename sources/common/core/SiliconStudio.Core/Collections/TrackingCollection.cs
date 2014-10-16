// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Core.Collections
{
    public interface ITrackingCollectionChanged
    {
        event EventHandler<TrackingCollectionChangedEventArgs> CollectionChanged;
    }

    /// <summary>
    /// Overrides <see cref="Collection{T}"/> with value types enumerators to avoid allocation in foreach loops, and various helper functions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataSerializer(typeof(ListAllSerializer<,>), Mode = DataSerializerGenericMode.TypeAndGenericArguments)]
    public class FastCollection<T> : Collection<T>
    {
        /// <summary>
        /// Adds the elements of the specified source to the end of <see cref="FastCollection{T}"/>.
        /// </summary>
        /// <param name="items">The items.</param>
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator</returns>
        public new List<T>.Enumerator GetEnumerator()
        {
            // Assume the underlying collection is a list. (true when created with null constructor -> TODO improve this code in future)
            return ((List<T>)Items).GetEnumerator();
        }

        /// <summary>
        /// Sorts the element in this <see cref="FastCollection{T}"/> using the default comparer.
        /// </summary>
        public void Sort()
        {
            //Assume the underlying collection is a list. (true when created with null constructor -> TODO improve this code in future)
            ((List<T>)Items).Sort();
        }

        /// <summary>
        /// Sorts the element in this <see cref="FastCollection{T}"/> using the specified comparer.
        /// </summary>
        /// <param name="comparison">The comparison to use.</param>
        public void Sort(Comparison<T> comparison)
        {
            //Assume the underlying collection is a list. (true when created with null constructor -> TODO improve this code in future)
            ((List<T>)Items).Sort(comparison);
        }

        /// <summary>
        /// Sorts the element in this <see cref="FastCollection{T}"/> using the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer to use.</param>
        public void Sort(Comparer<T> comparer)
        {
            //Assume the underlying collection is a list. (true when created with null constructor -> TODO improve this code in future)
            ((List<T>)Items).Sort(comparer);
        }
    }

    /// <summary>
    /// Represents a collection that generates events when items get added or removed.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    [DataSerializer(typeof(ListAllSerializer<,>), Mode = DataSerializerGenericMode.TypeAndGenericArguments)]
    public class TrackingCollection<T> : FastCollection<T>, ITrackingCollectionChanged
    {
        /// <inheritdoc/>
        public event EventHandler<TrackingCollectionChangedEventArgs> CollectionChanged;

        /// <inheritdoc/>
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            var collectionChanged = CollectionChanged;
            if (collectionChanged != null)
                collectionChanged(this, new TrackingCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, null, index, true));
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            var collectionChanged = CollectionChanged;
            if (collectionChanged != null)
                collectionChanged(this, new TrackingCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, Items[index], null, index, true));
            base.RemoveItem(index);
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            var collectionChanged = CollectionChanged;
            if (collectionChanged != null)
            {
                for (int i = Items.Count - 1; i >= 0; --i)
                    collectionChanged(this, new TrackingCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, Items[i], null, i, true));
            }
            base.ClearItems();
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, T item)
        {
            var collectionChanged = CollectionChanged;
            object oldItem = (collectionChanged != null) ? (object)Items[index] : null;
            if (collectionChanged != null)
                collectionChanged(this, new TrackingCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, null, index, false));
            base.SetItem(index, item);
            if (collectionChanged != null)
                collectionChanged(this, new TrackingCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, oldItem, index, false));
        }
    }
}