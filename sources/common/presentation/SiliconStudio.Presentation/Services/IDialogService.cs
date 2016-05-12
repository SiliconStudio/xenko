// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Presentation.Windows;

namespace SiliconStudio.Presentation.Services
{
    /// <summary>
    /// An interface to invoke dialogs from commands implemented in view models
    /// </summary>
    public interface IDialogService
    {
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
        /// Displays a modal message box and returns a task that completes when the message box is closed.
        /// </summary>
        /// <param name="message">The text to display as message in the message box.</param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="image">The image to display in the message box.</param>
        /// <param name="owner">The intended owner for the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value indicating which button the user pressed to close the window.</returns>
        Task<MessageBoxResult> MessageBox(string message, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal);

        /// <summary>
        /// Displays a modal message box and returns a task that completes when the message box is closed.
        /// </summary>
        /// <param name="message">The text to display as message in the message box.</param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="image">The image to display in the message box.</param>
        /// <param name="owner">The intended owner for the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value indicating which button the user pressed to close the window.</returns>
        Task<MessageBoxResult> MessageBox(string message, IEnumerable<DialogButtonInfo> buttons, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal);

        /// <summary>
        /// Displays a modal message box with an additional checkbox between the message and the buttons,
        /// and returns a task that completes when the message box is closed.
        /// The message displayed in the checkbox is the localized string <see cref="Resources.Strings.DontAskMeAgain"/>.
        /// </summary>
        /// <param name="message">The text to display as message in the message box.</param>
        /// <param name="isChecked">The initial status of the check box.</param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="image">The image to display in the message box.</param>
        /// <param name="owner">The intended owner for the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value indicating which button the user pressed to close the window.</returns>
        Task<CheckedMessageBoxResult> CheckedMessageBox(string message, bool? isChecked, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal);

        /// <summary>
        /// Displays a modal message box with an additional checkbox between the message and the buttons,
        /// and returns a task that completes when the message box is closed.
        /// </summary>
        /// <param name="message">The text to display as message in the message box.</param>
        /// <param name="isChecked">The initial status of the check box.</param>
        /// <param name="checkboxMessage"></param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="image">The image to display in the message box.</param>
        /// <param name="owner">The intended owner for the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value indicating which button the user pressed to close the window.</returns>
        Task<CheckedMessageBoxResult> CheckedMessageBox(string message, bool? isChecked, string checkboxMessage, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal);

        /// <summary>
        /// Displays a modal message box and blocks until the user closed the message box.
        /// </summary>
        /// <param name="message">The text to display as message in the message box.</param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="image">The image to display in the message box.</param>
        /// <param name="owner">The intended owner for the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value indicating which button the user pressed to close the window.</returns>
        MessageBoxResult BlockingMessageBox(string message, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal);

        /// <summary>
        /// Displays a modal message box and blocks until the user closed the message box.
        /// </summary>
        /// <param name="message">The text to display as message in the message box.</param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="image">The image to display in the message box.</param>
        /// <param name="owner">The intended owner for the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value indicating which button the user pressed to close the window.</returns>
        MessageBoxResult BlockingMessageBox(string message, IEnumerable<DialogButtonInfo> buttons, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal);

        /// <summary>
        /// Displays a modal message box with an additional checkbox between the message and the buttons,
        /// and blocks until the user closed the message box.
        /// The message displayed in the checkbox is the localized string <see cref="Resources.Strings.DontAskMeAgain"/>.
        /// </summary>
        /// <param name="message">The text to display as message in the message box.</param>
        /// <param name="isChecked">The initial status of the check box.</param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="image">The image to display in the message box.</param>
        /// <param name="owner">The intended owner for the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value indicating which button the user pressed to close the window.</returns>
        CheckedMessageBoxResult BlockingCheckedMessageBox(string message, bool? isChecked, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal);

        /// <summary>
        /// Displays a modal message box with an additional checkbox between the message and the buttons,
        /// and blocks until the user closed the message box.
        /// </summary>
        /// <param name="message">The text to display as message in the message box.</param>
        /// <param name="isChecked">The initial status of the check box.</param>
        /// <param name="checkboxMessage"></param>
        /// <param name="buttons">The buttons to display in the message box.</param>
        /// <param name="image">The image to display in the message box.</param>
        /// <param name="owner">The intended owner for the message box.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value indicating which button the user pressed to close the window.</returns>
        CheckedMessageBoxResult BlockingCheckedMessageBox(string message, bool? isChecked, string checkboxMessage, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal);

        /// <summary>
        /// Attempts to close the main window of the application.
        /// </summary>
        /// <param name="onClosed">An action to execute if the main window is successfully closed.</param>
        void CloseMainWindow(Action onClosed);
    }
}
