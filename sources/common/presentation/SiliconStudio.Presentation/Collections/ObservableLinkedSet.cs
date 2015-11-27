using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace SiliconStudio.Presentation.Collections
{
    public class ObservableLinkedSet<T> : IObservableCollection<T>, IReadOnlyObservableCollection<T>
    {
        private readonly HashSet<T> hashSet;
        private readonly ObservableLinkedList<T> list;

        public ObservableLinkedSet()
        {
            hashSet = new HashSet<T>();
            list = new ObservableLinkedList<T>();
        }

        public ObservableLinkedSet(IEnumerable<T> collection)
        {
            // First try to keep order by filling the list and use it for the hash set
            list = new ObservableLinkedList<T>(collection);
            hashSet = new HashSet<T>(list);
            // If there are duplicated values in the list, we won't be able to keep order
            if (hashSet.Count != list.Count)
            {
                list.Clear();
                list.AddRange(hashSet);
            }
        }


        public T First => list.First;

        public bool IsReadOnly => false;

        public T Last => list.Last;

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { list.CollectionChanged += value; }
            remove { list.CollectionChanged -= value; }
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { list.PropertyChanged += value; }
            remove { list.PropertyChanged -= value; }
        }

        public int Count => list.Count;

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void Add(T item)
        {
            AddLast(item);
        }

        public void AddFirst(T item)
        {
            if (Count > 0 && Equals(item, list.First))
                return;

            if (!hashSet.Add(item))
            {
                list.Remove(item);
            }
            list.AddFirst(item);
        }

        public void AddLast(T item)
        {
            if (Count > 0 && Equals(item, list.Last))
                return;

            if (!hashSet.Add(item))
            {
                list.Remove(item);
            }
            list.AddLast(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            AddRangeLast(items);
        }

        public void AddRangeFirst(IEnumerable<T> items)
        {
            var itemList = items.Where(x => hashSet.Add(x)).ToList();
            if (itemList.Count > 0)
            {
                list.AddRangeFirst(itemList);
            }
        }

        public void AddRangeLast(IEnumerable<T> items)
        {
            var itemList = items.Where(x => hashSet.Add(x)).ToList();
            if (itemList.Count > 0)
            {
                list.AddRangeLast(itemList);
            }
        }

        public void Clear()
        {
            hashSet.Clear();
            list.Clear();
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
            if (!hashSet.Remove(item))
                return false;
            return list.Remove(item);
        }

        public void RemoveFirst()
        {
            var item = list.First;
            hashSet.Remove(item);
            list.RemoveFirst();
        }

        public void RemoveLast()
        {
            var item = list.Last;
            hashSet.Remove(item);
            list.RemoveLast();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{{ObservableLinkedSet}} Count = {Count}";
        }
    }
}