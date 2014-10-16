// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Assets.Visitors
{
    /// <summary>
    /// Interface providing a generic access to hierarchical data that contains members (property/fields) 
    /// and collection items (array item, list item, dictionary item).
    /// </summary>
    public interface IDataVisitNode<T> where T : IDataVisitNode<T>
    {
        /// <summary>
        /// Gets or sets the parent of this node.
        /// </summary>
        /// <value>The parent.</value>
        T Parent { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance has <see cref="Members"/>.
        /// </summary>
        /// <value><c>true</c> if this instance has members; otherwise, <c>false</c>.</value>
        bool HasMembers { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has <see cref="Items"/>.
        /// </summary>
        /// <value><c>true</c> if this instance has items; otherwise, <c>false</c>.</value>
        bool HasItems { get; }

        /// <summary>
        /// Gets the members. Can be null if no members.
        /// </summary>
        /// <value>The members.</value>
        List<T> Members { get; set; }

        /// <summary>
        /// Gets the items (array, list or dictionary items). Can be null if no items.
        /// </summary>
        /// <value>The items.</value>
        List<T> Items { get; set; }
    }
}