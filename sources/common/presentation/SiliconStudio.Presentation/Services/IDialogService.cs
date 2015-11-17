// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Windows;

namespace SiliconStudio.Presentation.Services
{
    /// <summary>
    /// An interface to invoke dialogs from commands implemented in view models
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Gets the parent window handle.
        /// </summary>
        /// <value>The parent window.</value>
        Window ParentWindow { get; }

        /// <summary>
        /// Creates a modal file open dialog.
        /// </summary>
        /// <returns>An instance of <see cref="IFileOpenModalDialog"/>.</returns>
        IFileOpenModalDialog CreateFileOpenModalDialog();

        /// <summary>
        /// Create a modal folder open dialog.
        /// </summary>
        /// <returns>An instance of <see cref="IFolderOpenModalDialog"/>.</returns>
        IFolderOpenModalDialog CreateFolderOpenModalDialog();

        /// <summary>
        /// Creates a modal file save dialog.
        /// </summary>
        /// <returns>An instance of <see cref="IFileSaveModalDialog"/>.</returns>
        IFileSaveModalDialog CreateFileSaveModalDialog();

        /// <summary>
        /// Displays a modal message box.
        /// </summary>
        /// <param name="message">The text to display as message in the message box.</param>
        /// <param name="caption">The title of the message box</param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="image">The image to display in the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value indicating which button the user pressed to close the window.</returns>
        MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None);

        /// <summary>
        /// Displays a modal message box with an additional checkbox between the message and the buttons.
        /// </summary>
        /// <param name="message">The text to display as message in the message box.</param>
        /// <param name="caption">The title of the message box</param>
        /// <param name="checkedMessage"></param>
        /// <param name="isChecked"></param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="image">The image to display in the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value indicating which button the user pressed to close the window.</returns>
        MessageBoxResult ShowCheckedMessageBox(string message, string caption, string checkedMessage, ref bool? isChecked, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None);

        /// <summary>
        /// Attempts to close the current window.
        /// </summary>
        /// <param name="dialogResult">a nullable boolean indicating, if the current window behave like a dialog window, the result of the dialog invocation.</param>
        void CloseCurrentWindow(bool? dialogResult = null);
    }
}
