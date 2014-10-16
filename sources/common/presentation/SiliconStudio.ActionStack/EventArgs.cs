using System;

namespace SiliconStudio.ActionStack
{
    public enum ActionItemDiscardType
    {
        /// <summary>
        /// Item discarded because the stack is full.
        /// </summary>
        Swallowed,
        /// <summary>
        /// Item discarded because it has been undone and new action have been done since.
        /// </summary>
        Disbranched,
        /// <summary>
        /// Item discarded by the application either because they are not relevant or not actually cancellable.
        /// </summary>
        DiscardedByApplication
    }

    public class DiscardedActionItemsEventArgs<T> : ActionItemsEventArgs<T> where T : IActionItem
    {
        public ActionItemDiscardType Type { get; private set; }

        public DiscardedActionItemsEventArgs(ActionItemDiscardType type, T actionItems)
            : base(actionItems)
        {
            Type = type;
        }

        public DiscardedActionItemsEventArgs(ActionItemDiscardType type, T[] actionItems)
            : base(actionItems)
        {
            Type = type;
        }
    }

    public class ActionItemsEventArgs<T> : EventArgs where T : IActionItem
    {
        public T[] ActionItems { get; private set; }

        public ActionItemsEventArgs(T actionItems)
            : this(new[] { actionItems })
        {
        }

        public ActionItemsEventArgs(T[] actionItems)
        {
            if (actionItems == null)
                throw new ArgumentNullException("actionItems");

            ActionItems = actionItems;
        }
    }

    public class ActionItemEventArgs<T> : EventArgs where T : class, IActionItem
    {
        public T ActionItem { get; private set; }

        public ActionItemEventArgs(T actionItem)
        {
            if (actionItem == null)
                throw new ArgumentNullException("actionItem");

            ActionItem = actionItem;
        }
    }
}
