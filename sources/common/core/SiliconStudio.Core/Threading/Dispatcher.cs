using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Core.Threading
{
    public interface IRecycle
    {
        void Release(object item);
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Delegate)]
    public class PooledAttribute : Attribute
    {  
    }

    public class Dispatcher
    {
        private static readonly List<Action> batches = new List<Action>();
        private static ConcurrentPool<AutoResetEvent> events = new ConcurrentPool<AutoResetEvent>(() => new AutoResetEvent(false));

        private static readonly int MaxDregreeOfParallelism = Environment.ProcessorCount;

        [Pooled]
        public delegate void ForAction(int index);

        [Pooled]
        public delegate void ForAction<in TLocal>(int index, TLocal local);

        [Pooled]
        public delegate void ForEachAction<in T>(T item);

        [Pooled]
        public delegate void ForeachAction<in T, in TLocal>(T item, TLocal local);

        [Pooled]
        public delegate T BatchInitializer<out T>();

        [Pooled]
        public delegate void BatchFinalizer<in T>(T local);

        public static void For(int fromInclusive, int toExclusive, ForAction action)
        {
            using (Profile(action))
            {
                if (fromInclusive > toExclusive)
                {
                    var temp = fromInclusive;
                    fromInclusive = toExclusive + 1;
                    toExclusive = temp + 1;
                }

                var count = toExclusive - fromInclusive;
                if (count == 0)
                    return;

                if (MaxDregreeOfParallelism <= 1)
                {
                    ExecuteBatch(fromInclusive, toExclusive, action);
                }
                else
                {
                    var finished = events.Acquire();
                    int remainingBatcheCount = 0;

                    try
                    {
                        int batchCount = Math.Min(MaxDregreeOfParallelism, count);
                        int batchSize = (count + (batchCount - 1)) / batchCount;

                        // If there's more than one batch, kick off worker threads
                        bool isParallel = batchSize < toExclusive;
                        if (isParallel)
                        {
                            lock (batches)
                            {

                                int batchStartInclusive = fromInclusive + batchSize;
                                int batchEndExclusive = batchStartInclusive + batchSize;

                                while (batchStartInclusive < toExclusive)
                                {
                                    if (batchEndExclusive > toExclusive)
                                        batchEndExclusive = toExclusive;

                                    var start = batchStartInclusive;
                                    var end = batchEndExclusive;

                                    remainingBatcheCount++;
                                    batches.Add(() =>
                                    {
                                        ExecuteBatch(start, end, action);

                                        if (Interlocked.Decrement(ref remainingBatcheCount) == 0)
                                        {
                                            finished.Set();
                                        }
                                    });

                                    batchStartInclusive = batchEndExclusive;
                                    batchEndExclusive += batchSize;
                                }

                                ThreadPool.Instance.QueueWorkItems(batches);
                                batches.Clear();
                            }
                        }

                        // Execute some work synchroneuously
                        ExecuteBatch(fromInclusive, Math.Min(toExclusive, fromInclusive + batchSize), action);

                        // Wait for all workers to finish
                        if (isParallel)
                        {
                            finished.WaitOne();
                        }
                    }
                    finally
                    {
                        events.Release(finished);
                    }
                }
            }
        }

        public static void For<TLocal>(int fromInclusive, int toExclusive, BatchInitializer<TLocal> initializeLocal, ForAction<TLocal> action, BatchFinalizer<TLocal> finalizeLocal = null)
        {
            using (Profile(action))
            {
                if (fromInclusive > toExclusive)
                {
                    var temp = fromInclusive;
                    fromInclusive = toExclusive + 1;
                    toExclusive = temp + 1;
                }

                var count = toExclusive - fromInclusive;
                if (count == 0)
                    return;

                if (MaxDregreeOfParallelism <= 1)
                {
                    ExecuteBatch(fromInclusive, toExclusive, initializeLocal, action, finalizeLocal);
                }
                else
                {
                    var finished = events.Acquire();
                    int remainingBatcheCount = 0;

                    try
                    {
                        int batchCount = Math.Min(MaxDregreeOfParallelism, count);
                        int batchSize = (count + (batchCount - 1)) / batchCount;

                        // If there's more than one batch, kick off worker threads
                        bool isParallel = batchSize < toExclusive;
                        if (isParallel)
                        {
                            lock (batches)
                            {

                                int batchStartInclusive = fromInclusive + batchSize;
                                int batchEndExclusive = batchStartInclusive + batchSize;

                                while (batchStartInclusive < toExclusive)
                                {
                                    if (batchEndExclusive > toExclusive)
                                        batchEndExclusive = toExclusive;

                                    var start = batchStartInclusive;
                                    var end = batchEndExclusive;

                                    remainingBatcheCount++;
                                    batches.Add(() =>
                                    {
                                        ExecuteBatch(start, end, initializeLocal, action, finalizeLocal);

                                        if (Interlocked.Decrement(ref remainingBatcheCount) == 0)
                                        {
                                            finished.Set();
                                        }
                                    });

                                    batchStartInclusive = batchEndExclusive;
                                    batchEndExclusive += batchSize;
                                }

                                ThreadPool.Instance.QueueWorkItems(batches);
                                batches.Clear();
                            }
                        }

                        // Execute some work synchroneuously
                        ExecuteBatch(fromInclusive, Math.Min(toExclusive, fromInclusive + batchSize), initializeLocal, action, finalizeLocal);

                        // Wait for all workers to finish
                        if (isParallel)
                        {
                            finished.WaitOne();
                        }
                    }
                    finally
                    {
                        events.Release(finished);
                    }
                }
            }
        }
        
        public static void ForEach<TItem, TLocal>(IReadOnlyList<TItem> collection, BatchInitializer<TLocal> initializeLocal, ForeachAction<TItem, TLocal> action, BatchFinalizer<TLocal> finalizeLocal = null)
        {
            For(0, collection.Count, initializeLocal, (i, local) => action(collection[i], local), finalizeLocal);
        }

        public static void ForEach<T>(IReadOnlyList<T> collection, ForEachAction<T> action)
        {
            For(0, collection.Count, i => action(collection[i]));
        }

        public static void ForEach<T>(List<T> collection, ForEachAction<T> action)
        {
            For(0, collection.Count, i => action(collection[i]));
        }

        public static void ForEach<T>(FastCollection<T> collection, ForEachAction<T> action)
        {
            For(0, collection.Count, i => action(collection[i]));
        }

        public static void ForEach<T>(FastList<T> collection, ForEachAction<T> action)
        {
            For(0, collection.Count, i => action(collection[i]));
        }

        public static void ForEach<TKey, TValue>(Dictionary<TKey, TValue> collection, ForEachAction<KeyValuePair<TKey, TValue>> action)
        {
            if (collection.Count == 0)
                return;

            int batchCount = Math.Min(MaxDregreeOfParallelism, collection.Count);
            int batchSize = (collection.Count + (batchCount - 1)) / batchCount;
            batchCount = (collection.Count + (batchSize - 1)) / batchSize;

            var finished = events.Acquire();
            int remainingBatcheCount = batchCount - 1;

            try
            {
                if (batchCount > 1)
                {
                    lock (batches)
                    {
                        for (int batchIndex = 1; batchIndex < batchCount; batchIndex++)
                        {
                            int offset = batchIndex * batchSize;

                            batches.Add(() =>
                            {
                                ExecuteBatch(collection, offset, batchSize, action);

                                if (Interlocked.Decrement(ref remainingBatcheCount) == 0)
                                {
                                    finished.Set();
                                }
                            });
                        }

                        ThreadPool.Instance.QueueWorkItems(batches);
                        batches.Clear();
                    }
                }

                ExecuteBatch(collection, 0, batchSize, action);

                if (batchCount > 1)
                {
                    finished.WaitOne();
                }
            }
            finally
            {
                events.Release(finished);
            }
        }

        private static void ExecuteBatch(int fromInclusive, int toExclusive, ForAction action)
        {
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                action(i);
            }
        }

        private static void ExecuteBatch<TLocal>(int fromInclusive, int toExclusive, BatchInitializer<TLocal> initializeLocal, ForAction<TLocal> action, BatchFinalizer<TLocal> finalizeLocal)
        {
            TLocal local = default(TLocal);
            try
            {
                if (initializeLocal != null)
                {
                    local = initializeLocal();
                }

                for (int i = fromInclusive; i < toExclusive; i++)
                {
                    action(i, local);
                }
            }
            finally
            {
                finalizeLocal?.Invoke(local);
            }
        }

        private static void ExecuteBatch<TKey, TValue>(Dictionary<TKey, TValue> dictionary, int offset, int count, ForEachAction<KeyValuePair<TKey, TValue>> action)
        {
            var enumerator = dictionary.GetEnumerator();
            int index = 0;

            // Skip to offset
            while (index < offset && enumerator.MoveNext())
            {
                index++;
            }

            // Process batch
            while (index < offset + count && enumerator.MoveNext())
            {
                action(enumerator.Current);
                index++;
            }
        }

        public static void Sort<T>(ConcurrentCollector<T> collection, IComparer<T> comparer)
        {
            Sort(collection.Items, 0, collection.Count, comparer);
        }

        public static void Sort<T>(FastList<T> collection, IComparer<T> comparer)
        {
            Sort(collection.Items, 0, collection.Count, comparer);
        }

        public static void Sort<T>(T[] collection, int index, int length, IComparer<T> comparer)
        {
            int depth = 0;
            int degreeOfParallelism = MaxDregreeOfParallelism;

            while ((degreeOfParallelism >>= 1) != 0)
                depth++;

            Sort(collection, index, length - 1, depth, comparer);
        }

        private static void Sort<T>(T[] collection, int left, int right, int depth, IComparer<T> comparer)
        {
            const int sequentialThreshold = 2048;

            if (right > left)
            {
                if (depth == 0 || right - left < sequentialThreshold)
                {
                    Array.Sort(collection, left, right - left + 1, comparer);
                }
                else
                {
                    int pivot = Partition(collection, left, right, comparer);

                    using (var finishedEvent = new ManualResetEvent(false))
                    {
                        ThreadPool.Instance.QueueWorkItem(() =>
                        {
                            Sort(collection, left, pivot - 1, depth - 1, comparer);
                            finishedEvent.Set();
                        });

                        Sort(collection, pivot + 1, right, depth - 1, comparer);
                        finishedEvent.WaitOne();
                    }
                }
            }
        }

        private static int Partition<T>(T[] collection, int left, int right, IComparer<T> comparer)
        {
            int i = left, j = right;
            int mid = (left + right) / 2;

            if (comparer.Compare(collection[right], collection[left]) < 0)
                Swap(collection, left, right);
            if (comparer.Compare(collection[mid], collection[left]) < 0)
                Swap(collection, left, mid);
            if (comparer.Compare(collection[right], collection[mid]) < 0)
                Swap(collection, mid, right);

            while (i <= j)
            {
                var pivot = collection[mid];

                while (comparer.Compare(collection[i], pivot) < 0)
                {
                    i++;
                }

                while (comparer.Compare(collection[j], pivot) > 0)
                {
                    j--;
                }

                if (i <= j)
                {
                    Swap(collection, i++, j--);
                }
            }

            return mid;
        }

        private static void Swap<T>(T[] collection, int i, int j)
        {
            var temp = collection[i];
            collection[i] = collection[j];
            collection[j] = temp;
        }

        private class DispatcherNode
        {
            public MethodBase Caller;
            public int Count;
            public TimeSpan TotalTime;
        }

        private static ConcurrentDictionary<MethodInfo, DispatcherNode> nodes = new ConcurrentDictionary<MethodInfo, DispatcherNode>();

        private struct ProfilingScope : IDisposable
        {
#if false
            public Stopwatch Stopwatch;
            public Delegate Action;
#endif
            public void Dispose()
            {
#if false
                Stopwatch.Stop();
                var elapsed = Stopwatch.Elapsed;

                DispatcherNode node;
                if (!nodes.TryGetValue(Action.Method, out node))
                {
                    int skipFrames = 1;
                    MethodBase caller = null;

                    do
                    {
                        caller = new StackFrame(skipFrames++, true).GetMethod();
                    }
                    while (caller.DeclaringType == typeof(Dispatcher));
                    
                    node = nodes.GetOrAdd(Action.Method, key => new DispatcherNode());
                    node.Caller = caller;
                }

                node.Count++;
                node.TotalTime += elapsed;

                if (node.Count % 500 == 0)
                {
                    Console.WriteLine($"[{node.Count}] {node.Caller.DeclaringType.Name}.{node.Caller.Name}: {node.TotalTime.TotalMilliseconds / node.Count}");
                }
#endif
            }
        }

        private static ProfilingScope Profile(Delegate action)
        {
            var result = new ProfilingScope();
#if false
            result.Action = action;
            result.Stopwatch = new Stopwatch();
            result.Stopwatch.Start();
#endif
            return result;

        }
    }
}
