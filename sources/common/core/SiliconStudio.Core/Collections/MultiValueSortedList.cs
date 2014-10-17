// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Core.Collections
{
    /// <summary>
    /// Represents a priority queue, where keys are sorted and each key might have mlutiple values.
    /// </summary>
    /// <remarks>
    /// Storage is based on a <see cref="List{TKey}"/> and a <see cref="List{KeyValuePair{TKey, TValue}}"/>.
    /// </remarks>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class MultiValueSortedList<TKey, TValue> : ILookup<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, ICollection, IEnumerable where TKey : IComparable<TKey>
    {
        private struct Grouping : IGrouping<TKey, TValue>
        {
            public IEnumerator<TValue> GetEnumerator() { return enumerable.GetEnumerator(); }
            IEnumerator IEnumerable.GetEnumerator() { return enumerable.GetEnumerator(); } 
            public TKey Key { get { return key; } }
            public Grouping(TKey key, IEnumerable<TValue> values) { this.key = key; enumerable = values; }
            private readonly TKey key;
            private readonly IEnumerable<TValue> enumerable;
        }

        private struct GroupingEnumerator : IEnumerator<IGrouping<TKey, TValue>>
        {
            public void Dispose() { keys.Dispose(); }
            public bool MoveNext() { return keys.MoveNext(); }
            public void Reset() { keys.Reset(); }
            public IGrouping<TKey, TValue> Current { get { return new Grouping(keys.Current, list[keys.Current]); } }
            object IEnumerator.Current { get { return new Grouping(keys.Current, list[keys.Current]); } }
            public GroupingEnumerator(MultiValueSortedList<TKey, TValue> list) { keys = list.DistinctKeys().GetEnumerator(); this.list = list; }
            readonly IEnumerator<TKey> keys;
            readonly MultiValueSortedList<TKey, TValue> list;
        }

        private readonly List<KeyValuePair<TKey, TValue>> list = new List<KeyValuePair<TKey, TValue>>();
        private readonly List<TKey> keys = new List<TKey>();

        public IEnumerable<TKey> Keys { get { return DistinctKeys(); } }
        public IEnumerable<TValue> Values { get { return list.Select(x => x.Value); } }

        private IEnumerable<TKey> DistinctKeys()
        {
            if (keys.Count == 0)
                return keys;

            bool first = true;
            TKey distinctKey = default(TKey);
            return keys.Where(x =>
                {
                    bool result = first || !distinctKey.Equals(x);
                    distinctKey = x;
                    first = false;
                    return result;
                });
        }

        private int KeyToIndex(object key)
        {
            return keys.BinarySearch((TKey)key);
        }

        public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator()
        {
            return new GroupingEnumerator(this);
        }


        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            int lower = 0;
            int greater = list.Count;
            int current = (lower + greater) >> 1;
            while (greater - lower > 1)
            {
                if (keys[current].CompareTo(item.Key) < 0)
                {
                    lower = current;
                }
                else
                {
                    greater = current;
                }
                current = (lower + greater) >> 1;
            }
            list.Insert(greater, item);
            keys.Insert(greater, item.Key);
        }

        public bool Contains(object key)
        {
            return KeyToIndex(key) >= 0;
        }

        public void Add(object key, object value)
        {
            Add(new KeyValuePair<TKey, TValue>((TKey)key, (TValue)value));
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            list.Clear();
            keys.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return KeyToIndex(item.Key) >= 0;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool removed = false;
            for (int i = KeyToIndex(item.Key); i >= 0; i = KeyToIndex(item.Key))
            {
                list.RemoveAt(i);
                keys.RemoveAt(i);
                removed = true;
            }
            return removed;
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)list).CopyTo(array, index);
        }

        public bool Contains(TKey key)
        {
            return KeyToIndex(key) >= 0;
        }

        public int Count { get { return list.Count; } }

        public IEnumerable<TValue> this[TKey key] { get { return list.Skip(KeyToIndex(key)).TakeWhile(x => x.Key.Equals(key)).Select(x => x.Value); } }

        int ICollection.Count { get { return list.Count; } }

        public object SyncRoot { get { return ((ICollection)list).SyncRoot; } }

        public bool IsSynchronized { get { return ((ICollection)list).IsSynchronized; } }

        int ICollection<KeyValuePair<TKey, TValue>>.Count { get { return list.Count; } }

        public bool IsFixedSize { get { return false; } }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly { get { return false; } }

        public bool ContainsKey(TKey key)
        {
            return KeyToIndex(key) >= 0;
        }

        public void Add(TKey key, TValue value)
        {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool Remove(TKey key)
        {
            bool removed = false;
            for (int i = KeyToIndex(key); i >= 0; i = KeyToIndex(key))
            {
                list.RemoveAt(i);
                keys.RemoveAt(i);
                removed = true;
            }
            return removed;
        }
    }
}
