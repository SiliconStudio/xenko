// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// Base class for action stack events arguments.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IActionItem"/> related to this event.</typeparam>
    public class ActionItemsEventArgs<T> : EventArgs where T : class, IActionItem
    {
        /// <summary>
        /// Gets the array of <see cref="IActionItem"/> related to this event.
        /// </summary>
        public T[] ActionItems { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionItemsEventArgs{T}"/> with a single item.
        /// </summary>
        /// <param name="actionItem">The action item related to this event.</param>
        public ActionItemsEventArgs(T actionItem)
            : this(new[] { actionItem })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionItemsEventArgs{T}"/> with an array of action items.
        /// </summary>
        /// <param name="actionItems">The action items related to this event.</param>
        public ActionItemsEventArgs(T[] actionItems)
        {
            if (actionItems == null)
                throw new ArgumentNullException(nameof(actionItems));

            ActionItems = actionItems;
        }
    }
}