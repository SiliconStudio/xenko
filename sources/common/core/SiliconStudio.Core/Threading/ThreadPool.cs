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

        private readonly int MaxThreadCount = Environment.ProcessorCount * 2;
        private readonly List<Task> workers = new List<Task>();
        private readonly Queue<Action> workItems = new Queue<Action>();
        private readonly ManualResetEvent workAvailable = new ManualResetEvent(false);

        private SpinLock spinLock = new SpinLock();
        private int activeThreadCount;

        public void QueueWorkItem(Action workItem)
        {
            bool lockTaken = false;
            try
            {
                spinLock.Enter(ref lockTaken);

                workItems.Enqueue(workItem);

                if (activeThreadCount + 1 >= workers.Count && workers.Count < MaxThreadCount)
                {
                    var worker = Task.Factory.StartNew(ProcessWorkItems, workers.Count, TaskCreationOptions.LongRunning);
                    workers.Add(worker);
                    //Console.WriteLine($"Thread {workers.Count} added");
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

                var newWorkerCount = Math.Min(activeThreadCount + workItems.Count, MaxThreadCount - workers.Count);

                while (newWorkerCount-- > 0)
                {
                    var worker = Task.Factory.StartNew(ProcessWorkItems, workers.Count, TaskCreationOptions.LongRunning);
                    workers.Add(worker);
                    //Console.WriteLine($"Thread {workers.Count} added");
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

            //var spinWait = new SpinWait();

            while (true)
            {
                Action workItem = null;

                //while (!spinWait.NextSpinWillYield)
                {
                    bool lockTaken = false;
                    try
                    {
                        spinLock.Enter(ref lockTaken);

                        if (workItems.Count > 0)
                        {
                            try
                            {
                                workItem = workItems.Dequeue();
                                //Interlocked.Increment(ref activeThreadCount);

                                if (workItems.Count == 0)
                                    workAvailable.Reset();
                            }
                            catch
                            {

                            }
                        }

                        //if (workItems.Count > 0)
                        //{
                        //    // If we didn't consume the last work item, kick off another worker
                        //    workAvailable.Set();
                        //}
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
                            Interlocked.Increment(ref activeThreadCount);
                            workItem.Invoke();

                            //spinWait.Reset();
                        }
                        catch (Exception e)
                        {

                        }
                        finally
                        {
                            Interlocked.Decrement(ref activeThreadCount);
                        }
                    }
                    else
                    {
                        //spinWait.SpinOnce();
                    }
                }

                // Wait for another work item to be (potentially) available
                workAvailable.WaitOne();
            }
        }
    }
}
