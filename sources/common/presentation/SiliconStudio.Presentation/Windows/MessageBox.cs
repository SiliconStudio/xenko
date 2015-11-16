// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SiliconStudio.Presentation.Windows
{
    using MessageBoxButton = Services.MessageBoxButton;
    using MessageBoxImage = Services.MessageBoxImage;
    using MessageBoxResult = Services.MessageBoxResult;
    
    public class MessageBox : MessageDialogBase
    {
        private MessageBox()
        {
        }

        /// <summary>
        /// Identifies the <see cref="Image"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(MessageBox));

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        /// <summary>
        /// Gets a new instance of <see cref="DialogButtonInfo"/> to serve as 'Cancel' button.
        /// </summary>
        /// <remarks>
        /// <see cref="DialogButtonInfo.IsCancel"/> is set to <see langword="true"/>.
        /// <see cref="DialogButtonInfo.Result"/> is set to <see cref="MessageBoxResult.Cancel"/>.</remarks>
        public static DialogButtonInfo ButtonCancel => new DialogButtonInfo
        {
            IsCancel = true,
            Result = (int)MessageBoxResult.Cancel,
            Content = "Cancel",
        };

        /// <summary>
        /// Gets a new instance of <see cref="DialogButtonInfo"/> to serve as 'No' button.
        /// </summary>
        /// <remarks>
        /// <see cref="DialogButtonInfo.Result"/> is set to <see cref="MessageBoxResult.No"/>.</remarks>
        public static DialogButtonInfo ButtonNo => new DialogButtonInfo
        {
            Result = (int)MessageBoxResult.No,
            Content = "No",
        };

        /// <summary>
        /// Gets a new instance of <see cref="DialogButtonInfo"/> to serve as 'OK' button.
        /// </summary>
        /// <remarks>
        /// <see cref="DialogButtonInfo.IsDefault"/> is set to <see langword="true"/>.
        /// <see cref="DialogButtonInfo.Result"/> is set to <see cref="MessageBoxResult.OK"/>.</remarks>
        public static DialogButtonInfo ButtonOK => new DialogButtonInfo
        {
            IsDefault = true,
            Result = (int)MessageBoxResult.OK,
            Content = "OK",
        };

        /// <summary>
        /// Gets a new instance of <see cref="DialogButtonInfo"/> to serve as 'Yes' button.
        /// </summary>
        /// <remarks>
        /// <see cref="DialogButtonInfo.IsDefault"/> is set to <see langword="true"/>.
        /// <see cref="DialogButtonInfo.Result"/> is set to <see cref="MessageBoxResult.Yes"/>.</remarks>
        public static DialogButtonInfo ButtonYes => new DialogButtonInfo
        {
            IsDefault = true,
            Result = (int)MessageBoxResult.Yes,
            Content = "Yes",
        };

        public static ICollection<DialogButtonInfo> GetButtons(MessageBoxButton button)
        {
            ICollection<DialogButtonInfo> buttons;
            switch (button)
            {
                case MessageBoxButton.OK:
                    buttons = new[] { ButtonOK };
                    break;

                case MessageBoxButton.OKCancel:
                    buttons = new[] { ButtonOK, ButtonCancel };
                    break;

                case MessageBoxButton.YesNoCancel:
                    buttons = new[] { ButtonYes, ButtonNo, ButtonCancel };
                    break;

                case MessageBoxButton.YesNo:
                    var buttonNo = ButtonNo;
                    buttonNo.IsCancel = true;
                    buttons = new[] { ButtonYes, buttonNo };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
            return buttons;
        }

        public static void SetImage(MessageBox messageBox, MessageBoxImage image)
        {
            string imageKey;
            switch (image)
            {
                case MessageBoxImage.None:
                    imageKey = null;
                    break;

                case MessageBoxImage.Error:
                    imageKey = "ImageErrorDialog";
                    break;

                case MessageBoxImage.Question:
                    imageKey = "ImageQuestionDialog";
                    break;

                case MessageBoxImage.Warning:
                    imageKey = "ImageWarningDialog";
                    break;

                case MessageBoxImage.Information:
                    imageKey = "ImageInformationDialog";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(image), image, null);
            }
            messageBox.Image = imageKey != null ? (ImageSource)messageBox.TryFindResource(imageKey) : null;
        }

        /// <summary>
        /// Displays a <see cref="MessageBox"/> an returns the <see cref="MessageBoxResult"/> depending on the user's choice.
        /// </summary>
        /// <param name="owner">A <see cref="Window"/> that represents the owner window of the message box.</param>
        /// <param name="message">A <see cref="string"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="string"/> that specifies the title bar caption to display.</param>
        /// <param name="button">A <see cref="MessageBoxButton"/> value that specifies which button or buttons to display</param>
        /// <param name="image">A <see cref="MessageBoxImage"/> value that specifies the icon to display.</param>
        /// <returns>A <see cref="MessageBoxResult"/> value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(Window owner, string message, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            var buttons = GetButtons(button);
            var messageBox = new MessageBox
            {
                Owner = owner,
                WindowStartupLocation = owner != null ? WindowStartupLocation.CenterOwner : WindowStartupLocation.CenterScreen,
                Title = caption,
                Content = message,
                ButtonsSource = buttons,
            };
            SetImage(messageBox, image);
            return (MessageBoxResult)messageBox.ShowInternal();
        }
    }
}
