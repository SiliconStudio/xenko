// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.View
{
    public class DispatcherService : IDispatcherService
    {
        private readonly Dispatcher dispatcher;

        public static DispatcherService Create()
        {
            return new DispatcherService(Dispatcher.CurrentDispatcher);
        }

        public DispatcherService(Dispatcher dispatcher)
        {
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");
            this.dispatcher = dispatcher;
        }

        public void Invoke(Action callback)
        {
            dispatcher.Invoke(callback);
        }

        public TResult Invoke<TResult>(Func<TResult> callback)
        {
            return dispatcher.Invoke(callback);
        }

        public Action Invoked(Action callback)
        {
            return () => dispatcher.Invoke(callback);
        }

        public Task InvokeAsync(Action callback)
        {
            return dispatcher.InvokeAsync(callback).Task;
        }

        public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback)
        {
            return dispatcher.InvokeAsync(callback).Task;
        }
    }
}
