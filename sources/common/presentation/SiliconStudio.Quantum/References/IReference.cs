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
        /// Gets the index of this reference in its parent collection. If the reference is not in a collection, this will return <see cref="Index.Empty"/>.
        /// </summary>
        Index Index { get; }

        /// <summary>
        /// Gets this object casted as a <see cref="ObjectReference"/>.
        /// </summary>
        ObjectReference AsObject { get; }

        /// <summary>
        /// Gets this object casted as a <see cref="ReferenceEnumerable"/>.
        /// </summary>
        ReferenceEnumerable AsEnumerable { get; }

        /// <summary>
        /// Indicates whether the reference contains the given index.
        /// </summary>
        /// <param name="index">The index to check.</param>
        /// <returns><c>True</c> if the reference contains the given index, <c>False</c> otherwise.</returns>
        /// <remarks>If it is an <see cref="ObjectReference"/> it will return true only for <c>null</c>.</remarks>
        bool HasIndex(Index index);

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
