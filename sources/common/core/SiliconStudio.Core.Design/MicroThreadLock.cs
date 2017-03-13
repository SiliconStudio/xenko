using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SiliconStudio.Core.MicroThreading;

namespace SiliconStudio.Core
{
    /// <summary>
    /// An hybrid lock that allows to do asynchrounous work when acquired from a <see cref="MicroThread"/>, and still allow to await for acquisition out of a
    /// microthread. This lock support re-entrancy.
    /// </summary>
    public class MicroThreadLock : IDisposable
    {
        private readonly MicroThreadLocal<MicroThreadAsyncLock> asyncLocks = new MicroThreadLocal<MicroThreadAsyncLock>();
        private readonly ThreadLocal<MicroThreadSyncLock> syncLocks = new ThreadLocal<MicroThreadSyncLock>();
        private readonly Queue<MicroThreadLockBase> lockQueue = new Queue<MicroThreadLockBase>();
        private readonly object syncLock = new object();
        private bool isDisposed;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(MicroThreadLock));
            isDisposed = true;
        }

        /// <summary>
        /// Acquires a synchronous lock. The lock will use a standard <see cref="Monitor"/> and therefore should not be used for asynchronous work.
        /// </summary>
        /// <returns>A task that completes when the lock is acquired.</returns>
        /// <remarks>This way of acquiring the lock is only valid when not in a <see cref="MicroThread"/>.</remarks>
        public Task<IDisposable> LockSync()
        {
            if (Scheduler.CurrentMicroThread != null) throw new InvalidOperationException($"Synchronous lock cannot be acquired from a micro-thread. Use {nameof(LockAsync)}.");
            return Lock();
        }

        /// <summary>
        /// Acquires an asynchronous lock. The lock will be tied to the current <see cref="MicroThread"/> to allow re-entrancy.
        /// </summary>
        /// <returns>A task that completes when the lock is acquired.</returns>
        /// <remarks>This way of acquiring the lock is only valid when in a <see cref="MicroThread"/>.</remarks>
        public Task<IDisposable> LockAsync()
        {
            if (Scheduler.CurrentMicroThread == null) throw new InvalidOperationException($"Aynchronous lock can only be acquired from a micro-thread. Use {nameof(LockSync)}.");
            return Lock();
        }

        private async Task<IDisposable> Lock()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(MicroThreadLock));

            // Two cases here:
            // 1) We're in a micro-thread, we lock the database "asynchronously" using a MicroThreadLocal storage to handle re-entrancy, allowing awaits during the lock.
            // 2) We're not in a micro-thread, we lock the database "synchronously" using a ThreadLocal storage to handle re-entrancy and a standard Monitor.
            var inMicroThread = Scheduler.CurrentMicroThread != null;

            if (inMicroThread)
            {
                // If we already acquired the lock in this micro-thread, we're just re-entering
                if (asyncLocks.IsValueCreated)
                {
                    var currentLock = asyncLocks.Value;
                    currentLock.Reenter();
                    return currentLock;
                }
            }
            else
            {
                // If we already acquired the lock in this thread, we're just re-entering
                if (syncLocks.IsValueCreated)
                {
                    var currentLock = syncLocks.Value;
                    currentLock.Reenter();
                    return currentLock;
                }
            }

            // Select the proper type of lock depending on whether we're in a micro-thread or not.
            var newLock = inMicroThread ? (MicroThreadLockBase)new MicroThreadAsyncLock(this) : new MicroThreadSyncLock(this);
            lock (lockQueue)
            {
                if (lockQueue.Count == 0)
                {
                    // Nothing else is in the queue, let's acquire immediately.
                    newLock.Acquire();
                }
                // Let's enqueue this new lock so it can be notified by the previous lock when it can be acquired.
                lockQueue.Enqueue(newLock);
            }
            await newLock.Acquired;
            newLock.Enter();
            return newLock;
        }

        private abstract class MicroThreadLockBase : IDisposable
        {
            protected readonly MicroThreadLock MicroThreadLock;
            private readonly TaskCompletionSource<int> acquisition;
            private int reentrancy;

            protected MicroThreadLockBase(MicroThreadLock microThreadLock)
            {
                MicroThreadLock = microThreadLock;
                acquisition = new TaskCompletionSource<int>();
            }

            public Task Acquired => acquisition.Task;

            public virtual void Dispose()
            {
                if (reentrancy == 0)
                    throw new InvalidOperationException("Trying to dispose a lock that has already been released.");

                --reentrancy;
                if (reentrancy == 0)
                {
                    lock (MicroThreadLock.lockQueue)
                    {
                        // Remove ourself from the queue.
                        var thisLock = MicroThreadLock.lockQueue.Dequeue();
                        if (thisLock != this) throw new InvalidOperationException("The first lock in the queue was not the current lock");
                        // If another lock is waiting, let's acquire it
                        if (MicroThreadLock.lockQueue.Count > 0)
                        {
                            var nextLock = MicroThreadLock.lockQueue.Peek();
                            nextLock.Acquire();
                        }
                    }
                }
            }

            internal void Acquire()
            {
                acquisition.SetResult(0);
            }

            internal virtual void Enter()
            {
                if (reentrancy != 0) throw new InvalidOperationException("Trying to enter a lock that has already been entered");
                ++reentrancy;
                Register();
            }

            internal virtual void Reenter()
            {
                if (!acquisition.Task.IsCompleted) throw new InvalidOperationException("Trying to reenter a lock that has not yet been acquired");
                ++reentrancy;
            }

            internal abstract void Register();
        }

        private class MicroThreadAsyncLock : MicroThreadLockBase
        {
            public MicroThreadAsyncLock(MicroThreadLock microThreadLock)
                : base(microThreadLock)
            {
            }


            internal override void Register()
            {
                MicroThreadLock.asyncLocks.Value = this;
            }
        }

        private class MicroThreadSyncLock : MicroThreadLockBase
        {
            public MicroThreadSyncLock(MicroThreadLock microThreadLock)
                : base(microThreadLock)
            {
            }

            public override void Dispose()
            {
                Monitor.Exit(MicroThreadLock.syncLock);
                base.Dispose();
            }

            internal override void Enter()
            {
                Monitor.Enter(MicroThreadLock.syncLock);
                base.Enter();
            }
            internal override void Reenter()
            {
                Monitor.Enter(MicroThreadLock.syncLock);
                base.Reenter();
            }

            internal override void Register()
            {
                MicroThreadLock.syncLocks.Value = this;
            }
        }
    }
}
