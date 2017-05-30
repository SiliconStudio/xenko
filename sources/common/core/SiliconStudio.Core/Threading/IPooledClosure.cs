// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.Threading
{
    /// <summary>
    /// Interface implemented by pooled closure types through the AssemblyProcessor.
    /// Enables <see cref="PooledDelegateHelper"/> to keep closures and delegates alive.
    /// </summary>
    public interface IPooledClosure
    {
        void AddReference();

        void Release();
    }
}
