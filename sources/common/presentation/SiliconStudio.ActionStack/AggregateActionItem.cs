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
        private readonly IActionItem[] actionItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateActionItem"/> class with the given collection of action items.
        /// </summary>
        /// <param name="actionItems">The action items to add to this aggregation.</param>
        [Obsolete("Use constructor that includes a name argument")]
        public AggregateActionItem(params IActionItem[] actionItems)
            : this(null, actionItems)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateActionItem"/> class with the given name and collection of action items.
        /// </summary>
        /// <param name="name">The name of this action item.</param>
        /// <param name="actionItems">The action items to add to this aggregation.</param>
        public AggregateActionItem(string name, params IActionItem[] actionItems)
            : base(name)
        {
            if (actionItems == null) throw new ArgumentNullException(nameof(actionItems));
            if (actionItems.Length == 0) throw new ArgumentException(@"At least one action item must be passed to an AggregateAcitonItem", nameof(actionItems));
            if (actionItems.Any(x => x == null)) throw new ArgumentException(@"actionItems cannot contain null item.", nameof(actionItems));
            this.actionItems = actionItems;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IActionItem> ActionItems => actionItems;

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
            foreach (var actionItem in actionItems)
                actionItem.Freeze();
        }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
            for (var i = actionItems.Length - 1; i >= 0; --i)
                actionItems[i].Undo();
        }

        /// <inheritdoc/>
        protected override void RedoAction()
        {
            foreach (var actionItem in ActionItems)
                actionItem.Redo();
        }
    }
}
