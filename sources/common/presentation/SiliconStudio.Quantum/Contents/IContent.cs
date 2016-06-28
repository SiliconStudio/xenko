// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// Content of a <see cref="IGraphNode"/>.
    /// </summary>
    public interface IContent
    {
        /// <summary>
        /// Gets the <see cref="IContentNode"/> owning this content.
        /// </summary>
        IContentNode OwnerNode { get; }

        /// <summary>
        /// Gets the expected type of <see cref="Value"/>.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Gets whether this content hold a primitive type value. If so, the node owning this content should have no children and modifying its value should not trigger any node refresh.
        /// </summary>
        /// <remarks>Types registered as primitive types in the <see cref="INodeBuilder"/> used to build this content are taken in account by this property.</remarks>
        bool IsPrimitive { get; }

        /// <summary>
        /// Gets or sets the type descriptor of this content
        /// </summary>
        ITypeDescriptor Descriptor { get; }

        /// <summary>
        /// Gets wheither this content holds a reference or is a direct value.
        /// </summary>
        bool IsReference { get; }

        /// <summary>
        /// Gets the reference hold by this content, if applicable.
        /// </summary>
        IReference Reference { get; }

        /// <summary>
        /// Gets all the indices in the value of this content, if it is a collection. Otherwise, this property returns null.
        /// </summary>
        IEnumerable<Index> Indices { get; }

        /// <summary>
        /// Raised before the <see cref="Value"/> of this content changes and before the <see cref="Changing"/> event is raised.
        /// </summary>
        event EventHandler<ContentChangeEventArgs> PrepareChange;

        /// <summary>
        /// Raised after the <see cref="Value"/> of this content has changed and after the <see cref="Changed"/> event is raised.
        /// </summary>
        event EventHandler<ContentChangeEventArgs> FinalizeChange;

        /// <summary>
        /// Raised just before the <see cref="Value"/> of this content changes.
        /// </summary>
        event EventHandler<ContentChangeEventArgs> Changing;

        /// <summary>
        /// Raised when the <see cref="Value"/> of this content has changed.
        /// </summary>
        event EventHandler<ContentChangeEventArgs> Changed;

        /// <summary>
        /// Retrieves the value of this content.
        /// </summary>
        object Retrieve();

        /// <summary>
        /// Retrieves the value of one of the item if this content, if it holds a collection.
        /// </summary>
        /// <param name="index">The index to use to retrieve the value.</param>
        object Retrieve(Index index);

        /// <summary>
        /// Updates the value of this content with the given value.
        /// </summary>
        /// <param name="newValue">The new value to set.</param>
        void Update(object newValue);

        /// <summary>
        /// Updates the value of this content at the given index with the given value.
        /// </summary>
        /// <param name="newValue">The new value to set.</param>
        /// <param name="index">The index where to update the value.</param>
        void Update(object newValue, Index index);

        /// <summary>
        /// Adds a new item to this content, assuming the content is a collection.
        /// </summary>
        /// <param name="newItem">The new item to add to the collection.</param>
        void Add(object newItem);

        /// <summary>
        /// Adds a new item at the given index to this content, assuming the content is a collection.
        /// </summary>
        /// <param name="newItem">The new item to add to the collection.</param>
        /// <param name="itemIndex">The index at which the new item must be added.</param>
        void Add(object newItem, Index itemIndex);

        /// <summary>
        /// Removes an item from this content, assuming the content is a collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="itemIndex">The index from which the item must be removed.</param>
        void Remove(object item, Index itemIndex);
    }
}
