// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SiliconStudio.Presentation.Services;

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
            DependencyProperty.Register(nameof(CheckedMessage), typeof(string), typeof(CheckedMessageBox));

        /// <summary>
        /// Identifies the <see cref="IsCheckedProperty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(nameof(IsChecked), typeof(bool?), typeof(CheckedMessageBox));

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

        public static Task<CheckedMessageBoxResult> Show(WindowOwner owner, string message, string caption, MessageBoxButton button, MessageBoxImage image, string checkedMessage, bool? isChecked)
        {
            return Show(owner, message, caption, GetButtons(button), image, checkedMessage, isChecked);
        }
        
        public static async Task<CheckedMessageBoxResult> Show(WindowOwner owner, string message, string caption, IEnumerable<DialogButtonInfo> buttons, MessageBoxImage image, string checkedMessage, bool? isChecked)
        {
            var buttonList = buttons.ToList();
            var messageBox = new CheckedMessageBox
            {
                Title = caption,
                Content = message,
                ButtonsSource = buttonList,
                CheckedMessage = checkedMessage,
                IsChecked = isChecked,
            };
            SetImage(messageBox, image);
            SetKeyBindings(messageBox, buttonList);

            var result = (MessageBoxResult)await messageBox.ShowInternal(owner);
            return new CheckedMessageBoxResult(result, messageBox.IsChecked);
        }
    }
}
