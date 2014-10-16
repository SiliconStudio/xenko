// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.Quantum.Legacy
{
    /// <summary>
    /// The <see cref="IViewModelNode"/> interface represents a node in a view model. A view model is represented by a graph of nodes, where
    /// each node is wrapping a <see cref="Content"/>. The implementation of <see cref="IContent"/> that is used defines how the
    /// the value behind the node can be fetched, or modified.
    /// </summary>
    public interface IViewModelNode
    {
        /// <summary>
        /// Gets or sets the node name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Guid"/>.
        /// </summary>
        Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        IViewModelNode Parent { get; set; }

        /// <summary>
        /// Gets the children collection.
        /// </summary>
        // TODO: change this to an IEnumerable to prevent adding children from the interface
        // TODO: rename this Members
        IList<IViewModelNode> Children { get; }

        /// <summary>
        /// Gets the content of the <see cref="IViewModelNode"/>.
        /// </summary>
        IContent Content { get; }
    }
}