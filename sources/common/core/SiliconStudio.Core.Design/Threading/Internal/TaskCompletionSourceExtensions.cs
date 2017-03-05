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
using System.ComponentModel;
using System.Threading.Tasks;

namespace SiliconStudio.Core.Threading
{
    /// <summary>
    /// Provides extension methods for <see cref="TaskCompletionSource{TResult}"/>.
    /// </summary>
    public static class TaskCompletionSourceExtensions
    {
        /// <summary>
        /// Attempts to complete a <see cref="TaskCompletionSource{TResult}"/>, propagating the completion of <paramref name="task"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the target asynchronous operation.</typeparam>
        /// <typeparam name="TSourceResult">The type of the result of the source asynchronous operation.</typeparam>
        /// <param name="this">The task completion source. May not be <c>null</c>.</param>
        /// <param name="task">The task. May not be <c>null</c>.</param>
        /// <returns><c>true</c> if this method completed the task completion source; <c>false</c> if it was already completed.</returns>
        public static bool TryCompleteFromCompletedTask<TResult, TSourceResult>(this TaskCompletionSource<TResult> @this, Task<TSourceResult> task) where TSourceResult : TResult
        {
            if (task.IsFaulted)
                return @this.TrySetException(task.Exception.InnerExceptions);
            if (task.IsCanceled)
                return @this.TrySetCanceled();
            return @this.TrySetResult(task.Result);
        }

        /// <summary>
        /// Attempts to complete a <see cref="TaskCompletionSource{TResult}"/>, propagating the completion of <paramref name="eventArgs"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the asynchronous operation.</typeparam>
        /// <param name="this">The task completion source. May not be <c>null</c>.</param>
        /// <param name="eventArgs">The event arguments passed to the completion event. May not be <c>null</c>.</param>
        /// <param name="getResult">The delegate used to retrieve the result. This is only invoked if <paramref name="eventArgs"/> indicates successful completion. May not be <c>null</c>.</param>
        /// <returns><c>true</c> if this method completed the task completion source; <c>false</c> if it was already completed.</returns>
        public static bool TryCompleteFromEventArgs<TResult>(this TaskCompletionSource<TResult> @this, AsyncCompletedEventArgs eventArgs, Func<TResult> getResult)
        {
            if (eventArgs.Cancelled)
                return @this.TrySetCanceled();
            if (eventArgs.Error != null)
                return @this.TrySetException(eventArgs.Error);
            return @this.TrySetResult(getResult());
        }

        /// <summary>
        /// Attempts to complete a <see cref="TaskCompletionSource"/>, propagating the completion of <paramref name="task"/>.
        /// </summary>
        /// <param name="this">The task completion source. May not be <c>null</c>.</param>
        /// <param name="task">The task. May not be <c>null</c>.</param>
        /// <returns><c>true</c> if this method completed the task completion source; <c>false</c> if it was already completed.</returns>
        public static bool TryCompleteFromCompletedTask(this TaskCompletionSource @this, Task task)
        {
            if (task.IsFaulted)
                return @this.TrySetException(task.Exception.InnerExceptions);
            if (task.IsCanceled)
                return @this.TrySetCanceled();
            return @this.TrySetResult();
        }

        /// <summary>
        /// Attempts to complete a <see cref="TaskCompletionSource"/>, propagating the completion of <paramref name="eventArgs"/>.
        /// </summary>
        /// <param name="this">The task completion source. May not be <c>null</c>.</param>
        /// <param name="eventArgs">The event arguments passed to the completion event. May not be <c>null</c>.</param>
        /// <returns><c>true</c> if this method completed the task completion source; <c>false</c> if it was already completed.</returns>
        public static bool TryCompleteFromEventArgs(this TaskCompletionSource @this, AsyncCompletedEventArgs eventArgs)
        {
            if (eventArgs.Cancelled)
                return @this.TrySetCanceled();
            if (eventArgs.Error != null)
                return @this.TrySetException(eventArgs.Error);
            return @this.TrySetResult();
        }

        /// <summary>
        /// Attempts to complete a <see cref="TaskCompletionSource{TResult}"/> with the specified value, forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the asynchronous operation.</typeparam>
        /// <param name="this">The task completion source. May not be <c>null</c>.</param>
        /// <param name="result">The result of the asynchronous operation.</param>
        public static void TrySetResultWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> @this, TResult result)
        {
            // Set the result on a threadpool thread, so any synchronous continuations will execute in the background.
            TaskShim.Run(() => @this.TrySetResult(result));

            // Wait for the TCS task to complete; note that the continuations may not be complete.
            @this.Task.Wait();
        }

