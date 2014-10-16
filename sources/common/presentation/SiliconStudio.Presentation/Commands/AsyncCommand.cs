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

        public override void Execute(object parameter)
        {
            Task.Run(() => action(parameter));
        }

        public async Task ExecuteAwaitable(object parameter = null)
        {
            await Task.Run(() => action(parameter));
        }
    }
}
