// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows;
using System.Windows.Input;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Presentation.Controls;
using SiliconStudio.Presentation.Internal;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// A behavior that can be attached to a <see cref="TextBoxBase"/> and will close the window it is contained in when <see cref="Key.Enter"/> is pressed.
    /// A command can then be executed before closing the window, you can use the <see cref="CloseWindowBehavior{T}.Command"/> and <see cref="CloseWindowBehavior{T}.CommandParameter"/> property of this behavior.
    /// </summary>
    public class TextBoxCloseWindowBehavior : CloseWindowBehavior<TextBoxBase>
    {
        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(TextBoxCloseWindowBehavior));

        /// <summary>
        /// Gets or sets whether this behavior is currently enabled.
        /// </summary>
        public bool IsEnabled { get { return (bool)GetValue(IsEnabledProperty); } set { SetValue(IsEnabledProperty, value.Box()); } }

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyUp += KeyUp;
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            AssociatedObject.KeyUp -= KeyUp;
            base.OnDetaching();
        }
        
        private void KeyUp(object sender, [NotNull] KeyEventArgs e)
        {
            if (e.Key != Key.Enter || !IsEnabled || AssociatedObject.HasChangesToValidate)
            {
                return;
            }

            Close();
        }
    }
}