        /// <summary>
        /// Attempts to complete a <see cref="TaskCompletionSource"/>, forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.
        /// </summary>
        /// <param name="this">The task completion source. May not be <c>null</c>.</param>
        public static void TrySetResultWithBackgroundContinuations(this TaskCompletionSource @this)
        {
            // Set the result on a threadpool thread, so any synchronous continuations will execute in the background.
            TaskShim.Run(() => @this.TrySetResult());

            // Wait for the TCS task to complete; note that the continuations may not be complete.
            @this.Task.Wait();
        }

        /// <summary>
        /// Attempts to complete a <see cref="TaskCompletionSource{TResult}"/> as canceled, forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the asynchronous operation.</typeparam>
        /// <param name="this">The task completion source. May not be <c>null</c>.</param>
        public static void TrySetCanceledWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> @this)
        {
            // Complete on a threadpool thread, so any synchronous continuations will execute in the background.
            TaskShim.Run(() => @this.TrySetCanceled());

            // Wait for the TCS task to complete; note that the continuations may not be complete.
            try
            {
                @this.Task.Wait();
            }
            catch (AggregateException)
            {
            }
        }

        /// <summary>
        /// Attempts to complete a <see cref="TaskCompletionSource"/> as canceled, forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.
        /// </summary>
        /// <param name="this">The task completion source. May not be <c>null</c>.</param>
        public static void TrySetCanceledWithBackgroundContinuations(this TaskCompletionSource @this)
        {
            // Set the result on a threadpool thread, so any synchronous continuations will execute in the background.
            TaskShim.Run(() => @this.TrySetCanceled());

            // Wait for the TCS task to complete; note that the continuations may not be complete.
            try
            {
                @this.Task.Wait();
            }
            catch (AggregateException)
            {
            }
        }

        /// <summary>
        /// Attempts to complete a <see cref="TaskCompletionSource{TResult}"/> as faulted, forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the asynchronous operation.</typeparam>
        /// <param name="this">The task completion source. May not be <c>null</c>.</param>
        /// <param name="exception">The exception to bind to the task.</param>
        public static void TrySetExceptionWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> @this, Exception exception)
        {
            // Complete on a threadpool thread, so any synchronous continuations will execute in the background.
            TaskShim.Run(() => @this.TrySetException(exception));

            // Wait for the TCS task to complete; note that the continuations may not be complete.
            try
            {
                @this.Task.Wait();
            }
            catch (AggregateException)
            {
            }
        }

        /// <summary>
        /// Attempts to complete a <see cref="TaskCompletionSource"/> as faulted, forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.
        /// </summary>
        /// <param name="this">The task completion source. May not be <c>null</c>.</param>
        /// <param name="exception">The exception to bind to the task.</param>
        public static void TrySetExceptionWithBackgroundContinuations(this TaskCompletionSource @this, Exception exception)
        {
            // Set the result on a threadpool thread, so any synchronous continuations will execute in the background.
            TaskShim.Run(() => @this.TrySetException(exception));

            // Wait for the TCS task to complete; note that the continuations may not be complete.
            try
            {
                @this.Task.Wait();
            }
            catch (AggregateException)
            {
            }
        }

        /// <summary>
        /// Attempts to complete a <see cref="TaskCompletionSource{TResult}"/> as faulted, forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the asynchronous operation.</typeparam>
        /// <param name="this">The task completion source. May not be <c>null</c>.</param>
        /// <param name="exceptions">The exceptions to bind to the task.</param>
        public static void TrySetExceptionWithBackgroundContinuations<TResult>(this TaskCompletionSource<TResult> @this, IEnumerable<Exception> exceptions)
        {
            // Complete on a threadpool thread, so any synchronous continuations will execute in the background.
            TaskShim.Run(() => @this.TrySetException(exceptions));

            // Wait for the TCS task to complete; note that the continuations may not be complete.
            try
            {
                @this.Task.Wait();
            }
            catch (AggregateException)
            {
            }
        }

        /// <summary>
        /// Attempts to complete a <see cref="TaskCompletionSource"/> as faulted, forcing all continuations onto a threadpool thread even if they specified <c>ExecuteSynchronously</c>.
        /// </summary>
        /// <param name="this">The task completion source. May not be <c>null</c>.</param>
        /// <param name="exceptions">The exceptions to bind to the task.</param>
        public static void TrySetExceptionWithBackgroundContinuations(this TaskCompletionSource @this, IEnumerable<Exception> exceptions)
        {
            // Set the result on a threadpool thread, so any synchronous continuations will execute in the background.
            TaskShim.Run(() => @this.TrySetException(exceptions));

            // Wait for the TCS task to complete; note that the continuations may not be complete.
            try
            {
                @this.Task.Wait();
            }
            catch (AggregateException)
            {
            }
        }
    }
}