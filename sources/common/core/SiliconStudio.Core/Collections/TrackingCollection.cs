// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Specialized;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Core.Collections
{
    /// <summary>
    /// Represents a collection that generates events when items get added or removed.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    [DataSerializer(typeof(ListAllSerializer<,>), Mode = DataSerializerGenericMode.TypeAndGenericArguments)]
    public class TrackingCollection<T> : FastCollection<T>, ITrackingCollectionChanged
    {
        private EventHandler<TrackingCollectionChangedEventArgs> itemAdded;
        private EventHandler<TrackingCollectionChangedEventArgs> itemRemoved;

        /// <inheritdoc/>
        public event EventHandler<TrackingCollectionChangedEventArgs> CollectionChanged
        {
            add
            {
                // We keep a list in reverse order for removal, so that we can easily have multiple handlers depending on each others
                itemAdded = (EventHandler<TrackingCollectionChangedEventArgs>)Delegate.Combine(itemAdded, value);
                itemRemoved = (EventHandler<TrackingCollectionChangedEventArgs>)Delegate.Combine(value, itemRemoved);
            }
            remove
            {
                itemAdded = (EventHandler<TrackingCollectionChangedEventArgs>)Delegate.Remove(itemAdded, value);
                itemRemoved = (EventHandler<TrackingCollectionChangedEventArgs>)Delegate.Remove(itemRemoved, value);
            }
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            var collectionChanged = itemAdded;
            if (collectionChanged != null)
                collectionChanged(this, new TrackingCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, null, index, true));
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            var collectionChanged = itemRemoved;
            if (collectionChanged != null)
                collectionChanged(this, new TrackingCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this[index], null, index, true));
            base.RemoveItem(index);
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            ClearItemsEvents();
            base.ClearItems();
        }

        protected void ClearItemsEvents()
        {
            var collectionChanged = itemRemoved;
            if (collectionChanged != null)
            {
                for (int i = this.Count - 1; i >= 0; --i)
                    collectionChanged(this, new TrackingCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this[i], null, i, true));
            }
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, T item)
        {
            // Note: Changing CollectionChanged is not thread-safe
            var collectionChangedRemoved = itemRemoved;

            object oldItem = (collectionChangedRemoved != null) ? (object)this[index] : null;
            if (collectionChangedRemoved != null)
                collectionChangedRemoved(this, new TrackingCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, null, index, false));

            base.SetItem(index, item);

            var collectionChangedAdded = itemAdded;
            if (collectionChangedAdded != null)
                collectionChangedAdded(this, new TrackingCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, oldItem, index, false));
        }
    }

    /// <summary>
    /// Represents a collection that generates events when items get added or removed.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    [DataSerializer(typeof(ListAllSerializer<,>), Mode = DataSerializerGenericMode.TypeAndGenericArguments)]
    public abstract class TrackingCollection2<T> : FastCollection<T>
    {
        protected abstract void AddItem2(int index, T item);
        protected abstract void RemoveItem2(int index, T item);

        /// <inheritdoc/>
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            AddItem2(index, item);
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            RemoveItem2(index, this[index]);
            base.RemoveItem(index);
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            for (int i = this.Count - 1; i >= 0; --i)
                RemoveItem2(i, this[i]);
            base.ClearItems();
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, T item)
        {
            RemoveItem2(index, this[index]);
            base.SetItem(index, item);
            AddItem2(index, this[index]);
        }
    }
}