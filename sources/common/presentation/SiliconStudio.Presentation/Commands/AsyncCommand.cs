// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.Commands
{
    public class AsyncCommand : CommandBase
    {
        private readonly Action<object> action;
        private readonly Func<bool> canExecute;

        public AsyncCommand(IViewModelServiceProvider serviceProvider, Action<object> action)
            : base(serviceProvider)
        {
            this.action = action;
        }

        public AsyncCommand(IViewModelServiceProvider serviceProvider, Action action)
            : base(serviceProvider)
        {
            this.action = x => action();
        }

        public AsyncCommand(IViewModelServiceProvider serviceProvider, Action<object> action, Func<bool> canExecute)
            : base(serviceProvider)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public AsyncCommand(IViewModelServiceProvider serviceProvider, Action action, Func<bool> canExecute)
            : base(serviceProvider)
        {
            this.action = x => action();
            this.canExecute = canExecute;
        }

        public override void Execute(object parameter)
        {
            Task.Run(() => action(parameter));
        }

        public Task ExecuteAwaitable(object parameter = null)
        {
            return Task.Run(() => action(parameter));
        }

        public override bool CanExecute(object parameter)
        {
            var result = base.CanExecute(parameter);
            return result && canExecute != null ? canExecute() : result;
        }
    }

    public class AsyncCommand<T> : CommandBase
    {
        private readonly Action<T> action;
        private readonly Func<bool> canExecute;

        public AsyncCommand(IViewModelServiceProvider serviceProvider, Action<T> action)
            : base(serviceProvider)
        {
            this.action = action;
        }

        public AsyncCommand(IViewModelServiceProvider serviceProvider, Action<T> action, Func<bool> canExecute)
            : base(serviceProvider)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        public override void Execute(object parameter)
        {
            if ((typeof(T).IsValueType || parameter != null) && !(parameter is T))
                throw new ArgumentException(@"Unexpected parameter type in the command.", "parameter");

            action((T)parameter);
        }


        public Task ExecuteAwaitable(object parameter)
        {
            return Task.Run(() => Execute(parameter));
        }

        public override bool CanExecute(object parameter)
        {
            var result = base.CanExecute(parameter);
            return result && canExecute != null ? canExecute() : result;
        }
    }
}
