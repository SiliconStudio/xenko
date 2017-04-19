using System.Threading.Tasks;

namespace SiliconStudio.Presentation.Windows
{
    /// <summary>
    /// Represents a window that can asynchronously close and/or cancel a request to close.
    /// </summary>
    public interface IAsyncClosableWindow
    {
        /// <summary>
        /// Tries to close the window.
        /// </summary>
        /// <returns>A task that completes either when the window is closed, or when the request to close has been cancelled. The result of the task indicates if the window has been closed.</returns>
        Task<bool> TryClose();
    }
}