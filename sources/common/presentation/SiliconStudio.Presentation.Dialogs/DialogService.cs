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
        private readonly IDispatcherService dispatcher;

        public DialogService(IDispatcherService dispatcher, string applicationName, Window parentWindow)
        {
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
            this.dispatcher = dispatcher;
            ApplicationName = applicationName;
            ParentWindow = parentWindow;
        }

        public string ApplicationName { get; set; }

        [Obsolete]
        public Window ParentWindow { get; set; }

        public IFileOpenModalDialog CreateFileOpenModalDialog()
        {
            return new FileOpenModalDialog(dispatcher, ParentWindow);
        }

        public IFolderOpenModalDialog CreateFolderOpenModalDialog()
        {
            return new FolderOpenModalDialog(dispatcher, ParentWindow);
        }

        public IFileSaveModalDialog CreateFileSaveModalDialog()
        {
            return new FileSaveModalDialog(dispatcher, ParentWindow);
        }

        public Task<MessageBoxResult> MessageBox(string message, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.MessageBox(dispatcher, message, ApplicationName, buttons, image, owner);
        }

        public Task<MessageBoxResult> MessageBox(string message, IEnumerable<DialogButtonInfo> buttons, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.MessageBox(dispatcher, message, ApplicationName, buttons, image, owner);
        }

        public Task<CheckedMessageBoxResult> CheckedMessageBox(string message, bool? isChecked, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.CheckedMessageBox(dispatcher, message, ApplicationName, isChecked, button, image, owner);
        }

        public Task<CheckedMessageBoxResult> CheckedMessageBox(string message, bool? isChecked, string checkboxMessage, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.CheckedMessageBox(dispatcher, message, ApplicationName, isChecked, checkboxMessage, button, image, owner);
        }

        public MessageBoxResult MessageBoxSync(string message, MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.MessageBoxSync(dispatcher, message, ApplicationName, buttons, image, owner);
        }

        public MessageBoxResult MessageBoxSync(string message, IEnumerable<DialogButtonInfo> buttons, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.MessageBoxSync(dispatcher, message, ApplicationName, buttons, image, owner);
        }

        public CheckedMessageBoxResult CheckedMessageBoxSync(string message, bool? isChecked, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.CheckedMessageBoxSync(dispatcher, message, ApplicationName, isChecked, button, image, owner);
        }

        public CheckedMessageBoxResult CheckedMessageBoxSync(string message, bool? isChecked, string checkboxMessage, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None, WindowOwner owner = WindowOwner.LastModal)
        {
            return DialogHelper.CheckedMessageBoxSync(dispatcher, message, ApplicationName, isChecked, checkboxMessage, button, image, owner);
        }
        
        [Obsolete("This method will be removed soon")]
        public void CloseCurrentWindow(bool? dialogResult = null)
        {
            // Window.DialogResult setter will throw an exception when the window was not displayed with ShowDialog, even if we're setting null.
            if (ParentWindow.DialogResult != dialogResult)
            {
                ParentWindow.DialogResult = dialogResult;
            }
            ParentWindow.Close();
            if (!ParentWindow.IsLoaded)
            {
                ParentWindow = null;
            }
        }
    }
}
