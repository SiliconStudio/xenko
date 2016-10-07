// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SiliconStudio.Core.Tasks
{
    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) :
            base(() => Task.Factory.StartNew(valueFactory))
        {
        }

        public AsyncLazy(Func<Task<T>> taskFactory) :
            base(() => Task.Factory.StartNew(taskFactory).Unwrap())
        {
        }

        public TaskAwaiter<T> GetAwaiter() { return Value.GetAwaiter(); }
    }
}