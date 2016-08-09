using System;

namespace SiliconStudio.Core.Threading
{
    internal static class PooledDelegateHelper
    {
        public static void AddReference(Delegate pooledDelegate)
        {
            var closure = pooledDelegate.Target as IPooledClosure;
            closure?.AddReference();
        }

        public static void Release(Delegate pooledDelegate)
        {
            var closure = pooledDelegate.Target as IPooledClosure;
            closure?.Release();
        }

        public static PooledDelegateScope AddScropedReference(Delegate pooledDelegate)
        {
            AddReference(pooledDelegate);
            return new PooledDelegateScope(pooledDelegate);
        }

        public struct PooledDelegateScope : IDisposable
        {
            private readonly Delegate pooledDelegate;

            public PooledDelegateScope(Delegate pooledDelegate)
            {
                this.pooledDelegate = pooledDelegate;
            }

            public void Dispose()
            {
                Release(pooledDelegate);
            }
        }
    }
}