// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;

namespace SiliconStudio.Presentation.Collections
{
    /// <summary>
    /// This interface regroups the <see cref="IReadOnlyList{T}"/> interface, the
    /// <see cref="System.ComponentModel.INotifyPropertyChanged"/> interface, and the <see cref="System.Collections.Specialized.INotifyCollectionChanged"/>
    /// interface. It has no additional members.
    /// </summary>
    /// <typeparam name="T">The type of item contained in the collection.</typeparam>
    public interface IReadOnlyObservableList<out T> : IReadOnlyList<T>, IReadOnlyObservableCollection<T>
    {
    }
}
