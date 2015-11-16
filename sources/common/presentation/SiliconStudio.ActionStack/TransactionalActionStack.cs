// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;

namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// This class is an implementation of the <see cref="ITransactionalActionStack"/> interface.
    /// </summary>
    /// <remarks>
    /// A transactional action stack is an action stack that generates <see cref="IAggregateActionItem"/> from the action items that are added after a
    /// transaction is started and before it is finished. A transaction can also be cancelled (undone) or discarded instead of creating an aggregate action item.
    /// Multiple transactions can be created at the same time, each transaction that ends will become a single item of the parent transaction in progress.
    /// </remarks>
    public class TransactionalActionStack : ActionStack, ITransactionalActionStack
    {
        /// <summary>
        /// The stack of transactions currently in progress.
        /// </summary>
        protected readonly Stack<List<IActionItem>> TransactionStack = new Stack<List<IActionItem>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionalActionStack"/> class with the given capacity.
        /// </summary>
        /// <param name="capacity">The stack capacity. If negative, the action stack will have an unlimited capacity.</param>
        public TransactionalActionStack(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionalActionStack"/> class with the given capacity and existing action items.
        /// </summary>
        /// <param name="capacity">The stack capacity. If negative, the action stack will have an unlimited capacity.</param>
        /// <param name="initialActionsItems">The action items to add to the stack.</param>
        public TransactionalActionStack(int capacity, IEnumerable<IActionItem> initialActionsItems)
            : base(capacity, initialActionsItems)
        {
        }

        /// <inheritdoc/>
        public bool TransactionInProgress => TransactionStack.Count > 0;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> TransactionStarted;

        /// <inheritdoc/>
        public event EventHandler<ActionItemsEventArgs<IActionItem>> TransactionEnded;

        /// <inheritdoc/>
        public event EventHandler<ActionItemsEventArgs<IActionItem>> TransactionCancelled;

        /// <inheritdoc/>
        public event EventHandler<ActionItemsEventArgs<IActionItem>> TransactionDiscarded;

        /// <inheritdoc/>
        public IDisposable BeginEndTransaction(string name)
        {
            BeginTransaction();
            return new AnonymousDisposable(() => EndTransaction(name));
        }

        /// <inheritdoc/>
        public IDisposable BeginEndTransaction(Func<string> getName)
        {
            BeginTransaction();
            return new AnonymousDisposable(() =>
            {
                string displayName;
                try
                {
                    displayName = getName();
                }
                catch
                {
                    displayName = "<<ERROR>>";
                }

                EndTransaction(displayName);
            });
        }

        /// <inheritdoc/>
        public IDisposable BeginCancelTransaction()
        {
            BeginTransaction();
            return new AnonymousDisposable(CancelTransaction);
        }

        /// <inheritdoc/>
        public IDisposable BeginDiscardTransaction()
        {
            BeginTransaction();
            return new AnonymousDisposable(DiscardTransaction);
        }

        /// <inheritdoc/>
        public virtual void BeginTransaction()
        {
            var currentTransaction = new List<IActionItem>();
            TransactionStack.Push(currentTransaction);
            TransactionStarted?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void EndTransaction(string name)
        {
            EndTransaction(name, x => AggregateActionItems(x, name));
        }

        /// <inheritdoc/>
        public virtual void EndTransaction(string name, Func<IReadOnlyCollection<IActionItem>, IActionItem> aggregateActionItems)
        {
            if (TransactionStack.Count == 0) throw new InvalidOperationException(Properties.ExceptionMessages.CannotEndNoTransactionInProgress);

            var currentTransaction = TransactionStack.Pop();
            if (currentTransaction.Count > 0)
            {
                var aggregateActionItem = aggregateActionItems(currentTransaction);
                Add(aggregateActionItem);
                TransactionEnded?.Invoke(this, new ActionItemsEventArgs<IActionItem>(aggregateActionItem));
            }
            else
            {
                TransactionDiscarded?.Invoke(this, new ActionItemsEventArgs<IActionItem>(new IActionItem[0]));
            }
        }

        /// <inheritdoc/>
        public virtual void CancelTransaction()
        {
            if (TransactionStack.Count == 0) throw new InvalidOperationException(Properties.ExceptionMessages.CannotEndNoTransactionInProgress);
            var currentTransaction = TransactionStack.Pop();
            foreach (IActionItem item in currentTransaction.Reverse<IActionItem>())
            {
                item.Undo();
            }
            TransactionCancelled?.Invoke(this, new ActionItemsEventArgs<IActionItem>(currentTransaction.ToArray()));
        }

        /// <inheritdoc/>
        public virtual void DiscardTransaction()
        {
            if (TransactionStack.Count == 0) throw new InvalidOperationException(Properties.ExceptionMessages.CannotEndNoTransactionInProgress);
            var actions = TransactionStack.Pop();
            TransactionDiscarded?.Invoke(this, new ActionItemsEventArgs<IActionItem>(actions.ToArray()));
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IActionItem> GetCurrentTransactions()
        {
            if (TransactionStack.Count == 0) throw new InvalidOperationException(Properties.ExceptionMessages.NoTransactionInProgress);
            return TransactionStack.Peek();
        }

        /// <inheritdoc/>
        public override void Add(IActionItem item)
        {
            if (!UndoRedoInProgress && TransactionStack.Count > 0)
            {
                var currentTransaction = TransactionStack.Peek();
                currentTransaction.Add(item);
            }
            else
            {
                base.Add(item);
            }
        }

        private static IActionItem AggregateActionItems(IReadOnlyCollection<IActionItem> actionItems, string name = null)
        {
            if (actionItems.Count == 1)
            {
                var actionItem = actionItems.First();
                if (!string.IsNullOrEmpty(name))
                    actionItem.Name = name;
                return actionItem;
            }
            return new AggregateActionItem(name, actionItems.ToArray());
        }
    }
}
