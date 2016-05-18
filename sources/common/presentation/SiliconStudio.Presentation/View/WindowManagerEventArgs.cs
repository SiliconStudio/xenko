using System;
using SiliconStudio.Presentation.Windows;

namespace SiliconStudio.Presentation.View
{
    /// <summary>
    /// Arguments for events raised by the <see cref="WindowManager"/> class.
    /// </summary>
    public class WindowManagerEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowManagerEventArgs"/> class.
        /// </summary>
        /// <param name="window">The info of the window related to this event.</param>
        internal WindowManagerEventArgs(WindowInfo window)
        {
            Window = window;
        }

        /// <summary>
        /// Gets the info of the window related to this event.
        /// </summary>
        public WindowInfo Window { get; }
    }
}
