using System;
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// The <see cref="IContentNode"/> interface represents a node in a Quantum object graph. This node can represent an object or a member of an object.
    /// </summary>
    public interface IContentNode
    {
        /// <summary>
        /// Gets or sets the <see cref="System.Guid"/>.
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// Gets the command collection.
        /// </summary>
        IReadOnlyCollection<INodeCommand> Commands { get; }

        /// <summary>
        /// Gets the target of this node corresponding to the given index, if this node contains a sequence of references to some other nodes. 
        /// </summary>
        /// <exception cref="InvalidOperationException">The node does not contain a sequence of references to some other nodes.</exception>
        /// <exception cref="ArgumentException">The index is empty.</exception>
        /// <exception cref="KeyNotFoundException">The index does not exist.</exception>
        IObjectNode IndexedTarget(Index index);

        /// <summary>
        /// Gets the expected type of <see cref="Value"/>.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets the value.
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

        ObjectReference TargetReference { get; }

        ReferenceEnumerable ItemReferences { get; }

        /// <summary>
        /// Gets all the indices in the value of this content, if it is a collection. Otherwise, this property returns null.
        /// </summary>
        IEnumerable<Index> Indices { get; }

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
