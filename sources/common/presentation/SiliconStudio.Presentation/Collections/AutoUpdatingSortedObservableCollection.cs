// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SiliconStudio.Presentation.Collections
{
    /// <summary>
    /// An observable collection that automatically sorts inserted items using either the default comparer for their type, or a custom provider comparer.
    /// Insertion and search are both O(log(n)). The method <see cref="SortedObservableCollection{T}.BinarySearch"/> must be used for O(log(n)).
    /// The items must implement <see cref="INotifyPropertyChanging"/> and <see cref="INotifyPropertyChanged"/>.
    /// The collection watches for property changes in its items and reorders them accordingly if the changes affect the order.
    /// </summary>
    /// <typeparam name="T">The type of item this collection contains. Must be a class that implements <see cref="INotifyPropertyChanging"/> and <see cref="INotifyPropertyChanged"/>.</typeparam>
    /// <seealso cref="SortedObservableCollection{T}"/>
    public class AutoUpdatingSortedObservableCollection<T> : SortedObservableCollection<T> where T : class, INotifyPropertyChanging, INotifyPropertyChanged
    {
        protected T ChangingItem;
        protected int ChangingIndex;
        protected T AddedItem;
        protected int AddedIndex;
        private int changeCount;

        /// <summary>
        /// Public constructor. A comparer can be provided to compare items. If null, the default comparer will be used (if available).
        /// If no comparer is provided and no default comparer is available, an <see cref="InvalidOperationException"/> will be thrown when methods requesting comparer are invoked.
        /// </summary>
        /// <param name="comparer">The comparer to use to compare items.</param>
        public AutoUpdatingSortedObservableCollection(IComparer<T> comparer = null)
            : base(comparer)
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{{AutoUpdatingSortedObservableCollection}} Count = {0}", Count);
        }
        
        protected virtual void ItemPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            var item = (T)sender;
            if (ChangingItem != null && !ReferenceEquals(ChangingItem, item))
                throw new InvalidOperationException("Multiple items in the collection are changing concurrently.");

            ++changeCount;

            ChangingItem = item;
            ChangingIndex = GetIndex(item, false);
            AddedItem = null;
        }

        protected virtual void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var item = (T)sender;

            // An object has been added while a property of an existing object has been modified
            if (ChangingItem != null && AddedItem != null)
                throw new InvalidOperationException("PropertyChanged is invoked without PropertyChanging, or multiple items in the collection are changing concurrently.");

            // The object has been added to the collection during the property change, so it was not preregistered by the ItemPropertyChanging event
            if (ChangingItem == null && AddedItem != null)
            {
                ChangingItem = AddedItem;
                ChangingIndex = AddedIndex;
                ++changeCount;
            }

            // No object is changing, or a different object is currently changing
            if (ChangingItem == null || !ReferenceEquals(ChangingItem, item))
            {
                 throw new InvalidOperationException("PropertyChanged is invoked without PropertyChanging, or multiple items in the collection are changing concurrently.");
            }

            --changeCount;
            if (changeCount == 0)
            {
                bool needReorder = (ChangingIndex > 0 && DefaultCompareFunc(Items[ChangingIndex - 1], item) > 0) || (ChangingIndex < Count - 1 && DefaultCompareFunc(item, Items[ChangingIndex + 1]) > 0);
                if (needReorder)
                {
                    int newIndex = GetReorderingIndex(item);
                    if (newIndex != ChangingIndex && newIndex != ChangingIndex + 1)
                    {
                        if (newIndex > ChangingIndex)
                            --newIndex;

                        ObservableCollectionMoveItem(ChangingIndex, newIndex);
                    }
                    else
                    ChangingIndex = GetIndex(item, false);
                }
                ChangingItem = null;
            }
        }

        protected int GetReorderingIndex(T item)
        {
            int imin = 0;
            int imax = Count - 1;
            while (imax >= imin)
            {
                int imid = (imin + imax) / 2;

                int comp = DefaultCompareFunc(this[imid], item);
                if (comp < 0)
                    imin = imid + 1;
                else if (comp > 0)
                    imax = imid - 1;
                else
                {
                    bool found = true;
                    if (imid > 0)
                    {
                        comp = DefaultCompareFunc(this[imid - 1], item);
                        if (comp > 0)
                        {
                            imax = imid - 1;
                            found = false;
                        }
                    }
                    if (imid < Count - 1)
                    {
                        comp = DefaultCompareFunc(this[imid + 1], item);
                        if (comp < 0)
                        {
                            imin = imid + 1;
                            found = false;
                        }
                    }
                    if (found)
                        return imid;
                }
            }

            return imin;
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, T item)
        {
            item.PropertyChanging += ItemPropertyChanging;
            item.PropertyChanged += ItemPropertyChanged;
            base.InsertItem(index, item);
            AddedItem = item;
            AddedIndex = GetIndex(item, false);
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            foreach (var item in Items)
            {
                item.PropertyChanging -= ItemPropertyChanging;
                item.PropertyChanged -= ItemPropertyChanged;
            }
            base.ClearItems();
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            var item = Items[index];
            item.PropertyChanging -= ItemPropertyChanging;
            item.PropertyChanged -= ItemPropertyChanged;
            if (ChangingItem == item)
            {
                ChangingItem = null;
                changeCount = 0;
            }
            base.RemoveItem(index);
        }
    }
}
