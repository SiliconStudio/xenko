// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SiliconStudio.Presentation.Collections
{
    /// <summary>
    /// This interface regroups the <see cref="ICollection{T}"/> interface, the
    /// <see cref="INotifyPropertyChanged"/> interface, and the <see cref="INotifyCollectionChanged"/>
    /// interface. It has no additional members.
    /// </summary>
    /// <typeparam name="T">The type of item contained in the collection.</typeparam>
    public interface IObservableCollection<T> : ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
    }
}