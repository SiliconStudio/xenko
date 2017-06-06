// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Threading;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Translation;

namespace SiliconStudio.Presentation.Windows
{
    public static class DialogHelper
    {
        [NotNull]
        public static Task<MessageBoxResult> MessageBox([NotNull] IDispatcherService dispatcher, string message, string caption, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            return dispatcher.InvokeTask(() => Windows.MessageBox.Show(message, caption, button, image));
        }

        [NotNull]
        public static Task<int> MessageBox([NotNull] IDispatcherService dispatcher, string message, string caption, IEnumerable<DialogButtonInfo> buttons, MessageBoxImage image = MessageBoxImage.None)
        {
            return dispatcher.InvokeTask(() => Windows.MessageBox.Show(message, caption, buttons, image));
        }

        [NotNull]
        public static Task<CheckedMessageBoxResult> CheckedMessageBox([NotNull] IDispatcherService dispatcher, string message, string caption, bool? isChecked, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            return dispatcher.InvokeTask(() => Windows.CheckedMessageBox.Show(message, caption, button, image, TranslationManager.Instance.GetString("Don't ask me again"), isChecked));
        }

        [NotNull]
        public static Task<CheckedMessageBoxResult> CheckedMessageBox([NotNull] IDispatcherService dispatcher, string message, string caption, bool? isChecked, string checkboxMessage, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            return dispatcher.InvokeTask(() => Windows.CheckedMessageBox.Show(message, caption, button, image, checkboxMessage, isChecked));
        }

        [NotNull]
        public static Task<CheckedMessageBoxResult> CheckedMessageBox([NotNull] IDispatcherService dispatcher, string message, string caption, bool? isChecked, string checkboxMessage, IEnumerable<DialogButtonInfo> buttons, MessageBoxImage image = MessageBoxImage.None)
        {
            return dispatcher.InvokeTask(() => Windows.CheckedMessageBox.Show(message, caption, buttons, image, checkboxMessage, isChecked));
        }

        public static MessageBoxResult BlockingMessageBox([NotNull] IDispatcherService dispatcher, string message, string caption, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            return PushFrame(dispatcher, () => MessageBox(dispatcher, message, caption, button, image));
        }

        public static int BlockingMessageBox([NotNull] IDispatcherService dispatcher, string message, string caption, IEnumerable<DialogButtonInfo> buttons, MessageBoxImage image = MessageBoxImage.None)
        {
            return PushFrame(dispatcher, () => MessageBox(dispatcher, message, caption, buttons, image));
        }

        public static CheckedMessageBoxResult BlockingCheckedMessageBox([NotNull] IDispatcherService dispatcher, string message, string caption, bool? isChecked, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            return PushFrame(dispatcher, () => CheckedMessageBox(dispatcher, message, caption, isChecked, button, image));
        }

        public static CheckedMessageBoxResult BlockingCheckedMessageBox([NotNull] IDispatcherService dispatcher, string message, string caption, bool? isChecked, string checkboxMessage, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.None)
        {
            return PushFrame(dispatcher, () => CheckedMessageBox(dispatcher, message, caption, isChecked, checkboxMessage, button, image));
        }

        public static CheckedMessageBoxResult BlockingCheckedMessageBox([NotNull] IDispatcherService dispatcher, string message, string caption, bool? isChecked, string checkboxMessage, IEnumerable<DialogButtonInfo> buttons, MessageBoxImage image = MessageBoxImage.None)
        {
            return PushFrame(dispatcher, () => CheckedMessageBox(dispatcher, message, caption, isChecked, checkboxMessage, buttons, image));
        }

        private static TResult PushFrame<TResult>([NotNull] IDispatcherService dispatcher, Func<Task<TResult>> task)
        {
            return dispatcher.Invoke(() =>
            {
                var frame = new DispatcherFrame();
                var frameTask = task().ContinueWith(x => { frame.Continue = false; return x.Result; });
                ComponentDispatcher.PushModal();
                Dispatcher.PushFrame(frame);
                ComponentDispatcher.PopModal();
                return frameTask.Result;
            });
        }
    }
}
