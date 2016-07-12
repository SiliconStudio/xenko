using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SiliconStudio.Core.Threading
{
    public class ConcurrentPool<T> where T : class
    {
        private readonly ConcurrentBag<T> items;
        private readonly Func<T> factory;

        public ConcurrentPool(Func<T> factory)
        {
            this.factory = factory;
            items = new ConcurrentBag<T>();
        }

        public ConcurrentPool(IEnumerable<T> collection, Func<T> factory)
        {
            items = new ConcurrentBag<T>(collection);
            this.factory = factory;
        }

        public T Acquire()
        {
            T result;
            if (!items.TryTake(out result))
            {
                result = factory();
            }
            return result;
        }

        public void Release(T item)
        {
            items.Add(item);
        }
    }
}