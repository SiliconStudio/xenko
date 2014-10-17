using System.Threading.Tasks;

using SiliconStudio.BuildEngine.Editor.Model;
using SiliconStudio.Presentation.Core;

namespace SiliconStudio.BuildEngine.Editor.ViewModel
{
    public interface IDirectoryViewModel
    {
        SortedObservableCollection<IDirectoryViewModel> SubDirectories { get; }

        /// <summary>
        /// Name of the directory.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Parent directory, or null if it is a root directory.
        /// </summary>
        IDirectoryViewModel Parent { get; set; }

        /// <summary>
        /// Path of the directory, using aliases if available.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Indicate wheither this directory is expanded in the view.
        /// </summary>
        bool IsExpanded { get; set; }

        IDirectoryViewModel FindSubDirectory(string name);

        Task Refresh(IDataExplorer explorer);
    }
}
