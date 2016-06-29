// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

using SiliconStudio.Presentation.Commands;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// This command will bind a <see cref="ICommandBase"/> to a <see cref="RoutedCommand"/>. It works just as a <see cref="CommandBinding"/> except that the bound
    /// command is executed when the routed command is executed. The <b>CanExecute</b> handler also invoke the <b>CanExecute</b> method of the <see cref="ICommandBase"/>.
    /// </summary>
    public class CommandBindingBehavior : Behavior<FrameworkElement>
    {
        private CommandBinding commandBinding;

        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommandBase), typeof(CommandBindingBehavior), new PropertyMetadata(null, CommandChanged));
        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register(nameof(IsEnabled), typeof(bool), typeof(CommandBindingBehavior), new PropertyMetadata(true));
        // Using a DependencyProperty as the backing store for RoutedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RoutedCommandProperty =
            DependencyProperty.Register(nameof(RoutedCommand), typeof(RoutedCommand), typeof(CommandBindingBehavior));

        /// <summary>
        /// Gets or sets the <see cref="RoutedCommand"/> to bind.
        /// </summary>
        public RoutedCommand RoutedCommand { get { return (RoutedCommand)GetValue(RoutedCommandProperty); } set { SetValue(RoutedCommandProperty, value); } }

        /// <summary>
        /// Gets or sets the <see cref="ICommandBase"/> to bind.
        /// </summary>
        public ICommandBase Command { get { return (ICommandBase)GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }

        /// <summary>
        /// Gets or sets whether this command binding is enabled. When disabled, the <see cref="Command"/> won't be executed.
        /// </summary>
        public bool IsEnabled { get { return (bool)GetValue(IsEnabledProperty); } set { SetValue(IsEnabledProperty, value); } }

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            commandBinding = new CommandBinding(RoutedCommand, (s, e) => OnExecuted(e), (s, e) => OnCanExecute(e));
            AssociatedObject.CommandBindings.Add(commandBinding);
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            AssociatedObject.CommandBindings.Remove(commandBinding);
        }

        private static void CommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnCanExecute(CanExecuteRoutedEventArgs canExecuteRoutedEventArgs)
        {
            if (Command != null)
            {
                canExecuteRoutedEventArgs.CanExecute = IsEnabled && Command.CanExecute(canExecuteRoutedEventArgs.Parameter);
            }
            else
            {
                canExecuteRoutedEventArgs.CanExecute = false;
            }

            if (canExecuteRoutedEventArgs.CanExecute)
            {
                canExecuteRoutedEventArgs.Handled = true;
            }
            else
            {
                canExecuteRoutedEventArgs.ContinueRouting = true;
            }
        }

        private void OnExecuted(ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            if (Command != null && IsEnabled)
            {
                Command.Execute(executedRoutedEventArgs.Parameter);
                executedRoutedEventArgs.Handled = true;
            }
        }
    }
}
