// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Core
{
    /// <summary>
    /// Similar to the <see cref="System.IDisposable"/> but only deals with managed resources.
    /// </summary>
    /// <remarks>
    /// Class implementing both <see cref="IDestroyable"/> and <see cref="System.IDisposable"/> should call <see cref="Destroy"/>
    /// from the <see cref="System.IDisposable.Dispose"/> method when appropriate. 
    /// </remarks>
    public interface IDestroyable
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting managed resources.
        /// </summary>
        void Destroy();
    }
}
