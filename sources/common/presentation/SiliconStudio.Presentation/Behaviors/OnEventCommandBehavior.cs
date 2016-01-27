// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Windows;
using System.Windows.Input;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// An implementation of the <see cref="OnEventBehavior"/> class that allows to invoke a command when a specific event is raised.
    /// </summary>
    public class OnEventCommandBehavior : OnEventBehavior
    {
        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(OnEventCommandBehavior));

        /// <summary>
        /// Identifies the <see cref="CommandParameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(OnEventCommandBehavior));

        /// <summary>
        /// Identifies the <see cref="PassEventArgsAsParameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PassEventArgsAsParameterProperty = DependencyProperty.Register(nameof(PassEventArgsAsParameter), typeof(bool), typeof(OnEventCommandBehavior));
        
        /// <summary>
        /// Gets or sets the command to invoke when the event is raised.
        /// </summary>
        public ICommand Command { get { return (ICommand)GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }

        /// <summary>
        /// Gets or sets the parameter of the command to invoke when the event is raised.
        /// </summary>
        public object CommandParameter { get { return GetValue(CommandParameterProperty); } set { SetValue(CommandParameterProperty, value); } }

        /// <summary>
        /// Gets or sets whether the arguments of the event should be used as the parameter of the command to execute when the event is raised.
        /// </summary>
        public bool PassEventArgsAsParameter { get { return (bool)GetValue(PassEventArgsAsParameterProperty); } set { SetValue(PassEventArgsAsParameterProperty, value); } }

        /// <inheritdoc/>
        protected override void OnEvent(EventArgs e)
        {
            var cmd = Command;
            var parameter = PassEventArgsAsParameter ? e : CommandParameter;
            if (cmd == null || !cmd.CanExecute(parameter))
                return;

            cmd.Execute(parameter);
        }
    }
}
