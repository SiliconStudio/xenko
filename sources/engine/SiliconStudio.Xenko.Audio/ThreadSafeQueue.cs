// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

namespace SiliconStudio.Xenko.Audio
{
    /// <summary>
    /// Thread safe queue.
    /// </summary>
    /// <remarks>Fields <see cref="InternalQueue"/> and <see cref="InternalLock"/> can be used to perform specific optimizations. 
    /// In this case it is the responsibility of the user to ensure the proper use of the Queue.</remarks>
    /// <typeparam name="T"></typeparam>
    internal class ThreadSafeQueue<T>
    {
        private readonly List<T> cachedDequeueList = new List<T>();

        private readonly object internalLock = new object();
        public object InternalLock
        {
            get { return internalLock; }
        }

        private readonly Queue<T> internalQueue = new Queue<T>();
        public Queue<T> InternalQueue
        {
            get { return internalQueue; }
        }

        public int Count
        {
            get
            {
                lock (InternalLock)
                {
                    return internalQueue.Count;
                }
            }
        }

        public void Enqueue(T item)
        {
            lock (InternalLock)
            {
                internalQueue.Enqueue(item);
            }
        }

        public bool TryDequeue(out T result)
        {
            bool ret;

            result = default(T);

            lock (InternalLock)
            {
                ret = internalQueue.Count > 0;
                if (ret)
                    result = internalQueue.Dequeue();
            }

            return ret;
        }

        public List<T> DequeueAsList()
        {
            lock (InternalLock)
            {
                cachedDequeueList.Clear();

                while (internalQueue.Count > 0)
                    cachedDequeueList.Add(internalQueue.Dequeue());
                
                return cachedDequeueList;
            }
        }
    }
}
