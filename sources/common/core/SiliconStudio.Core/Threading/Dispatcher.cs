using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Core.Threading
{
    public interface IRecycle
    {
        void Release(object item);
    }

    public delegate void PooledAction();

    public delegate void PooledAction<in T>(T obj);
    
    public class Dispatcher
    {
        private const bool disableParallelization = false;

        //public void Invoke(PooledAction action)
        //{
        //    ThreadPool.Instance.QueueWorkItem(() =>
        //    {

        //    });
        //}

        public static void For2(int fromInclusive, int toExclusive, PooledAction<int> action)
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
                var finishedLock = new object();

                for (int i = 0; i < batchCount; i++)
                {
                    if (batchEndExclusive > toExclusive)
                        batchEndExclusive = toExclusive;

                    if (batchEndExclusive - batchStartInclusive <= 0)
                        break;

                    var start = batchStartInclusive;
                    var end = batchEndExclusive;

                    ThreadPool.Instance.QueueWorkItem(() =>
                    {
                        ExecuteBatch(start, end, action);

                        Interlocked.Increment(ref batchesProcessed);
                        if (batchesProcessed == batchCount)
                        {
                            lock (finishedLock)
                            {
                                Monitor.Pulse(finishedLock);
                            }
                        }
                    });

                    batchStartInclusive = batchEndExclusive;
                    batchEndExclusive += batchSize;
                }

                lock (finishedLock)
                {
                    if (batchesProcessed < batchCount)
                        Monitor.Wait(finishedLock);
                }
            }
        }

        //[MethodImpl(MethodImplOptions.NoInlining)]
        public static void For(int fromInclusive, int toExclusive, PooledAction<int> action)
        {
            //var stopwatch = new Stopwatch();
            //stopwatch.Start();

            var count = toExclusive - fromInclusive;
            if (count == 0)
                return;

            if (disableParallelization)
            {
                //Parallel.For(fromInclusive, toExclusive, i => action(i));
                ExecuteBatch(fromInclusive, toExclusive, action);
            }
            else
            {
                //Parallel.For(fromInclusive, toExclusive, i => action(i));
                //return;

                int batchCount = Math.Min(Environment.ProcessorCount, count);
                int batchSize = (count + (batchCount - 1)) / batchCount;

                var finishedLock = new BatchState { Count = batchCount };

                Fork(batchSize, toExclusive, fromInclusive, fromInclusive + batchSize, action, finishedLock);

                lock (finishedLock)
                {
                    if (finishedLock.Count > 0)
                        Monitor.Wait(finishedLock);
                }
            }

            //var elapsed = stopwatch.Elapsed;
            //stopwatch.Stop();

            //var caller = new StackFrame(1, true).GetMethod();
            //if (caller.Name == "ForEach")
            //    caller = new StackFrame(2, true).GetMethod();

            //Console.WriteLine($"{caller.DeclaringType.Name}.{caller.Name}: {(float)elapsed.Ticks / (toExclusive - fromInclusive)}");
        }

        public static void ForEach<T>(PooledAction<T> action, MoveNextDelegate<T> tryMoveNext)
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

                lock (finishedLock)
                {
                    if (finishedLock.Count > 0)
                        Monitor.Wait(finishedLock);
                }
            }
        }

        private static void Fork<T>(int batchCount, PooledAction<T> action, MoveNextDelegate<T> tryMoveNext, BatchState batchState)
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
                lock (batchState)
                {
                    Monitor.Pulse(batchState);
                }
            }
        }

        private class BatchState
        {
            public int Count;
        }

        private static void Fork(int batchSize, int toExclusive, int batchStartInclusive, int batchEndExclusive, PooledAction<int> action, BatchState batchState)
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
                lock (batchState)
                {
                    Monitor.Pulse(batchState);
                }
            }
        }

        public static void ForEach<T>(IReadOnlyList<T> collection, PooledAction<T> action)
        {
            For(0, collection.Count, i => action(collection[i]));
        }

        public static void ForEach<T>(List<T> collection, PooledAction<T> action)
        {
            For(0, collection.Count, i => action(collection[i]));
        }

        public static void ForEach<T>(FastCollection<T> collection, PooledAction<T> action)
        {
            For(0, collection.Count, i => action(collection[i]));
        }

        public static void ForEach<T>(FastList<T> collection, PooledAction<T> action)
        {
            For(0, collection.Count, i => action(collection[i]));
        }

        public static void ForEach<TKey, TValue>(Dictionary<TKey, TValue> collection, PooledAction<KeyValuePair<TKey, TValue>> action)
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

        private static void ExecuteBatch(int fromInclusive, int toExclusive, PooledAction<int> action)
        {
            var step = toExclusive - fromInclusive < 0 ? -1 : 1;
            for (int i = fromInclusive; i < toExclusive; i += step)
            {
                action(i);
            }
        }

        public static void Sort<T>(ConcurrentCollector<T> collection, IComparer<T> comparer)
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
