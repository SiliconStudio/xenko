using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine.Editor.Model
{
    public interface IDataExplorer
    {
        /// <summary>
        /// Enumerate all directories of the given path
        /// </summary>
        /// <param name="absolutePath">The base path to enumerate directories.</param>
        /// <returns>An enumerable of the directories directly contained in the <see cref="absolutePath"/></returns>
        Task<IEnumerable<string>> EnumerateDirectories(string absolutePath);

        /// <summary>
        /// Enumerate the data items contained in the given path
        /// </summary>
        /// <param name="absolutePath">The base path to enumerate data items</param>
        /// <param name="recursive">If true, will also list the items in sub-directories</param>
        /// <returns>An enumerable of the data items directly contained in the <see cref="absolutePath"/></returns>
        Task<IEnumerable<string>> EnumerateData(string absolutePath, bool recursive);

        /// <summary>
        /// Attempt to create a directory with the given path
        /// </summary>
        /// <param name="absolutePath">The path of the directory to create</param>
        /// <returns>true if the directory could be created, false otherwise</returns>
        Task<bool> CreateDirectory(string absolutePath);
    }
}
