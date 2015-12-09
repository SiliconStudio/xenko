using System.Collections.Generic;

namespace SiliconStudio.Presentation.Collections
{
    /// <summary>
    /// This interface regroups the <see cref="IList{T}"/> interface and the <see cref="IObservableCollection{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of item contained in the list.</typeparam>
    public interface IObservableList<T> : IList<T>, IObservableCollection<T>
    {
        void AddRange(IEnumerable<T> items);
    }
}
