using System.Collections.Generic;

namespace SiliconStudio.Presentation.Collections
{
    public interface IObservableList<T> : IList<T>, IObservableCollection<T>
    {
        void AddRange(IEnumerable<T> items);
    }
}
