// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Input;

namespace SiliconStudio.Presentation.Commands
{
    internal class SystemCommand : ICommand
    {
        private readonly Func<Window, bool> canExecute;
        private readonly Action<Window> execute;

        internal SystemCommand(Func<Window, bool> canExecute, Action<Window> execute)
        {
            if (canExecute == null) throw new ArgumentNullException("canExecute");
            if (execute == null) throw new ArgumentNullException("execute");
            this.canExecute = canExecute;
            this.execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            var window = parameter as Window;
            return window != null && canExecute(window);
        }

        public void Execute(object parameter)
        {
            var window = parameter as Window;
            if (window == null) throw new ArgumentException(string.Format("The parameter of this command must be an instance of 'Window'."));
            execute(window);
        }

			// We provide an empty `add' and `remove' to avoid a warning about unused events that we have
			// to implement as they are part of the ICommand definition.
        public event EventHandler CanExecuteChanged { add { } remove { } }
    }
}
