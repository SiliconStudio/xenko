// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    /// <summary>
    /// A <see cref="TaskScheduler"/> with control over concurrency and priority, useful with <see cref="EffectCompilerCache"/>.
    /// </summary>
    public class EffectPriorityScheduler : TaskScheduler
    {
        private static object lockObject = new object();
        private BlockingCollection<Task> tasks = new BlockingCollection<Task>();
        private ThreadPriority threadPriority;
        private Thread[] threads;
        private readonly int maximumConcurrencyLevel;

        public EffectPriorityScheduler(ThreadPriority threadPriority, int maximumConcurrencyLevel)
        {
            if (maximumConcurrencyLevel == 0)
                throw new ArgumentOutOfRangeException("maximumConcurrencyLevel");

            this.threadPriority = threadPriority;
            this.maximumConcurrencyLevel = maximumConcurrencyLevel;
        }

        /// <inheritdoc/>
        public override int MaximumConcurrencyLevel
        {
            get { return maximumConcurrencyLevel; }
        }

        /// <inheritdoc/>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        /// <inheritdoc/>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return tasks;
        }

        /// <inheritdoc/>
        protected override void QueueTask(Task task)
        {
            // Add task to thread-safe queue
            tasks.Add(task);

            // If necessary, create threads
            if (threads == null)
            {
                lock (lockObject)
                {
                    if (threads == null)
                    {
                        threads = new Thread[maximumConcurrencyLevel];
                        for (int i = 0; i < maximumConcurrencyLevel; i++)
                        {
                            threads[i] = new Thread(() =>
                            {
                                foreach (Task t in tasks.GetConsumingEnumerable())
                                    TryExecuteTask(t);
                            })
                            {
                                Name = string.Format("PriorityScheduler: {0}", i),
                                Priority = threadPriority,
                                IsBackground = true,
                            };
                            threads[i].Start();
                        }
                    }
                }
            }
        }
    }
}