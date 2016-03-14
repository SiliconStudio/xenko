// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace SiliconStudio.Presentation.Collections
{
    public class ObservableList<T> : IObservableList<T>, IReadOnlyObservableList<T>
    {
        private readonly List<T> list;

        public ObservableList()
        {
            list = new List<T>();
        }

        public ObservableList(IEnumerable<T> collection)
        {
            list = new List<T>(collection);
        }

        public ObservableList(int capacity)
        {
            list = new List<T>(capacity);
        }

        public T this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                var oldItem = list[index];
                list[index] = value;
                var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index);
                OnCollectionChanged(arg);
            }
        }

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public event PropertyChangedEventHandler PropertyChanged;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IList ToIList()
        {
            return new NonGenericObservableListWrapper<T>(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            Insert(Count, item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            var itemList = items.ToList();
            if (itemList.Count > 0)
            {
                list.AddRange(itemList);

                var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemList, Count - itemList.Count);
                OnCollectionChanged(arg);
            }
        }

        public void Clear()
        {
            var raiseEvent = list.Count > 0;
            list.Clear();
            if (raiseEvent)
            {
                var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                OnCollectionChanged(arg);
            }
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public int FindIndex(Predicate<T> match)
        {
            return list.FindIndex(match);
        }

        public bool Remove(T item)
        {
            int index = list.IndexOf(item);
            if (index != -1)
            {
                RemoveAt(index);
            }
            return index != -1;
        }

        public void RemoveRange(int index, int count)
        {
            var oldItems = list.Skip(index).Take(count).ToList();
            list.RemoveRange(index, count);
            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItems, index);
            OnCollectionChanged(arg);
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            list.Insert(index, item);
            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
            OnCollectionChanged(arg);
        }

        public void RemoveAt(int index)
        {
            var item = list[index];
            list.RemoveAt(index);
            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
            OnCollectionChanged(arg);
        }

        public void Reset()
        {
            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(arg);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{{ObservableList}} Count = {Count}";
        }

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs arg)
        {
            CollectionChanged?.Invoke(this, arg);

            switch (arg.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                    break;
            }
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs arg)
        {
            PropertyChanged?.Invoke(this, arg);
        }
    }
}
