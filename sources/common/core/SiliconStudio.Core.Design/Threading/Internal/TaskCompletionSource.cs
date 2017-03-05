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
using System.Threading.Tasks;

namespace SiliconStudio.Core.Threading
{
    /// <summary>
    /// Represents the producer side of a <see cref="System.Threading.Tasks.Task"/> unbound to a delegate, providing access to the consumer side through the <see cref="Task"/> property.
    /// </summary>
    public sealed class TaskCompletionSource
    {
        /// <summary>
        /// The underlying TCS.
        /// </summary>
        private readonly TaskCompletionSource<object> _tcs;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskCompletionSource"/> class.
        /// </summary>
        public TaskCompletionSource()
        {
            _tcs = new TaskCompletionSource<object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskCompletionSource"/> class with the specified state.
        /// </summary>
        /// <param name="state">The state to use as the underlying <see cref="Task"/>'s <see cref="IAsyncResult.AsyncState"/>.</param>
        public TaskCompletionSource(object state)
        {
            _tcs = new TaskCompletionSource<object>(state);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskCompletionSource"/> class with the specified options.
        /// </summary>
        /// <param name="creationOptions">The options to use when creating the underlying <see cref="Task"/>.</param>
        public TaskCompletionSource(TaskCreationOptions creationOptions)
        {
            _tcs = new TaskCompletionSource<object>(creationOptions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskCompletionSource"/> class with the specified state and options.
        /// </summary>
        /// <param name="state">The state to use as the underlying <see cref="Task"/>'s <see cref="IAsyncResult.AsyncState"/>.</param>
        /// <param name="creationOptions">The options to use when creating the underlying <see cref="Task"/>.</param>
        public TaskCompletionSource(object state, TaskCreationOptions creationOptions)
        {
            _tcs = new TaskCompletionSource<object>(state, creationOptions);
        }

        /// <summary>
        /// Gets the <see cref="Task"/> created by this <see cref="TaskCompletionSource"/>.
        /// </summary>
        public Task Task
        {
            get { return _tcs.Task; }
        }

        /// <summary>
        /// Transitions the underlying <see cref="Task"/> into the <see cref="TaskStatus.Canceled"/> state.
        /// </summary>
        /// <exception cref="InvalidOperationException">The underlying <see cref="Task"/> has already been completed.</exception>
        public void SetCanceled()
        {
            _tcs.SetCanceled();
        }

        /// <summary>
        /// Attempts to transition the underlying <see cref="Task"/> into the <see cref="TaskStatus.Canceled"/> state.
        /// </summary>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        public bool TrySetCanceled()
        {
            return _tcs.TrySetCanceled();
        }

        /// <summary>
        /// Transitions the underlying <see cref="Task"/> into the <see cref="TaskStatus.Faulted"/> state.
        /// </summary>
        /// <param name="exception">The exception to bind to this <see cref="Task"/>. May not be <c>null</c>.</param>
        /// <exception cref="InvalidOperationException">The underlying <see cref="Task"/> has already been completed.</exception>
        public void SetException(Exception exception)
        {
            _tcs.SetException(exception);
        }

        /// <summary>
        /// Transitions the underlying <see cref="Task"/> into the <see cref="TaskStatus.Faulted"/> state.
        /// </summary>
        /// <param name="exceptions">The collection of exceptions to bind to this <see cref="Task"/>. May not be <c>null</c> or contain <c>null</c> elements.</param>
        /// <exception cref="InvalidOperationException">The underlying <see cref="Task"/> has already been completed.</exception>
        public void SetException(IEnumerable<Exception> exceptions)
        {
            _tcs.SetException(exceptions);
        }

        /// <summary>
        /// Attempts to transition the underlying <see cref="Task"/> into the <see cref="TaskStatus.Faulted"/> state.
        /// </summary>
        /// <param name="exception">The exception to bind to this <see cref="Task"/>. May not be <c>null</c>.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        public bool TrySetException(Exception exception)
        {
            return _tcs.TrySetException(exception);
        }

        /// <summary>
        /// Attempts to transition the underlying <see cref="Task"/> into the <see cref="TaskStatus.Faulted"/> state.
        /// </summary>
        /// <param name="exceptions">The collection of exceptions to bind to this <see cref="Task"/>. May not be <c>null</c> or contain <c>null</c> elements.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        public bool TrySetException(IEnumerable<Exception> exceptions)
        {
            return _tcs.TrySetException(exceptions);
        }

        /// <summary>
        /// Transitions the underlying <see cref="Task"/> into the <see cref="TaskStatus.RanToCompletion"/> state.
        /// </summary>
        /// <exception cref="InvalidOperationException">The underlying <see cref="Task"/> has already been completed.</exception>
        public void SetResult()
        {
            _tcs.SetResult(null);
        }

        /// <summary>
        /// Attempts to transition the underlying <see cref="Task"/> into the <see cref="TaskStatus.RanToCompletion"/> state.
        /// </summary>
        /// <returns><c>true</c> if the operation was successful; otherwise, <c>false</c>.</returns>
        public bool TrySetResult()
        {
            return _tcs.TrySetResult(null);
        }
    }
}