// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#region Copyright and license
// Some parts of this file were inspired by AsyncEx (https://github.com/StephenCleary/AsyncEx)
/*
The MIT license (MTI)
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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconStudio.Presentation
{
    /// <summary>
    /// Watches a task and raises property-changed notifications when the task completes.
    /// </summary>
    public interface INotifyTaskCompletion : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the task being watched.
        /// This property never changes and is never <c>null</c>.
        /// </summary>
        Task Task { get; }

        /// <summary>
        /// Gets a task that completes successfully when <see cref="Task"/> completes (successfully, faulted, or canceled).
        /// This property never changes and is never <c>null</c>.
        /// </summary>
        Task TaskCompleted { get; }

        /// <summary>
        /// Gets the current task status. This property raises a notification when the task completes.
        /// </summary>
        TaskStatus Status { get; }

        /// <summary>
        /// Gets whether the task has completed. This property raises a notification when the value changes to <c>true</c>.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Gets whether the task has completed successfully. This property raises a notification only if the task completes successfully (i.e., if the value changes to <c>true</c>).
        /// </summary>
        bool IsSuccessfullyCompleted { get; }

        /// <summary>
        /// Gets whether the task has been canceled. This property raises a notification only if the task is canceled (i.e., if the value changes to <c>true</c>).
        /// </summary>
        bool IsCanceled { get; }

        /// <summary>
        /// Gets whether the task has faulted. This property raises a notification only if the task faults (i.e., if the value changes to <c>true</c>).
        /// </summary>
        bool IsFaulted { get; }

        /// <summary>
        /// Gets the wrapped faulting exception for the task. Returns <c>null</c> if the task is not faulted. This property raises a notification only if the task faults (i.e., if the value changes to non-<c>null</c>).
        /// </summary>
        AggregateException Exception { get; }

        /// <summary>
        /// Gets the original faulting exception for the task. Returns <c>null</c> if the task is not faulted. This property raises a notification only if the task faults (i.e., if the value changes to non-<c>null</c>).
        /// </summary>
        Exception InnerException { get; }

        /// <summary>
        /// Gets the error message for the original faulting exception for the task. Returns <c>null</c> if the task is not faulted. This property raises a notification only if the task faults (i.e., if the value changes to non-<c>null</c>).
        /// </summary>
        string ErrorMessage { get; }
    }

    /// <summary>
    /// Watches a task and raises property-changed notifications when the task completes.
    /// </summary>
    /// <typeparam name="TResult">The type of the task result.</typeparam>
    public interface INotifyTaskCompletion<TResult> : INotifyTaskCompletion
    {
        /// <summary>
        /// Gets the task being watched. This property never changes and is never <c>null</c>.
        /// </summary>
        new Task<TResult> Task { get; }

        /// <summary>
        /// Gets the result of the task. Returns the default value of <typeparamref name="TResult"/> if the task has not completed successfully.
        /// This property raises a notification only if the task completes successfully.
        /// </summary>
        TResult Result { get; }
    }

    /// <summary>
    /// Factory for task completion notifiers.
    /// </summary>
    public static class NotifyTaskCompletion
    {
        /// <summary>
        /// Creates a new task notifier watching the specified task.
        /// </summary>
        /// <param name="task">The task to watch.</param>
        /// <returns>A new task notifier watching the specified task.</returns>
        public static INotifyTaskCompletion Create(Task task)
        {
            return new NotifyTaskCompletionImplementation(task);
        }

        /// <summary>
        /// Creates a new task notifier watching the specified task.
        /// </summary>
        /// <typeparam name="TResult">The type of the task result.</typeparam>
        /// <param name="task">The task to watch.</param>
        /// <returns>A new task notifier watching the specified task.</returns>
        public static INotifyTaskCompletion<TResult> Create<TResult>(Task<TResult> task)
        {
            return new NotifyTaskCompletionImplementation<TResult>(task);
        }

        /// <summary>
        /// Executes the specified asynchronous code and creates a new task notifier watching the returned task.
        /// </summary>
        /// <param name="asyncAction">The asynchronous code to execute.</param>
        /// <returns>A new task notifier watching the returned task.</returns>
        public static INotifyTaskCompletion Create(Func<Task> asyncAction)
        {
            return Create(asyncAction());
        }

        /// <summary>
        /// Executes the specified asynchronous code and creates a new task notifier watching the returned task.
        /// </summary>
        /// <param name="asyncAction">The asynchronous code to execute.</param>
        /// <returns>A new task notifier watching the returned task.</returns>
        public static INotifyTaskCompletion<TResult> Create<TResult>(Func<Task<TResult>> asyncAction)
        {
            return Create(asyncAction());
        }

        /// <summary>
        /// Watches a task and raises property-changed notifications when the task completes.
        /// </summary>
        private sealed class NotifyTaskCompletionImplementation : INotifyTaskCompletion
        {
            /// <summary>
            /// Initializes a task notifier watching the specified task.
            /// </summary>
            /// <param name="task">The task to watch.</param>
            public NotifyTaskCompletionImplementation(Task task)
            {
                Task = task;
                if (task.IsCompleted)
                {
                    TaskCompleted = Task.FromResult(true);
                    return;
                }

                var scheduler = (SynchronizationContext.Current == null) ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
                TaskCompleted = task.ContinueWith(t =>
                {
                    var propertyChanged = PropertyChanged;
                    if (propertyChanged == null)
                        return;

                    propertyChanged(this, new PropertyChangedEventArgs(nameof(Status)));
                    propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
                    if (t.IsCanceled)
                    {
                        propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCanceled)));
                    }
                    else if (t.IsFaulted)
                    {
                        propertyChanged(this, new PropertyChangedEventArgs(nameof(IsFaulted)));
                        propertyChanged(this, new PropertyChangedEventArgs(nameof(Exception)));
                        propertyChanged(this, new PropertyChangedEventArgs(nameof(InnerException)));
                        propertyChanged(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
                    }
                    else
                    {
                        propertyChanged(this, new PropertyChangedEventArgs(nameof(IsSuccessfullyCompleted)));
                    }
                },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    scheduler);
            }

            public Task Task { get; }

            public Task TaskCompleted { get; }

            public TaskStatus Status => Task.Status;

            public bool IsCompleted => Task.IsCompleted;

            public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;

            public bool IsCanceled => Task.IsCanceled;

            public bool IsFaulted => Task.IsFaulted;

            public AggregateException Exception => Task.Exception;

            public Exception InnerException => Exception?.InnerException;

            public string ErrorMessage => InnerException?.Message;

            public event PropertyChangedEventHandler PropertyChanged;
        }

        /// <summary>
        /// Watches a task and raises property-changed notifications when the task completes.
        /// </summary>
        /// <typeparam name="TResult">The type of the task result.</typeparam>
        private sealed class NotifyTaskCompletionImplementation<TResult> : INotifyTaskCompletion<TResult>
        {
            /// <summary>
            /// Initializes a task notifier watching the specified task.
            /// </summary>
            /// <param name="task">The task to watch.</param>
            public NotifyTaskCompletionImplementation(Task<TResult> task)
            {
                Task = task;
                if (task.IsCompleted)
                {
                    TaskCompleted = System.Threading.Tasks.Task.FromResult(true);
                    return;
                }

                var scheduler = (SynchronizationContext.Current == null) ? TaskScheduler.Current : TaskScheduler.FromCurrentSynchronizationContext();
                TaskCompleted = task.ContinueWith(t =>
                {
                    var propertyChanged = PropertyChanged;
                    if (propertyChanged == null)
                        return;

                    propertyChanged(this, new PropertyChangedEventArgs(nameof(Status)));
                    propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCompleted)));
                    if (t.IsCanceled)
                    {
                        propertyChanged(this, new PropertyChangedEventArgs(nameof(IsCanceled)));
                    }
                    else if (t.IsFaulted)
                    {
                        propertyChanged(this, new PropertyChangedEventArgs(nameof(IsFaulted)));
                        propertyChanged(this, new PropertyChangedEventArgs(nameof(Exception)));
                        propertyChanged(this, new PropertyChangedEventArgs(nameof(InnerException)));
                        propertyChanged(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
                    }
                    else
                    {
                        propertyChanged(this, new PropertyChangedEventArgs(nameof(IsSuccessfullyCompleted)));
                    }
                },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    scheduler);
            }

            public Task<TResult> Task { get; }

            public Task TaskCompleted { get; }

            Task INotifyTaskCompletion.Task => Task;

            public TResult Result => (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : default(TResult);

            public TaskStatus Status => Task.Status;

            public bool IsCompleted => Task.IsCompleted;

            public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;

            public bool IsCanceled => Task.IsCanceled;

            public bool IsFaulted => Task.IsFaulted;

            public AggregateException Exception => Task.Exception;

            public Exception InnerException => Exception?.InnerException;

            public string ErrorMessage => InnerException?.Message;

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}
