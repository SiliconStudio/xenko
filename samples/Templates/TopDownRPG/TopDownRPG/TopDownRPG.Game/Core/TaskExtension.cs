// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Threading.Tasks;

namespace TopDownRPG.Core
{
    public static class TaskExtension
    {
        public static async Task<T> InterruptedBy<T>(this Task<T> mainTask, Task interruptingTask, Action<Task> interruptionAction)
        {
            var firstCompleted = await Task.WhenAny(mainTask, interruptingTask);
            if (firstCompleted != mainTask)
            {
                // Interrupted, run action
                interruptionAction(firstCompleted);
                // And return a task that will never complete
                return await new TaskCompletionSource<T>().Task;
            }
            return mainTask.Result;
        }
    }
}
