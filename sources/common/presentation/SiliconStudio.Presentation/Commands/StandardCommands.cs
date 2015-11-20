using System;
using System.Diagnostics;
using System.Windows.Input;

namespace SiliconStudio.Presentation.Commands
{
    public static class StandardCommands
    {
        static StandardCommands()
        {
            OpenHyperlinkCommand = new StandardCommand<string>(link => Process.Start(link), link => Uri.IsWellFormedUriString(link, UriKind.RelativeOrAbsolute));
        }

        public static ICommand OpenHyperlinkCommand { get; }

        private class StandardCommand<T> : ICommand
        {
            private readonly Func<T, bool> canExecute;
            private readonly Action<T> action;

            public StandardCommand(Action<T> action, Func<T, bool> canExecute = null)
            {
                if (action == null)
                    throw new ArgumentNullException(nameof(action));

                this.action = action;
                this.canExecute = canExecute;
            }

            public bool CanExecute(object parameter)
            {
                if ((typeof(T).IsValueType || parameter != null) && !(parameter is T))
                    throw new ArgumentException(@"Unexpected parameter type in the command.", nameof(parameter));
                return canExecute?.Invoke((T)parameter) ?? true;
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
