// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Extensions;

namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// An <see cref="ActionItem"/> that represents a set of multiple action items.
    /// </summary>
    public class AggregateActionItem : ActionItem, IAggregateActionItem
    {
        private readonly IEnumerable<IActionItem> actionItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateActionItem"/> class with the given collection of action items.
        /// </summary>
        /// <param name="actionItems">The action items to add to this aggregation.</param>
        public AggregateActionItem(IEnumerable<IActionItem> actionItems)
            : this(null, actionItems)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateActionItem"/> class with the given name and collection of action items.
        /// </summary>
        /// <param name="name">The name of this action item.</param>
        /// <param name="actionItems">The action items to add to this aggregation.</param>
        public AggregateActionItem(string name, IEnumerable<IActionItem> actionItems)
            : base(name)
        {
            if (actionItems == null) throw new ArgumentNullException("actionItems");
            this.actionItems = actionItems;
        }

        /// <inheritdoc/>
        public IEnumerable<IActionItem> ActionItems { get { return actionItems; } }
        
        /// <inheritdoc/>
        public override bool IsSaved { get { return ActionItems.All(x => x.IsSaved); } set { ActionItems.ForEach(x => x.IsSaved = value); } }

        /// <inheritdoc/>
        public bool ContainsAction(IActionItem actionItem)
        {
            return ActionItems.Any(x => x == actionItem || (x is IAggregateActionItem && ((IAggregateActionItem)x).ContainsAction(actionItem)));
        }

        /// <inheritdoc/>
        public IEnumerable<IActionItem> GetInnerActionItems()
        {
            yield return this;
            foreach (var actionItem in ActionItems)
            {
                var aggregateActionItem = actionItem as AggregateActionItem;
                if (aggregateActionItem != null)
                {
                    foreach (var subActionItem in aggregateActionItem.GetInnerActionItems())
                    {
                        yield return subActionItem;
                    }
                }
                else
                {
                    yield return actionItem;
                }
            }
        }

        /// <inheritdoc/>
        protected override void FreezeMembers()
        {
            foreach (var actionItem in actionItems.NotNull())
                actionItem.Freeze();
        }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
            foreach (var actionItem in ActionItems.Reverse().NotNull())
                actionItem.Undo();
        }

        /// <inheritdoc/>
        protected override void RedoAction()
        {
            foreach (var actionItem in ActionItems.NotNull())
                actionItem.Redo();
        }
    }
}
