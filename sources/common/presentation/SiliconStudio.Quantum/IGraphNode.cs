// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// The <see cref="IGraphNode"/> interface extends the <see cref="IContentNode"/> by giving access to the <see cref="Parent"/> of this node
    /// representing the container object of its content, as well as its <see cref="Children"/> nodes representing the members of its content.
    /// </summary>
    public interface IGraphNode : IContentNode
    {
        /// <summary>
        /// Gets the child corresponding to the given name.
        /// </summary>
        /// <param name="name">The name of the child to retrieves.</param>
        /// <returns>The child corresponding to the given name.</returns>
        /// <exception cref="KeyNotFoundException">This node has no child that matches the given name.</exception>
        IGraphNode this[string name] { get; }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        IGraphNode Parent { get; }

        /// <summary>
        /// Gets the children collection.
        /// </summary>
        IReadOnlyCollection<IGraphNode> Children { get; }

        /// <summary>
        /// Gets the target of this node, if this node contains a reference to another node. 
        /// </summary>
        /// <exception cref="InvalidOperationException">The node does not contain a reference to another node.</exception>
        IGraphNode Target { get; }

        /// <summary>
        /// Gets the target of this node corresponding to the given index, if this node contains a sequence of references to some other nodes. 
        /// </summary>
        /// <exception cref="InvalidOperationException">The node does not contain a sequence of references to some other nodes.</exception>
        /// <exception cref="ArgumentException">The index is empty.</exception>
        /// <exception cref="KeyNotFoundException">The index does not exist.</exception>
        IGraphNode IndexedTarget(Index index);

        /// <summary>
        /// Attempts to retrieve the child node of this <see cref="IGraphNode"/> that matches the given name.
        /// </summary>
        /// <param name="name">The name of the child to retrieve.</param>
        /// <returns>The child node that matches the given name, or <c>null</c> if no child matches.</returns>
        IGraphNode TryGetChild(string name);
    }
}