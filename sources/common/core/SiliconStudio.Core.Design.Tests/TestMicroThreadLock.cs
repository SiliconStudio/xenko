using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.MicroThreading;

namespace SiliconStudio.Core.Design.Tests
{
    [TestFixture]
    public class TestMicroThreadLock
    {
        const int ThreadCount = 50;
        const int IncrementCount = 20;

        [Test, Timeout(1000)]
        public void TestConcurrencyInMicrothreads()
        {
            var scheduler = new Scheduler();
            var microThreadLock = new MicroThreadLock();
            var counter = 0;
            for (var j = 0; j < ThreadCount; ++j)
            {
                var microThread = scheduler.Create();
                microThread.Start(async () =>
                {
                    using (await microThreadLock.LockAsync())
                    {
                        var initialValue = counter;
                        for (var i = 0; i < IncrementCount; ++i)
                        {
                            Assert.AreEqual(initialValue + i, counter);
                            await Task.Yield();
                            ++counter;
                        }
                    }
                });
            }
            while (scheduler.MicroThreads.Count > 0)
            {
                scheduler.Run();
            }
            Assert.AreEqual(ThreadCount * IncrementCount, counter);
        }

        [Test, Timeout(1000)]
        public void TestReentrancyInMicrothreads()
        {
            var scheduler = new Scheduler();
            var microThreadLock = new MicroThreadLock();
            var counter = 0;
            for (var j = 0; j < ThreadCount; ++j)
            {
                var microThread = scheduler.Create();
                microThread.Start(async () =>
                {
                    using (await microThreadLock.LockAsync())
                    {
                        var initialValue = counter;
                        using (await microThreadLock.LockAsync())
                        {
                            for (var i = 0; i < IncrementCount; ++i)
                            {
                                using (await microThreadLock.LockAsync())
                                {
                                    using (await microThreadLock.LockAsync())
                                    {
                                        Assert.AreEqual(initialValue + i, counter);
                                    }
                                    using (await microThreadLock.LockAsync())
                                    {
                                        await Task.Yield();
                                    }
                                    using (await microThreadLock.LockAsync())
                                    {
                                        ++counter;
                                    }
                                }
                            }
                        }
                    }
                });
            }
            while (scheduler.MicroThreads.Count > 0)
            {
                scheduler.Run();
            }
            Assert.AreEqual(ThreadCount * IncrementCount, counter);
        }

        [Test, Timeout(1000)]
        public void TestConcurrencyInThreads()
        {
            var microThreadLock = new MicroThreadLock();
            var counter = 0;
            var threads = new List<Thread>();
            for (var j = 0; j < ThreadCount; ++j)
            {
                var thread = new Thread(() =>
                {
                    var sc = new TestSynchronizationContext();
                    SynchronizationContext.SetSynchronizationContext(sc);
                    sc.Post(async x =>
                    {
                        using (await microThreadLock.LockSync())
                        {
                            var initialValue = counter;
                            for (var i = 0; i < IncrementCount; ++i)
                            {
                                Assert.AreEqual(initialValue + i, counter);
                                Thread.Sleep(1);
                                ++counter;
                            }
                        }
                        sc.SignalEnd();
                    }, null);
                    sc.RunUntilEnd();
                }) { Name = $"Thread {j}" };
                thread.Start();
                threads.Add(thread);
            }
            threads.ForEach(x => x.Join());
            Assert.AreEqual(ThreadCount * IncrementCount, counter);
        }

        [Test, Timeout(1000)]
        public void TestReentrancyInThreads()
        {
            var microThreadLock = new MicroThreadLock();
            var counter = 0;
            var threads = new List<Thread>();
            for (var j = 0; j < ThreadCount; ++j)
            {
                var thread = new Thread(() =>
                {
                    var sc = new TestSynchronizationContext();
                    SynchronizationContext.SetSynchronizationContext(sc);
                    sc.Post(async x =>
                    {
                        using (await microThreadLock.LockSync())
                        {
                            var initialValue = counter;
                            using (await microThreadLock.LockSync())
                            {
                                for (var i = 0; i < IncrementCount; ++i)
                                {
                                    using (await microThreadLock.LockSync())
                                    {
                                        Assert.AreEqual(initialValue + i, counter);
                                    }
                                    using (await microThreadLock.LockSync())
                                    {
                                        Thread.Sleep(1);
                                    }
                                    using (await microThreadLock.LockSync())
                                    {
                                        ++counter;
                                    }
                                }
                            }
                        }
                        sc.SignalEnd();
                    }, null);
                    sc.RunUntilEnd();
                }) { Name = $"Thread {j}" };
                thread.Start();
                threads.Add(thread);
            }
            threads.ForEach(x => x.Join());
            Assert.AreEqual(ThreadCount * IncrementCount, counter);
        }

        [Test, Timeout(1000)]
        public void TestConcurrencyInThreadsAndMicrothreads()
        {
            var scheduler = new Scheduler();
            var microThreadLock = new MicroThreadLock();
            var counter = 0;
            for (var j = 0; j < ThreadCount; ++j)
            {
                var microThread = scheduler.Create();
                microThread.Start(async () =>
                {
                    using (await microThreadLock.LockAsync())
                    {
                        var initialValue = counter;
                        for (var i = 0; i < IncrementCount; ++i)
                        {
                            Assert.AreEqual(initialValue + i, counter);
                            await Task.Yield();
                            ++counter;
                        }
                    }
                });
            }
            var threads = new List<Thread>();
            for (var j = 0; j < ThreadCount; ++j)
            {
                var thread = new Thread(() =>
                {
                    var sc = new TestSynchronizationContext();
                    SynchronizationContext.SetSynchronizationContext(sc);
                    sc.Post(async x =>
                    {
                        using (await microThreadLock.LockSync())
                        {
                            var initialValue = counter;
                            for (var i = 0; i < IncrementCount; ++i)
                            {
                                Assert.AreEqual(initialValue + i, counter);
                                Thread.Sleep(1);
                                ++counter;
                            }
                        }
                        sc.SignalEnd();
                    }, null);
                    sc.RunUntilEnd();
                })
                { Name = $"Thread {j}" };
                thread.Start();
                threads.Add(thread);
            }
            while (scheduler.MicroThreads.Count > 0)
            {
                scheduler.Run();
            }
            threads.ForEach(x => x.Join());
            Assert.AreEqual(2 * ThreadCount * IncrementCount, counter);
        }


        /// <summary>
        /// A very basic dispatcher implementation for our unit tests.
        /// </summary>
        private class TestSynchronizationContext : SynchronizationContext
        {
            private readonly List<Tuple<SendOrPostCallback, object>> continuations = new List<Tuple<SendOrPostCallback, object>>();
            private bool ended;

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (continuations)
                {
                    continuations.Add(Tuple.Create(d, state));
                }
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException();
            }

            public void RunUntilEnd()
            {
                while (!ended)
                {
                    List<Tuple<SendOrPostCallback, object>> localCopy;
                    lock (continuations)
                    {
                        localCopy = continuations.ToList();
                        continuations.Clear();
                    }
                    foreach (var continuation in localCopy)
                    {
                        continuation.Item1.Invoke(continuation.Item2);
                    }
                    Thread.Sleep(1);
                }
            }

            public void SignalEnd() => ended = true;
        }
    }
}
