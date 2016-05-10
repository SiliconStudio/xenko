// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
