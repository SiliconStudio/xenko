using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace SiliconStudio.Presentation.Collections
{
    public class ObservableSet<T> : IObservableList<T>, IReadOnlyObservableList<T>
    {
        private readonly HashSet<T> hashSet;
        private readonly List<T> list;

        public ObservableSet()
            : this(EqualityComparer<T>.Default)
        {
        }

        public ObservableSet(IEnumerable<T> collection)
              : this(EqualityComparer<T>.Default, collection)
        {
        }

        public ObservableSet(IEqualityComparer<T> comparer)
        {
            hashSet = new HashSet<T>(comparer);
            list = new List<T>();
        }

        public ObservableSet(IEqualityComparer<T> comparer, IEnumerable<T> collection)
        {
            list = new List<T>();
            hashSet = new HashSet<T>(comparer);
            foreach (var item in collection)
            {
                if (hashSet.Add(item))
                    list.Add(item);
            }
        }

        public ObservableSet(int capacity)
        {
            hashSet = new HashSet<T>();
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
                hashSet.Remove(oldItem);
                if (!hashSet.Add(value)) throw new InvalidOperationException("Unable to set this value at the given index because this value is already contained in this ObservableSet.");
                list[index] = value;
                var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index);
                OnCollectionChanged(arg);
            }
        }

        public bool IsReadOnly => false;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Count => list.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public IList ToIList()
        {
            return new NonGenericObservableSetWrapper<T>(this);
        }

        public void Add(T item)
        {
            if (hashSet.Add(item))
            {
                list.Add(item);
                var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, list.Count - 1);
                OnCollectionChanged(arg);
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            var itemList = items.Where(x => hashSet.Add(x)).ToList();
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
            hashSet.Clear();
            list.Clear();
            if (raiseEvent)
            {
                var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                OnCollectionChanged(arg);
            }
        }

        public bool Contains(T item)
        {
            return hashSet.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (!hashSet.Contains(item))
                return false;
            int index = list.IndexOf(item);
            if (index != -1)
            {
                RemoveAt(index);
            }
            return index != -1;
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (hashSet.Add(item))
            {
                list.Insert(index, item);
                var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
                OnCollectionChanged(arg);
            }
        }

        public void RemoveAt(int index)
        {
            var item = list[index];
            list.RemoveAt(index);
            hashSet.Remove(item);

            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
            OnCollectionChanged(arg);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{{ObservableSet}} Count = {Count}";
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
