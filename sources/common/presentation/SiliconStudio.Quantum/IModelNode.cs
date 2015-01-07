// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// The <see cref="IModelNode"/> interface represents a node in a model object. A model object is represented by a graph of nodes, where
    /// each node is wrapping a <see cref="Content"/>. The implementation of <see cref="IContent"/> that is used defines how the
    /// the value behind the node can be fetched, or modified.
    /// </summary>
    public interface IModelNode
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
        /// Gets or sets the parent node.
        /// </summary>
        IModelNode Parent { get; }

        /// <summary>
        /// Gets the children collection.
        /// </summary>
        IReadOnlyCollection<IModelNode> Children { get; }

        /// <summary>
        /// Gets the content of the <see cref="IModelNode"/>.
        /// </summary>
        IContent Content { get; }

        /// <summary>
        /// Gets the command collection.
        /// </summary>
        IReadOnlyCollection<INodeCommand> Commands { get; }
    }
}