// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Threading.Tasks;

namespace SiliconStudio.Core
{
    /// <summary>
    /// An interface allowing to dispose an object asynchronously.
    /// </summary>
    public interface IAsyncDisposable
    {
        /// <summary>
        /// Disposes the given instance asynchronously.
        /// </summary>
        /// <returns>A task that completes when this instance has been disposed.</returns>
        Task DisposeAsync();
    }
}
