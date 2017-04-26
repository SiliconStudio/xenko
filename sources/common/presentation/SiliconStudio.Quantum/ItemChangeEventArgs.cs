// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// Arguments of the <see cref="IObjectNode.ItemChanging"/> and <see cref="IObjectNode.ItemChanged"/> events.
    /// </summary>
    public class ItemChangeEventArgs : EventArgs, INodeChangeEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemChangeEventArgs"/> class.
        /// </summary>
        /// <param name="node">The node that has changed.</param>
        /// <param name="index">The index in the member where the change occurred.</param>
        /// <param name="changeType">The type of change that occurred.</param>
        /// <param name="oldValue">The old value of the item that has changed.</param>
        /// <param name="newValue">The new value of the item that has changed.</param>
        public ItemChangeEventArgs([NotNull] IObjectNode node, Index index, ContentChangeType changeType, object oldValue, object newValue)
        {
            Collection = node;
            Index = index;
            ChangeType = changeType;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the object node of the collection that has changed.
        /// </summary>
        [NotNull]
        public IObjectNode Collection { get; }

        /// <summary>
        /// Gets the index where the change occurred.
        /// </summary>
        public Index Index { get; }

        /// <summary>
        /// The type of change.
        /// </summary>
        public ContentChangeType ChangeType { get; }

        /// <summary>
        /// Gets the old value of the member or the item of the member that has changed.
        /// </summary>
        public object OldValue { get; }

        /// <summary>
        /// Gets the new value of the member or the item of the member that has changed.
        /// </summary>
        public object NewValue { get; }

        /// <inheritdoc/>
        IGraphNode INodeChangeEventArgs.Node => Collection;
    }
}
