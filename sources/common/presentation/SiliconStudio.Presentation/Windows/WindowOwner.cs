namespace SiliconStudio.Presentation.Windows
{
    /// <summary>
    /// An enum representing the intended owner for a window to show.
    /// </summary>
    public enum WindowOwner
    {
        /// <summary>
        /// The owner of the window should be the last opened modal window.
        /// </summary>
        LastModal,
        /// <summary>
        /// The owner of the window should be the main window.
        /// </summary>
        MainWindow
    };
}
