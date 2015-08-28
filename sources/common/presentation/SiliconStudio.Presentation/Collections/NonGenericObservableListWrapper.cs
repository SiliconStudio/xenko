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
    /// <summary>
    /// A class that wraps an instance of the <see cref="ObservableList{T}"/> class and implement the <see cref="IList"/> interface.
    /// In some scenarii, <see cref="IList"/> does not support range changes on the collection (Especially when bound to a ListCollectionView).
    /// This is why the <see cref="ObservableList{T}"/> class does not implement this interface directly. However this wrapper class can be used
    /// when the <see cref="IList"/> interface is required.
    /// </summary>
    /// <typeparam name="T">The type of item contained in the <see cref="ObservableList{T}"/>.</typeparam>
    public class NonGenericObservableListWrapper<T> : IList, IList<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        private readonly ObservableList<T> list;
        private readonly object syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="NonGenericObservableListWrapper{T}"/> class.
        /// </summary>
        /// <param name="list">The <see cref="ObservableList{T}"/> to wrap.</param>
        public NonGenericObservableListWrapper(ObservableList<T> list)
        {
            this.list = list;
            list.PropertyChanged += ListPropertyChanged;
            list.CollectionChanged += ListCollectionChanged;
        }

        /// <inheritdoc/>
        public object this[int index] { get { return list[index]; } set { list[index] = (T)value; } }

        /// <inheritdoc/>
        T IList<T>.this[int index] { get { return list[index]; } set { list[index] = value; } }

        /// <inheritdoc/>
        public bool IsReadOnly { get { return list.IsReadOnly; } }

        /// <inheritdoc/>
        public bool IsFixedSize { get { return false; } }

        /// <inheritdoc/>
        public int Count { get { return list.Count; } }

        /// <inheritdoc/>
        public object SyncRoot { get { return syncRoot; } }

        /// <inheritdoc/>
        public bool IsSynchronized { get { return false; } }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <inheritdoc/>
        public IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /// <inheritdoc/>
        public void CopyTo(Array array, int index)
        {
            list.CopyTo((T[])array, index);
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }
        
        /// <inheritdoc/>
        public int Add(object value)
        {
            list.Add((T)value);
            return list.Count - 1;
        }

        /// <inheritdoc/>
        public void Add(T item)
        {
            list.Add(item);
        }

        public void AddRange(IEnumerable values)
        {
            list.AddRange(values.Cast<T>());
        }
        
        public void AddRange(IEnumerable<T> values)
        {
            list.AddRange(values);
        }
        
        /// <inheritdoc/>
        public bool Contains(object value)
        {
            return list.Contains((T)value);
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            return list.Contains(item);
        }
        
        /// <inheritdoc/>
        public void Clear()
        {
            list.Clear();
        }

        /// <inheritdoc/>
        public int IndexOf(object value)
        {
            return list.IndexOf((T)value);
        }

        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }
        
        /// <inheritdoc/>
        public void Insert(int index, object value)
        {
            list.Insert(index, (T)value);
        }

        /// <inheritdoc/>
        public void Insert(int index, T item)
        {
            list.Insert(index, item);
        }
        
        /// <inheritdoc/>
        public void Remove(object value)
        {
            list.Remove((T)value);
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{{NonGenericObservableListWrapper}} Count = {0}", Count);
        }
        
        private void ListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void ListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}