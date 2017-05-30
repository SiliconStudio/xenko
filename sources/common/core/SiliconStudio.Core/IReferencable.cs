// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core
{
    /// <summary>
    /// Base interface for all referencable objects.
    /// </summary>
    public interface IReferencable
    {
        /// <summary>
        /// Gets the reference count of this instance.
        /// </summary>
        /// <value>
        /// The reference count.
        /// </value>
        int ReferenceCount { get; }

        /// <summary>
        /// Increments the reference count of this instance.
        /// </summary>
        /// <returns>The method returns the new reference count.</returns>
        int AddReference();

        /// <summary>
        /// Decrements the reference count of this instance.
        /// </summary>
        /// <returns>The method returns the new reference count.</returns>
        /// <remarks>When the reference count is going to 0, the component should release/dispose dependents objects.</remarks>
        int Release();
    }
}
