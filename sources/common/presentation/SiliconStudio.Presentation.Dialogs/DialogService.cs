// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using SiliconStudio.Presentation.Resources;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Presentation.Windows;
using MessageBoxButton = SiliconStudio.Presentation.Services.MessageBoxButton;
using MessageBoxImage = SiliconStudio.Presentation.Services.MessageBoxImage;
using MessageBoxResult = SiliconStudio.Presentation.Services.MessageBoxResult;

namespace SiliconStudio.Presentation.Dialogs
{
    public class DialogService : IDialogService
    {
        public DialogService(Dispatcher dispatcher, Window parentWindow)
        {
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));

            Dispatcher = dispatcher;
            ParentWindow = parentWindow;
        }

        public Window ParentWindow { get; set; }

        protected Dispatcher Dispatcher { get; }

        public IFileOpenModalDialog CreateFileOpenModalDialog()
        {
            return new FileOpenModalDialog(Dispatcher, ParentWindow);
        }

        public IFolderOpenModalDialog CreateFolderOpenModalDialog()
        {
            return new FolderOpenModalDialog(Dispatcher, ParentWindow);
        }

        public IFileSaveModalDialog CreateFileSaveModalDialog()
        {
            return new FileSaveModalDialog(Dispatcher, ParentWindow);
        }

        public MessageBoxResult ShowMessageBox(string message, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            var parentWindow = ParentWindow;
            return Dispatcher.Invoke(() => Windows.MessageBox.Show(parentWindow, message, caption, button, image));
        }

        public MessageBoxResult ShowMessageBox(string message, string caption, IEnumerable<DialogButtonInfo> buttons, MessageBoxImage image)
        {
            var parentWindow = ParentWindow;
            return Dispatcher.Invoke(() => Windows.MessageBox.Show(parentWindow, message, caption, buttons, image));
        }

        public MessageBoxResult ShowCheckedMessageBox(string message, string caption, ref bool? isChecked, MessageBoxButton button, MessageBoxImage image)
        {
            return ShowCheckedMessageBox(message, caption, Strings.DontAskMeAgain, ref isChecked, button, image);
        }

        public MessageBoxResult ShowCheckedMessageBox(string message, string caption, string checkedMessage, ref bool? isChecked, MessageBoxButton button, MessageBoxImage image)
        {
            var parentWindow = ParentWindow;
            var localIsChecked = isChecked;
            var result = Dispatcher.Invoke(() =>
                CheckedMessageBox.Show(parentWindow, message, caption, button, image, checkedMessage, ref localIsChecked));
            isChecked = localIsChecked;
            return result;
        }

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
