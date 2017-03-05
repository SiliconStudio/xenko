// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#region Copyright and license
// Some parts of this file were inspired by AsyncEx (https://github.com/StephenCleary/AsyncEx)
/*
The MIT License (MIT)
https://opensource.org/licenses/MIT

Copyright (c) 2014 StephenCleary

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiliconStudio.Core.Threading
{
    internal static class TaskShim
    {
        public static Task Run(Action func)
        {
            return Task.Run(func);
        }

        public static Task Run(Func<Task> func)
        {
            return Task.Run(func);
        }

        public static Task<T> Run<T>(Func<T> func)
        {
            return Task.Run(func);
        }

        public static Task<T> Run<T>(Func<Task<T>> func)
        {
            return Task.Run(func);
        }

        public static Task<T> FromResult<T>(T value)
        {
            return Task.FromResult(value);
        }

        public static Task<T[]> WhenAll<T>(IEnumerable<Task<T>> tasks)
        {
            return Task.WhenAll(tasks);
        }

        public static Task<T[]> WhenAll<T>(params Task<T>[] tasks)
        {
            return Task.WhenAll(tasks);
        }

        public static Task WhenAll(params Task[] tasks)
        {
            return Task.WhenAll(tasks);
        }

        public static Task WhenAll(IEnumerable<Task> tasks)
        {
            return Task.WhenAll(tasks);
        }

        public static Task<Task<TResult>> WhenAny<TResult>(IEnumerable<Task<TResult>> tasks)
        {
            return Task.WhenAny(tasks);
        }

        public static Task<Task> WhenAny(IEnumerable<Task> tasks)
        {
            return Task.WhenAny(tasks);
        }

        public static Task<Task<TResult>> WhenAny<TResult>(params Task<TResult>[] tasks)
        {
            return Task.WhenAny(tasks);
        }

        public static Task<Task> WhenAny(params Task[] tasks)
        {
            return Task.WhenAny(tasks);
        }
    }
}