using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SiliconStudio.Core.Threading
{
    /// <remarks>
    /// Base on Stephen Toub's ManagedThreadPool
    /// </remarks>
    public class ThreadPool
    {
        public static readonly ThreadPool Instance = new ThreadPool();

        private readonly List<Task> workers = new List<Task>();
        private readonly Queue<Action> workItems = new Queue<Action>();
        private int activeThreadCount;
        private SpinLock spinLock = new SpinLock();
        private AutoResetEvent workAvailable = new AutoResetEvent(false);

        public void QueueWorkItem(Action workItem)
        {
            bool lockTaken = false;
            try
            {
                spinLock.Enter(ref lockTaken);

                workItems.Enqueue(workItem);

                if (activeThreadCount + 1 >= workers.Count && workers.Count < Environment.ProcessorCount * 2)
                {
                    var worker = Task.Factory.StartNew(ProcessWorkItems, workers.Count, TaskCreationOptions.LongRunning);
                    workers.Add(worker);
                    Console.WriteLine($"Thread {workers.Count} added");
                }

                workAvailable.Set();
            }
            finally
            {
                if (lockTaken)
                    spinLock.Exit(true);
            }
        }

        public void QueueWorkItems(IReadOnlyCollection<Action> workItems)
        {
            bool lockTaken = false;
            try
            {
                spinLock.Enter(ref lockTaken);

                foreach (var workItem in workItems)
                {
                    this.workItems.Enqueue(workItem);
                }

                var preferredWorkerCount = workItems.Count + activeThreadCount + 1;
                var newWorkerCount = Math.Min(preferredWorkerCount - workers.Count, Environment.ProcessorCount * 2);

                while (newWorkerCount-- > 0)
                {
                    var worker = Task.Factory.StartNew(ProcessWorkItems, workers.Count, TaskCreationOptions.LongRunning);
                    workers.Add(worker);
                    Console.WriteLine($"Thread {workers.Count} added");
                }

                workAvailable.Set();
            }
            finally
            {
                if (lockTaken)
                    spinLock.Exit(true);
            }
        }

        private ThreadPool()
        {
        }      

        private void ProcessWorkItems(object state)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = $"Xenko Thread Pool {state}";

            while (true)
            {
                Action workItem = null;

                bool lockTaken = false;
                try
                {
                    spinLock.Enter(ref lockTaken);

                    if (workItems.Count > 0)
                    {
                        try
                        {
                            workItem = workItems.Dequeue();
                            Interlocked.Increment(ref activeThreadCount);
                        }
                        catch
                        {

                        }
                    }

                    if (workItems.Count > 0)
                    {
                        // If we didn't consume the last work item, kick off another worker
                        workAvailable.Set();
                    }
                }
                finally
                {
                    if (lockTaken)
                        spinLock.Exit(true);
                }

                if (workItem != null)
                {
                    try
                    {
                        //Interlocked.Increment(ref activeThreadCount);
                        workItem.Invoke();
                    }
                    catch (Exception e)
                    {

                    }
                    finally
                    {
                        Interlocked.Decrement(ref activeThreadCount);
                    }
                }

                // Wait for another work item to be (potentially) available
                workAvailable.WaitOne();
            }
        }
    }
}
