// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiliconStudio.Core.Threading
{
    /// <summary>
    /// Helper methods to dispatch action-items from a list to several threads.
    /// </summary>
    public static class TaskList
    {
        /// <summary>
        /// Dispatcher to process items on several threads with a specified action.
        /// </summary>
        /// <typeparam name="T">Type of the item data.</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="threadCount">The thread count, number of thread tasks in parallel.</param>
        /// <param name="threshold">The threshold, if number of items is above the threshold, the task parallel is used, otherwise it is sequential.</param>
        /// <param name="action">The action.</param>
        public static void Dispatch<T>(IList<T> items, int threadCount, int threshold, Action<int, T> action)
        {
            if (items.Count < threshold)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    var entity = items[i];
                    action(i, entity);
                }
            }
            else
            {
                var count = items.Count / threadCount;

                var tasks = new Task[threadCount];

                int fromIndex = 0;
                for (int i = 0; i < threadCount; i++)
                {
                    if ((i + 1) == threadCount)
                    {
                        count += items.Count - count * threadCount;
                    }

                    var localIndex = fromIndex;
                    var localCount = count;
                    var task = new Task(
                        () =>
                        {
                            for (int j = 0; j < localCount; j++)
                            {
                                var entity = items[j + localIndex];
                                action(j + localIndex, entity);
                            }
                        });
                    tasks[i] = task;
                    task.Start();
                    fromIndex += count;
                }

                Task.WaitAll(tasks);
            }
        }
    }
}
