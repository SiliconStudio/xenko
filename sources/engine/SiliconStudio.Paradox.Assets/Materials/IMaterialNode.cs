// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Base interface for all nodes in the material tree
    /// </summary>
    public interface IMaterialNode
    {
        /// <summary>
        /// Gets or sets a value indicating whether this node is reducible.
        /// </summary>
        /// <value><c>true</c> if this instance is reducible; otherwise, <c>false</c>.</value>
        bool IsReducible { get; set; }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="context">The context to get the children.</param>
        /// <returns>The list of children.</returns>
        IEnumerable<MaterialNodeEntry> GetChildren(object context = null);
    }
}