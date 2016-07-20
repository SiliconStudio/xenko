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
        private readonly Semaphore workerThreadNeeded = new Semaphore();
        private int activeThreadCount;
        private SpinLock spinLock = new SpinLock();

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
                }
            }
            finally
            {
                if (lockTaken)
                    spinLock.Exit(false);
            }

            //lock (workItems)
            //{
            //    workItems.Enqueue(workItem);

            //    if (activeThreadCount + 1 >= workers.Count && workers.Count < Environment.ProcessorCount * 2)
            //    {
            //        var worker = Task.Factory.StartNew(ProcessWorkItems, workers.Count, TaskCreationOptions.LongRunning);
            //        workers.Add(worker);
            //    }
            //}

            //Console.WriteLine($"{DateTime.UtcNow.Ticks / 10000.0}: Adding");
            workerThreadNeeded.AddOne();
            //Console.WriteLine($"{DateTime.UtcNow.Ticks / 10000.0}: Added");
        }

        private ThreadPool()
        {
            //for (int i = 0; i < Environment.ProcessorCount * 2; i++)
            //{
            //    var worker = Task.Factory.StartNew(ProcessWorkItems, i, TaskCreationOptions.LongRunning);
            //    workers.Add(worker);
            //}
        }      

        private void ProcessWorkItems(object state)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = $"Xenko Thread Pool {state}";

            while (true)
            {
                workerThreadNeeded.WaitOne();
                //Console.WriteLine($"{DateTime.UtcNow.Ticks / 10000.0}: Finished waiting");

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
                        }
                        catch
                        {

                        }
                    }
                }
                finally
                {
                    if (lockTaken)
                        spinLock.Exit(false);
                }

                //lock (workItems)
                //{
                //    if (workItems.Count > 0)
                //    {
                //        try
                //        {
                //            workItem = workItems.Dequeue();
                //        }
                //        catch
                //        {
                            
                //        }
                //    }
                //}

                if (workItem != null)
                {
                    try
                    {
                        Interlocked.Increment(ref activeThreadCount);
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
            }
        }
    }
}
