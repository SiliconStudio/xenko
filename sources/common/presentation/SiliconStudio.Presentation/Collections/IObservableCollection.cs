// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
