// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.Commands
{
    /// <summary>
    /// An implementation of <see cref="CommandBase"/> that route <see cref="Execute"/> calls to a given anonymous method.
    /// </summary>
    /// <seealso cref="AnonymousCommand{T}"/>
    public class AnonymousCommand : CommandBase
    {
        private readonly Func<bool> canExecute;
        private readonly Action<object> action;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousCommand"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="Services.IDispatcherService"/> to use for this view model.</param>
        /// <param name="action">An anonymous method that will be called each time the command is executed.</param>
        public AnonymousCommand(IViewModelServiceProvider serviceProvider, Action action)
            : base(serviceProvider)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            this.action = x => action();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousCommand"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="Services.IDispatcherService"/> to use for this view model.</param>
        /// <param name="action">An anonymous method that will be called each time the command is executed.</param>
        /// <param name="canExecute">An anonymous method that will be called each time the command <see cref="CommandBase.CanExecute(object)"/> method is invoked.</param>
        public AnonymousCommand(IViewModelServiceProvider serviceProvider, Action action, Func<bool> canExecute)
            : base(serviceProvider)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            this.action = x => action();
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousCommand"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="Services.IDispatcherService"/> to use for this view model.</param>
        /// <param name="action">An anonymous method that will be called each time the command is executed.</param>
        public AnonymousCommand(IViewModelServiceProvider serviceProvider, Action<object> action)
            : base(serviceProvider)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            this.action = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousCommand"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="Services.IDispatcherService"/> to use for this view model.</param>
        /// <param name="action">An anonymous method that will be called each time the command is executed.</param>
        /// <param name="canExecute">An anonymous method that will be called each time the command <see cref="CommandBase.CanExecute(object)"/> method is invoked.</param>
        public AnonymousCommand(IViewModelServiceProvider serviceProvider, Action<object> action, Func<bool> canExecute)
            : base(serviceProvider)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            this.action = action;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Executes the command, and thus the anonymous method provided to the constructor.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <seealso cref="AnonymousCommand{T}"/>
        public override void Execute(object parameter)
        {
            action(parameter);
        }

        /// <summary>
        /// Indicates whether the command can be executed. Returns <c>true</c> if <see cref="CommandBase.IsEnabled"/> is <c>true</c> and either a <see cref="Func{Bool}"/>
        /// was provided to the constructor, and this function returns <c>true</c>, or no <see cref="Func{Bool}"/> was provided (or a <c>null</c> function).
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns><c>true</c> if the command can be executed, <c>false</c> otherwise.</returns>
        public override bool CanExecute(object parameter)
        {
            var result = base.CanExecute(parameter);
            return result && canExecute != null ? canExecute() : result;
        }
    }

    /// <summary>
    /// An implementation of <see cref="CommandBase"/> that route <see cref="Execute"/> calls to a given anonymous method with a typed parameter.
    /// </summary>
    /// <typeparam name="T">The type of parameter to use with the command.</typeparam>
    /// <seealso cref="AnonymousCommand"/>
    public class AnonymousCommand<T> : CommandBase
    {
        private readonly Action<T> action;
        private readonly Func<T, bool> canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousCommand{T}"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="Services.IDispatcherService"/> to use for this view model.</param>
        /// <param name="action">An anonymous method with a typed parameter that will be called each time the command is executed.</param>
        public AnonymousCommand(IViewModelServiceProvider serviceProvider, Action<T> action)
            : base(serviceProvider)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            this.action = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousCommand{T}"/> class.
        /// </summary>
        /// <param name="serviceProvider">A service provider that can provide a <see cref="Services.IDispatcherService"/> to use for this view model.</param>
        /// <param name="action">An anonymous method with a typed parameter that will be called each time the command is executed.</param>
        /// <param name="canExecute">An anonymous method that will be called each time the command <see cref="CommandBase.CanExecute(object)"/> method is invoked.</param>
        public AnonymousCommand(IViewModelServiceProvider serviceProvider, Action<T> action, Func<T, bool> canExecute)
            : base(serviceProvider)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            this.action = action;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Executes the command, and thus the anonymous method provided to the constructor.
        /// </summary>
        /// <seealso cref="AnonymousCommand"/>
        public override void Execute(object parameter)
        {
            if ((typeof(T).IsValueType || parameter != null) && !(parameter is T))
                throw new ArgumentException(@"Unexpected parameter type in the command.", nameof(parameter));

            action((T)parameter);
        }

        /// <summary>
        /// Indicates whether the command can be executed. Returns <c>true</c> if <see cref="CommandBase.IsEnabled"/> is <c>true</c> and either a <see cref="Func{T, Bool}"/>
        /// was provided to the constructor, and this function returns <c>true</c>, or no <see cref="Func{T, Bool}"/> was provided (or a <c>null</c> function).
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns><c>true</c> if the command can be executed, <c>false</c> otherwise.</returns>
        public override bool CanExecute(object parameter)
        {
            if ((typeof(T).IsValueType || parameter != null) && !(parameter is T))
                throw new ArgumentException(@"Unexpected parameter type in the command.", nameof(parameter));

            var result = base.CanExecute(parameter);
            return result && canExecute != null ? canExecute((T)parameter) : result;
        }
    }
}
