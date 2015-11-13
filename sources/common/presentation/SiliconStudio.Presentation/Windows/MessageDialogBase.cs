// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SiliconStudio.Presentation.Windows
{
    /// <summary>
    /// Base class for message-based dialog windows.
    /// </summary>
    public abstract class MessageDialogBase : Window
    {
        private int result;

        protected MessageDialogBase()
        {
            this.ButtonCommand = new MessageDialogCommand<int>(ButtonClick);
        }

        /// <summary>
        /// Identifies the <see cref="ButtonsSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ButtonsSourceProperty =
            DependencyProperty.Register("ButtonsSource", typeof(IEnumerable<DialogButtonInfo>), typeof(MessageDialogBase));

        /// <summary>
        /// Identifies the <see cref="MessageTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MessageTemplateProperty =
            DependencyProperty.Register("MessageTemplate", typeof(DataTemplate), typeof(MessageDialogBase));

        /// <summary>
        /// Identifies the <see cref="MessageTemplateSelector"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MessageTemplateSelectorProperty =
            DependencyProperty.Register("MessageTemplateSelector", typeof(DataTemplateSelector), typeof(MessageDialogBase));

        /// <summary>
        /// Identifies the <see cref="Result"/> dependency property.
        /// </summary>
        protected static readonly DependencyProperty ResultProperty =
            DependencyProperty.Register("Result", typeof(int), typeof(MessageDialogBase));

        /// <summary>
        /// Identifies the <see cref="ButtonCommand"/> dependency property.
        /// </summary>
        private static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(MessageDialogBase));

        public IEnumerable<DialogButtonInfo> ButtonsSource
        {
            get { return (IEnumerable<DialogButtonInfo>)GetValue(ButtonsSourceProperty); }
            set { SetValue(ButtonsSourceProperty, value); }
        }

        public DataTemplate MessageTemplate
        {
            get { return (DataTemplate)GetValue(MessageTemplateProperty); }
            set { SetValue(MessageTemplateProperty, value); }
        }

        public DataTemplateSelector MessageTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(MessageTemplateSelectorProperty); }
            set { SetValue(MessageTemplateSelectorProperty, value); }
        }

        public int Result
        {
            get { return (int)GetValue(ResultProperty); }
            protected set { SetValue(ResultProperty, value); }
        }

        private ICommand ButtonCommand
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
        }

        protected int ShowInternal()
        {
            this.ShowDialog();
            return result;
        }

        private void ButtonClick(int parameter)
        {
            this.result = parameter;
            this.Close();
        }

        private class MessageDialogCommand<T> : ICommand
        {
            private readonly Func<bool> canExecute;
            private readonly Action<T> action;

            public MessageDialogCommand(Action<T> action, Func<bool> canExecute = null)
            {
                if (action == null)
                    throw new ArgumentNullException(nameof(action));

                this.action = action;
                this.canExecute = canExecute;
            }

            public bool CanExecute(object parameter)
            {
                return canExecute?.Invoke() ?? true;
            }

            public void Execute(object parameter)
            {
                if ((typeof(T).IsValueType || parameter != null) && !(parameter is T))
                    throw new ArgumentException(@"Unexpected parameter type in the command.", nameof(parameter));

                action((T)parameter);
            }

            public event EventHandler CanExecuteChanged;
        }
    }

}
