// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows;
using SiliconStudio.Presentation.Controls;
using SiliconStudio.Presentation.Core;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// A behavior that can be attached to a <see cref="TextBoxBase"/> and will close the window it is contained in on <see cref="TextBoxBase"/> <see cref="TextBoxBase.Validated"/> event.
    /// A command can then be executed before closing the window, you can use the <see cref="CloseWindowBehavior{T}.Command"/> and <see cref="CloseWindowBehavior{T}.CommandParameter"/> property of this behavior.
    /// </summary>
    public class TextBoxCloseWindowBehavior : CloseWindowBehavior<TextBoxBase>
    {
        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(TextBoxCloseWindowBehavior));

        /// <summary>
        /// Gets or sets whether this behavior is currently enabled.
        /// </summary>
        public bool IsEnabled { get { return (bool)GetValue(IsEnabledProperty); } set { SetValue(IsEnabledProperty, value); } }

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Validated += Validated;
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            AssociatedObject.Validated -= Validated;
            base.OnDetaching();
        }

        /// <summary>
        /// Raised when the associated <see cref="TextBoxBase"/> is validated. Close the containing window
        /// </summary>
        private void Validated(object sender, ValidationRoutedEventArgs<string> e)
        {
            if (!IsEnabled)
                return;

            Close();
        }
    }
}