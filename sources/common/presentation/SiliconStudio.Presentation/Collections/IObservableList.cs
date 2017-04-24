// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.Collections
{
    /// <summary>
    /// This interface regroups the <see cref="IList{T}"/> interface and the <see cref="IObservableCollection{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of item contained in the list.</typeparam>
    public interface IObservableList<T> : IList<T>, IObservableCollection<T>
    {
        [CollectionAccess(CollectionAccessType.UpdatedContent)]
        void AddRange([NotNull] IEnumerable<T> items);
    }
}
