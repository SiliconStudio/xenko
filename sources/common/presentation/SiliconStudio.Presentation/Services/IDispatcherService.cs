// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

namespace SiliconStudio.Presentation.Services
{
    public interface IDispatcherService
    {
        void Invoke(Action callback);
        TResult Invoke<TResult>(Func<TResult> callback);

        Action Invoked(Action callback);

        Task InvokeAsync(Action callback);
        Task<TResult> InvokeAsync<TResult>(Func<TResult> callback);
    }
}
