using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Threading
{
    public class ConcurrentPool<T> where T : class
    {
        private readonly ConcurrentQueue<T> items;
        private readonly Func<T> factory;

        public ConcurrentPool(Func<T> factory)
        {
            this.factory = factory;
            items = new ConcurrentQueue<T>();
        }

        public ConcurrentPool([NotNull] IEnumerable<T> collection, Func<T> factory)
        {
            items = new ConcurrentQueue<T>(collection);
            this.factory = factory;
        }

        public T Acquire()
        {
            T result;
            if (!items.TryDequeue(out result))
            {
                result = factory();
            }
            return result;
        }

        public void Release(T item)
        {
            items.Enqueue(item);
        }
    }
}