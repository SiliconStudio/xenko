using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace SiliconStudio.Presentation.Collections
{
    /// <summary>
    /// This interface regroups the <see cref="IReadOnlyList{T}"/> interface, the
    /// <see cref="INotifyPropertyChanged"/> interface, and the <see cref="INotifyCollectionChanged"/>
    /// interface. It has no additional members.
    /// </summary>
    /// <typeparam name="T">The type of item contained in the collection.</typeparam>
    public interface IReadOnlyObservableList<T> : IReadOnlyList<T>, IReadOnlyObservableCollection<T>
    {
        /// <summary>
        /// Determines the index of a specific item in the <see cref="IReadOnlyObservableList{T}"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="IReadOnlyObservableList{T}"/>.</param>
        /// <returns>The index of item if found in the list; otherwise, -1.</returns>
        int IndexOf(T item);
    }
}