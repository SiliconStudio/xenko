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

using System.Threading.Tasks;

namespace SiliconStudio.Core.Threading
{
    /// <summary>
    /// Provides completed task constants.
    /// </summary>
    public static class TaskConstants
    {
        private static readonly Task<bool> booleanTrue = TaskShim.FromResult(true);
        private static readonly Task<int> intNegativeOne = TaskShim.FromResult(-1);

        /// <summary>
        /// A task that has been completed with the value <c>true</c>.
        /// </summary>
        public static Task<bool> BooleanTrue
        {
            get
            {
                return booleanTrue;
            }
        }

        /// <summary>
        /// A task that has been completed with the value <c>false</c>.
        /// </summary>
        public static Task<bool> BooleanFalse
        {
            get
            {
                return TaskConstants<bool>.Default;
            }
        }

        /// <summary>
        /// A task that has been completed with the value <c>0</c>.
        /// </summary>
        public static Task<int> Int32Zero
        {
            get
            {
                return TaskConstants<int>.Default;
            }
        }

        /// <summary>
        /// A task that has been completed with the value <c>-1</c>.
        /// </summary>
        public static Task<int> Int32NegativeOne
        {
            get
            {
                return intNegativeOne;
            }
        }

        /// <summary>
        /// A <see cref="Task"/> that has been completed.
        /// </summary>
        public static Task Completed
        {
            get
            {
                return booleanTrue;
            }
        }

        /// <summary>
        /// A <see cref="Task"/> that will never complete.
        /// </summary>
        public static Task Never
        {
            get
            {
                return TaskConstants<bool>.Never;
            }
        }

        /// <summary>
        /// A task that has been canceled.
        /// </summary>
        public static Task Canceled
        {
            get
            {
                return TaskConstants<bool>.Canceled;
            }
        }
    }

    /// <summary>
    /// Provides completed task constants.
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    public static class TaskConstants<T>
    {
        private static readonly Task<T> defaultValue = TaskShim.FromResult(default(T));

        private static readonly Task<T> never = new TaskCompletionSource<T>().Task;

        private static readonly Task<T> canceled = CanceledTask();

        private static Task<T> CanceledTask()
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
        }

        /// <summary>
        /// A task that has been completed with the default value of <typeparamref name="T"/>.
        /// </summary>
        public static Task<T> Default
        {
            get
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// A <see cref="Task"/> that will never complete.
        /// </summary>
        public static Task<T> Never
        {
            get
            {
                return never;
            }
        }

        /// <summary>
        /// A task that has been canceled.
        /// </summary>
        public static Task<T> Canceled
        {
            get
            {
                return canceled;
            }
        }
    }
}