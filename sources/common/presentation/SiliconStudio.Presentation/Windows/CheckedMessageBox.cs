// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SiliconStudio.Presentation.Windows
{
    using MessageBoxButton = Services.MessageBoxButton;
    using MessageBoxImage = Services.MessageBoxImage;
    using MessageBoxResult = Services.MessageBoxResult;

    public class CheckedMessageBox : MessageBox
    {
        /// <summary>
        /// Identifies the <see cref="CheckedMessage"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CheckedMessageProperty =
            DependencyProperty.Register("CheckedMessage", typeof(string), typeof(CheckedMessageBox));

        /// <summary>
        /// Identifies the <see cref="IsCheckedProperty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool?), typeof(CheckedMessageBox));

        public string CheckedMessage
        {
            get { return (string)GetValue(CheckedMessageProperty); }
            set { SetValue(CheckedMessageProperty, value); }
        }

        public bool? IsChecked
        {
            get { return (bool?)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static MessageBoxResult Show(Window owner, string message, string caption, MessageBoxButton button, MessageBoxImage image, string checkedMessage, ref bool? isChecked)
        {
            return Show(owner, message, caption, GetButtons(button), image, checkedMessage, ref isChecked);
        }
        
        public static MessageBoxResult Show(Window owner, string message, string caption, IEnumerable<DialogButtonInfo> buttons, MessageBoxImage image, string checkedMessage, ref bool? isChecked)
        {
            var messageBox = new CheckedMessageBox
            {
                Owner = owner,
                WindowStartupLocation = owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen,
                Title = caption,
                Content = message,
                ButtonsSource = buttons.ToList(),
                CheckedMessage = checkedMessage,
                IsChecked = isChecked,
            };
            SetImage(messageBox, image);

            var result = (MessageBoxResult)messageBox.ShowInternal();
            isChecked = messageBox.IsChecked;
            return result;
        }
    }
}
