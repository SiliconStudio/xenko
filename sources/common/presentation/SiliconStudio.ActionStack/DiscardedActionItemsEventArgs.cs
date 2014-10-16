// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// The argument of the <see cref="ActionStack.ActionItemsDiscarded"/> event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DiscardedActionItemsEventArgs<T> : ActionItemsEventArgs<T> where T : class, IActionItem
    {
        /// <summary>
        /// Gets the conditions in which the action items have been discarded.
        /// </summary>
        public ActionItemDiscardType Type { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscardedActionItemsEventArgs{T}"/> class.
        /// </summary>
        /// <param name="type">The type of discard that has been done.</param>
        /// <param name="actionItems">The action items that have been discarded.</param>
        public DiscardedActionItemsEventArgs(ActionItemDiscardType type, T[] actionItems)
            : base(actionItems)
        {
            Type = type;
        }
    }
}
