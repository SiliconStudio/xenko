// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.Windows;
using MessageBoxButton = SiliconStudio.Presentation.Services.MessageBoxButton;
using MessageBoxImage = SiliconStudio.Presentation.Services.MessageBoxImage;
using MessageBoxResult = SiliconStudio.Presentation.Services.MessageBoxResult;

namespace SiliconStudio.Presentation.Dialogs
{
    public class DialogService : IDialogService
    {
        private Action onClosedAction;

        public DialogService(IDispatcherService dispatcher, string applicationName)
        {
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));

            Dispatcher = dispatcher;
            ApplicationName = applicationName;
        }

        public string ApplicationName { get; }

        protected IDispatcherService Dispatcher { get; }

        public IFileOpenModalDialog CreateFileOpenModalDialog()
        {
            return new FileOpenModalDialog(Dispatcher);
        }

        public IFolderOpenModalDialog CreateFolderOpenModalDialog()
        {
            return new FolderOpenModalDialog(Dispatcher);
        }

        public IFileSaveModalDialog CreateFileSaveModalDialog()
        {
            return new FileSaveModalDialog(Dispatcher);
        }

        public Task<MessageBoxResult> MessageBox(string message, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.MessageBox(Dispatcher, message, ApplicationName, buttons, image, owner);
        }

        public Task<MessageBoxResult> MessageBox(string message, IEnumerable<DialogButtonInfo> buttons, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.MessageBox(Dispatcher, message, ApplicationName, buttons, image, owner);
        }

        public Task<CheckedMessageBoxResult> CheckedMessageBox(string message, bool? isChecked, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.CheckedMessageBox(Dispatcher, message, ApplicationName, isChecked, button, image, owner);
        }

        public Task<CheckedMessageBoxResult> CheckedMessageBox(string message, bool? isChecked, string checkboxMessage, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.CheckedMessageBox(Dispatcher, message, ApplicationName, isChecked, checkboxMessage, button, image, owner);
        }

        public MessageBoxResult BlockingMessageBox(string message, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.BlockingMessageBox(Dispatcher, message, ApplicationName, buttons, image, owner);
        }

        public MessageBoxResult BlockingMessageBox(string message, IEnumerable<DialogButtonInfo> buttons, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.BlockingMessageBox(Dispatcher, message, ApplicationName, buttons, image, owner);
        }

        public CheckedMessageBoxResult BlockingCheckedMessageBox(string message, bool? isChecked, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.BlockingCheckedMessageBox(Dispatcher, message, ApplicationName, isChecked, button, image, owner);
        }

        public CheckedMessageBoxResult BlockingCheckedMessageBox(string message, bool? isChecked, string checkboxMessage, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.BlockingCheckedMessageBox(Dispatcher, message, ApplicationName, isChecked, checkboxMessage, button, image, owner);
        }

        public void CloseMainWindow(Action onClosed)
        {
            var window = Application.Current.MainWindow;
            if (window != null)
            {
                onClosedAction = onClosed;
                window.Closed -= MainWindowClosed;
                window.Closed += MainWindowClosed;
                window.Close();
            }
        }

        private void MainWindowClosed(object sender, EventArgs e)
        {
            onClosedAction?.Invoke();
        }
    }
}
