using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace System.Windows.Core
{
    public class ObservableList<T> : IList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged
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

        public void CopyTo(Array array, int index)
        {
            ((IList)list).CopyTo(array, index);
        }

        public int Count { get { return list.Count; } }

        public object SyncRoot { get { return ((IList)list).SyncRoot; } }

        public bool IsSynchronized { get { return ((IList)list).IsSynchronized; } }

        public bool IsReadOnly { get { return ((IList)list).IsReadOnly; } }

        public bool IsFixedSize { get { return ((IList)list).IsFixedSize; } }

        object IList.this[int index] { get { return this[index]; } set { this[index] = (T)value; } }
        
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
            list.Clear();
            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(arg);
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
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
            return string.Format("{{ObservableList}} Count = {0}", Count);
        }

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs arg)
        {
            var handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, arg);
            }

            switch (arg.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                    break;
            }
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs arg)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, arg);
            }
        }

        int IList.Add(object value)
        {
            Add((T)value);
            return list.Count - 1;
        }

        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        void IList.Remove(object value)
        {
            Remove((T)value);
        }
    }
}