using System;
using System.Collections.Generic;
using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// The <see cref="IContentNode"/> interface represents a node in a Quantum object graph. This node can represent an object or a member of an object.
    /// The value behind the node can be accessed and modified with the <see cref="Content"/> property.
    /// </summary>
    public interface IContentNode
    {
        /// <summary>
        /// Gets or sets the node name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the <see cref="System.Guid"/>.
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// Gets the content of the <see cref="IContentNode"/>.
        /// </summary>
        IContent Content { get; }

        /// <summary>
        /// Gets the command collection.
        /// </summary>
        IReadOnlyCollection<INodeCommand> Commands { get; }

        /// <summary>
        /// Gets the child corresponding to the given name.
        /// </summary>
        /// <param name="name">The name of the child to retrieves.</param>
        /// <returns>The child corresponding to the given name.</returns>
        /// <exception cref="KeyNotFoundException">This node has no child that matches the given name.</exception>
        IContentNode this[string name] { get; }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        IContentNode Parent { get; }

        /// <summary>
        /// Gets the children collection.
        /// </summary>
        IReadOnlyCollection<IContentNode> Children { get; }

        /// <summary>
        /// Gets the target of this node, if this node contains a reference to another node. 
        /// </summary>
        /// <exception cref="InvalidOperationException">The node does not contain a reference to another node.</exception>
        IContentNode Target { get; }

        /// <summary>
        /// Gets the target of this node corresponding to the given index, if this node contains a sequence of references to some other nodes. 
        /// </summary>
        /// <exception cref="InvalidOperationException">The node does not contain a sequence of references to some other nodes.</exception>
        /// <exception cref="ArgumentException">The index is empty.</exception>
        /// <exception cref="KeyNotFoundException">The index does not exist.</exception>
        IContentNode IndexedTarget(Index index);

        /// <summary>
        /// Attempts to retrieve the child node of this <see cref="IContentNode"/> that matches the given name.
        /// </summary>
        /// <param name="name">The name of the child to retrieve.</param>
        /// <returns>The child node that matches the given name, or <c>null</c> if no child matches.</returns>
        IContentNode TryGetChild(string name);
    }
}
