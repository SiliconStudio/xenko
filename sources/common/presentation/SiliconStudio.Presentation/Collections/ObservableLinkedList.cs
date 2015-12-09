// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace SiliconStudio.Presentation.Collections
{
    public class ObservableLinkedList<T> : IObservableCollection<T>, IReadOnlyObservableCollection<T>
    {
        private readonly LinkedList<T> list;

        public ObservableLinkedList()
        {
            list = new LinkedList<T>();
        }

        public ObservableLinkedList(IEnumerable<T> collection)
        {
            list = new LinkedList<T>(collection);
        }

        public int Count => list.Count;

        public T First => list.Count > 0 ? list.First.Value : default(T);

        public bool IsReadOnly => false;

        public T Last => list.Count > 0 ? list.Last.Value : default(T);

        public event PropertyChangedEventHandler PropertyChanged;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T value)
        {
            AddLast(value);
        }

        public void AddFirst(T value)
        {
            list.AddFirst(value);
            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, 0);
            OnCollectionChanged(arg);
        }

        public void AddLast(T value)
        {
            list.AddLast(value);
            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, Count - 1);
            OnCollectionChanged(arg);
        }

        public void AddRange(IEnumerable<T> values)
        {
            AddRangeLast(values);
        }

        public void AddRangeFirst(IEnumerable<T> values)
        {
            var itemList = values.Reverse().ToList();
            if (itemList.Count > 0)
            {
                foreach (var item in itemList)
                {
                    list.AddFirst(item);
                }

                var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, itemList, Count - itemList.Count);
                OnCollectionChanged(arg);
            }
        }

        public void AddRangeLast(IEnumerable<T> values)
        {
            var itemList = values.ToList();
            if (itemList.Count > 0)
            {
                foreach (var item in itemList)
                {
                    list.AddLast(item);
                }

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

        public int IndexOf(T item)
        {
            var index = 0;
            foreach (var e in this)
            {
                if (EqualityComparer<T>.Default.Equals(item, e)) return index;
                index++;
            }
            return -1;
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            var success = list.Remove(item);
            if (success)
            {
                var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
                OnCollectionChanged(arg);
            }
            return success;
        }

        public void RemoveFirst()
        {
            var item = list.First;
            list.RemoveFirst();
            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, 0);
            OnCollectionChanged(arg);
        }

        public void RemoveLast()
        {
            var item = list.Last;
            list.RemoveFirst();
            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, Count);
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
            return $"{{ObservableLinkedList}} Count = {Count}";
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
