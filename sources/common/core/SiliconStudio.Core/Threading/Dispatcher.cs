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

    [AttributeUsage(AttributeTargets.Parameter)]
    public class PooledAttribute : Attribute
    {
        
    }

    public class Dispatcher
    {
        private class DispatcherNode
        {
            public MethodBase Caller;
            public int Count;
            public TimeSpan TotalTime;
        }

        private static ConcurrentDictionary<object, DispatcherNode> nodes = new ConcurrentDictionary<object, DispatcherNode>();

        private const bool disableParallelization = false;

        public static void For2(int fromInclusive, int toExclusive, [Pooled] Action<int> action)
        {
            var count = toExclusive - fromInclusive;
            if (count == 0)
                return;

            if (disableParallelization)
            {
                ExecuteBatch(fromInclusive, toExclusive, action);
            }
            else
            {
                int batchCount = Math.Min(Environment.ProcessorCount, count);
                int batchSize = (count + (batchCount - 1)) / batchCount;

                int batchStartInclusive = fromInclusive;
                int batchEndExclusive = batchStartInclusive + batchSize;

                int batchesProcessed = 0;
                var finished = new ManualResetEvent(false);
                var batches = new List<Action>();

                int remainingBatcheCount = 0;
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

                        Interlocked.Decrement(ref remainingBatcheCount);
                        if (remainingBatcheCount == 0)
                        {
                            finished.Set();
                        }
                    });

                    batchStartInclusive = batchEndExclusive;
                    batchEndExclusive += batchSize;
                }

                ThreadPool.Instance.QueueWorkItems(batches);
                //foreach (var batch in batches)
                //{
                //    ThreadPool.Instance.QueueWorkItem(batch);
                //}

                finished.WaitOne();
            }
        }

        public static void For<TLocal>(int fromInclusive, int toExclusive, [Pooled] Func<TLocal> initializeLocal, Action<int, TLocal> action, [Pooled] Action<TLocal> finalizeLocal = null)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (fromInclusive > toExclusive)
            {
                var temp = fromInclusive;
                fromInclusive = toExclusive + 1;
                toExclusive = temp + 1;
            }

            var count = toExclusive - fromInclusive;
            if (count == 0)
                return;

            if (disableParallelization)
            {
                ExecuteBatch(fromInclusive, toExclusive, initializeLocal, action, finalizeLocal);
            }
            else
            {
                //Parallel.For(fromInclusive, toExclusive, i => action(i));
                //return;

                int batchCount = Math.Min(Environment.ProcessorCount, count);
                int batchSize = (count + (batchCount - 1)) / batchCount;
                batchCount = (count + (batchSize - 1)) / batchSize;

                var finishedLock = new BatchState { Count = batchCount };

                Fork(batchSize, toExclusive, fromInclusive, fromInclusive + batchSize, initializeLocal, action, finalizeLocal, finishedLock);

                finishedLock.Finished.WaitOne();
            }

            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;

            DispatcherNode node;
            if (!nodes.TryGetValue(action.Method, out node))
            {
                var caller = new StackFrame(1, true).GetMethod();
                //if (caller.Name == "ForEach")
                //    caller = new StackFrame(2, true).GetMethod();

                node = nodes.GetOrAdd(action.Method, key => new DispatcherNode());
                node.Caller = caller;
            }

            node.Count++;
            node.TotalTime += elapsed;

            if (node.Count % 500 == 0)
            {
                //Console.WriteLine($"[{node.Count}] {node.Caller.DeclaringType.Name}.{node.Caller.Name}: {node.TotalTime.TotalMilliseconds / node.Count}");
            }
        }

        public static void For(int fromInclusive, int toExclusive, [Pooled] Action<int> action)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (fromInclusive > toExclusive)
            {
                var temp = fromInclusive;
                fromInclusive = toExclusive + 1;
                toExclusive = temp + 1;
            }

            var count = toExclusive - fromInclusive;
            if (count == 0)
                return;

            if (disableParallelization)
            {
                ExecuteBatch(fromInclusive, toExclusive, action);
            }
            else
            {
                //Parallel.For(fromInclusive, toExclusive, i => action(i));
                //return;

                int batchCount = Math.Min(Environment.ProcessorCount, count);
                int batchSize = (count + (batchCount - 1)) / batchCount;
                batchCount = (count + (batchSize - 1)) / batchSize;

                var finishedLock = new BatchState { Count = batchCount };

                Fork(batchSize, toExclusive, fromInclusive, fromInclusive + batchSize, action, finishedLock);

                finishedLock.Finished.WaitOne();
            }

            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;

            DispatcherNode node;
            if (!nodes.TryGetValue(action.Method, out node))
            {
                var caller = new StackFrame(1, true).GetMethod();
                if (caller.Name == "ForEach")
                    caller = new StackFrame(2, true).GetMethod();

                node = nodes.GetOrAdd(action.Method, key => new DispatcherNode());
                node.Caller = caller;
            }

            node.Count++;
            node.TotalTime += elapsed;

            if (node.Count % 500 == 0)
            {
                //Console.WriteLine($"[{node.Count}] {node.Caller.DeclaringType.Name}.{node.Caller.Name}: {node.TotalTime.TotalMilliseconds / node.Count}");
            }
        }

        public static void ForEach<T>([Pooled] Action<T> action, MoveNextDelegate<T> tryMoveNext)
        {
            if (disableParallelization)
            {
                T value;
                while (tryMoveNext(out value))
                    action(value);
            }
            else
            {
                int batchCount = Environment.ProcessorCount;

                var finishedLock = new BatchState { Count = batchCount };

                Fork(batchCount, action, tryMoveNext, finishedLock);

                finishedLock.Finished.WaitOne();
            }
        }

        private static void Fork<T>(int batchCount, [Pooled] Action<T> action, MoveNextDelegate<T> tryMoveNext, BatchState batchState)
        {
            if (--batchCount > 0)
            {
                ThreadPool.Instance.QueueWorkItem(() => Fork(batchCount, action, tryMoveNext, batchState));
            }

            T item;
            while (tryMoveNext(out item))
            {
                action(item);
            }

            if (Interlocked.Decrement(ref batchState.Count) == 0)
            {
                batchState.Finished.Set();
            }
        }

        private class BatchState
        {
            public int Count;
            public ManualResetEvent Finished = new ManualResetEvent(false);
        }

        private static void Fork(int batchSize, int toExclusive, int batchStartInclusive, int batchEndExclusive, [Pooled] Action<int> action, BatchState batchState)
        {
            var start = batchStartInclusive;
            var end = batchEndExclusive;

            batchStartInclusive = batchEndExclusive;
            batchEndExclusive += batchSize;

            if (batchEndExclusive > toExclusive)
                batchEndExclusive = toExclusive;

            if (batchEndExclusive - batchStartInclusive > 0)
            {
                ThreadPool.Instance.QueueWorkItem(() => Fork(batchSize, toExclusive, batchStartInclusive, batchEndExclusive, action, batchState));
            }

            ExecuteBatch(start, end, action);

            if (Interlocked.Decrement(ref batchState.Count) == 0)
            {
                batchState.Finished.Set();
            }
        }

        private static void Fork<TLocal>(int batchSize, int toExclusive, int batchStartInclusive, int batchEndExclusive, [Pooled] Func<TLocal> initializeLocal, Action<int, TLocal> action, [Pooled] Action<TLocal> finalizeLocal, BatchState batchState)
        {
            var start = batchStartInclusive;
            var end = batchEndExclusive;

            batchStartInclusive = batchEndExclusive;
            batchEndExclusive += batchSize;

            if (batchEndExclusive > toExclusive)
                batchEndExclusive = toExclusive;

            if (batchEndExclusive - batchStartInclusive > 0)
            {
                ThreadPool.Instance.QueueWorkItem(() => Fork(batchSize, toExclusive, batchStartInclusive, batchEndExclusive, initializeLocal, action, finalizeLocal, batchState));
            }

            ExecuteBatch(start, end, initializeLocal, action, finalizeLocal);

            if (Interlocked.Decrement(ref batchState.Count) == 0)
            {
                batchState.Finished.Set();
            }
        }
        
        public static void ForEach<TItem, TLocal>(IReadOnlyList<TItem> collection, [Pooled] Func<TLocal> initializeLocal, Action<TItem, TLocal> action, [Pooled] Action<TLocal> finalizeLocal = null)
        {
            For(0, collection.Count, initializeLocal, (i, local) => action(collection[i], local), finalizeLocal);
        }

        public static void ForEach<T>(IReadOnlyList<T> collection, [Pooled] Action<T> action)
        {
            For(0, collection.Count, i => action(collection[i]));
        }

        public static void ForEach<T>(List<T> collection, [Pooled] Action<T> action)
        {
            For(0, collection.Count, i => action(collection[i]));
        }

        public static void ForEach<T>(FastCollection<T> collection, [Pooled]Action<T> action)
        {
            For(0, collection.Count, i => action(collection[i]));
        }

        public static void ForEach<T>(FastList<T> collection, [Pooled] Action<T> action)
        {
            For(0, collection.Count, i => action(collection[i]));
        }

        public static void ForEach<TKey, TValue>(Dictionary<TKey, TValue> collection, [Pooled] Action<KeyValuePair<TKey, TValue>> action)
        {
            var enumerator = collection.GetEnumerator();
            var spinLock = new SpinLock();

            ForEach(action, (out KeyValuePair<TKey, TValue> item) =>
            {
                bool lockTaken = false;
                try
                {
                    spinLock.Enter(ref lockTaken);

                    if (enumerator.MoveNext())
                    {
                        item = enumerator.Current;
                        return true;
                    }
                }
                finally
                {
                    if (lockTaken)
                        spinLock.Exit(false);
                }

                item = new KeyValuePair<TKey, TValue>();
                return false;
            });
        }

        public delegate bool MoveNextDelegate<T>(out T currentValue);

        private static void ExecuteBatch(int fromInclusive, int toExclusive, [Pooled] Action<int> action)
        {
            for (int i = fromInclusive; i < toExclusive; i++)
            {
                action(i);
            }
        }

        private static void ExecuteBatch<TLocal>(int fromInclusive, int toExclusive, [Pooled] Func<TLocal> initializeLocal, Action<int, TLocal> action, [Pooled] Action<TLocal> finalizeLocal)
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
            int degreeOfParallelism = Environment.ProcessorCount;

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
    }
}
