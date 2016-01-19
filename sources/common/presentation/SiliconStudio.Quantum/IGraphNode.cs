// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
        /// Gets or sets the parent node.
        /// </summary>
        IGraphNode Parent { get; }

        /// <summary>
        /// Gets the children collection.
        /// </summary>
        IReadOnlyCollection<IGraphNode> Children { get; }
    }
}