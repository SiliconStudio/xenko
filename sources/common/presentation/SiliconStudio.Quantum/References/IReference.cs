// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.References
{
    public interface IReference : IEquatable<IReference>
    {
        /// <summary>
        /// Gets the data object targeted by this reference, if available.
        /// </summary>
        object ObjectValue { get; }

        /// <summary>
        /// Gets the type of object targeted by this reference, if available.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets the index of this reference in its parent collection. If the reference is not in a collection, this will return <see cref="Reference.NotInCollection"/>.
        /// </summary>
        object Index { get; }

        /// <summary>
        /// Clear the reference, making it represent a null or empty object.
        /// </summary>
        void Clear();

        /// <summary>
        /// Refresh this reference and its nested references.
        /// </summary>
        void Refresh(object newObjectValue);
    }
}
