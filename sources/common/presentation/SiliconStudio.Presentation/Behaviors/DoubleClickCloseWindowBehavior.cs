// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows;
using System.Windows.Controls;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// A behavior that can be attached to a <see cref="Control"/> and will close the window it is contained in when double-clicked.
    /// If you need to execute a command before closing the window, you can use the <see cref="CloseWindowBehavior{T}.Command"/> and <see cref="CloseWindowBehavior{T}.CommandParameter"/> property of this behavior.
    /// </summary>
    public class DoubleClickCloseWindowBehavior : CloseWindowBehavior<Control>
    {
        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDoubleClick += ControlDoubleClicked;
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            AssociatedObject.MouseDoubleClick -= ControlDoubleClicked;
            base.OnDetaching();
        }

        /// <summary>
        /// Raised when the associated button is clicked. Close the containing window
        /// </summary>
        private void ControlDoubleClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}