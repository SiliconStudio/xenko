using System;
using System.Threading;

namespace SiliconStudio.Core.Threading
{
    public class Semaphore
    {
        private readonly object semaphoreLock = new object();
        private int count;

        public Semaphore() : this(1)
        {
        }
        public Semaphore(int count)
        {
            if (count < 0)
            {
                throw new ArgumentException("Semaphore must have a count of at least 0.", nameof(count));
            }

            this.count = count;
        }

        public void WaitOne()
        {
            lock (semaphoreLock)
            {
                while (count <= 0)
                {
                    Monitor.Wait(semaphoreLock, Timeout.Infinite);
                }
                count--;
            }
        }

        public void AddOne()
        {
            lock (semaphoreLock)
            {
                count++;
                Monitor.Pulse(semaphoreLock);
            }
        }

        public void Reset(int count)
        {
            lock (semaphoreLock)
            {
                this.count = count;
            }
        }
    }
}