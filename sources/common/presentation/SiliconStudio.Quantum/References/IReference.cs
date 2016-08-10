// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Quantum;

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
        /// Refreshes this reference by making point to the proper target nodes. If no node exists yet for some of the target, they will be created
        /// using the given node factory.
        /// </summary>
        /// <param name="ownerNode">The node owning this reference.</param>
        /// <param name="nodeContainer">The node container containing the <paramref name="ownerNode"/> and the target nodes.</param>
        /// <param name="nodeFactory">The factory to use to create missing target nodes.</param>
        void Refresh(IGraphNode ownerNode, NodeContainer nodeContainer, NodeFactoryDelegate nodeFactory);

        /// <summary>
        /// Enumerates all <see cref="ObjectReference"/> contained in the reference.
        /// </summary>
        /// <returns>A sequence containing all <see cref="ObjectReference"/> containined in this reference.</returns>
        /// <remarks>
        /// If this reference is an <see cref="ObjectReference"/>, the returned enumerable will contain the reference itself.
        /// If this reference is a <see cref="ReferenceEnumerable"/>, the returned enumerable will be <c>this</c>.
        /// </remarks>
        IEnumerable<ObjectReference> Enumerate();
    }
}
