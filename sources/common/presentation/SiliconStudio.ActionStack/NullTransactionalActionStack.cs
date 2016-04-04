// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;

namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// An implementation of the <see cref="ITransactionalActionStack"/> that does not store any action item and does not trigger any event.
    /// </summary>
    /// <remarks>This class can be used when an action stack is required by some objects but you don't need the action stack yourself. Use the <see cref="Default"/> instance of this class.</remarks>
    public class NullTransactionalActionStack : ITransactionalActionStack
    {
        /// <summary>
        /// The single instance of the <see cref="NullTransactionalActionStack"/>.
        /// </summary>
        public static NullTransactionalActionStack Default = new NullTransactionalActionStack();

        /// <summary>
        /// Initializes a new instance of the <see cref="NullTransactionalActionStack"/> class.
        /// </summary>
        private NullTransactionalActionStack()
        {

        }

        /// <inheritdoc/>
        public bool TransactionInProgress => false;

        /// <inheritdoc/>
        public IEnumerable<IActionItem> ActionItems => Enumerable.Empty<IActionItem>();

        /// <inheritdoc/>
        public bool CanUndo => false;

        /// <inheritdoc/>
        public bool CanRedo => false;

        /// <inheritdoc/>
        public bool UndoRedoInProgress => false;

        // The following events are intentionally never invoked.
        // We provide an empty `add' and `remove' to avoid a warning about unused events that we have
        // to implement as they are part of the ITransactionalActionStack definition.
        /// <inheritdoc/>
        public event EventHandler<ActionItemsEventArgs<IActionItem>> ActionItemsAdded { add { } remove { } }

        /// <inheritdoc/>
        public event EventHandler ActionItemsCleared { add { } remove { } }

        /// <inheritdoc/>
        public event EventHandler<ActionItemsEventArgs<IActionItem>> TransactionCancelled { add { } remove { } }

        /// <inheritdoc/>
        public event EventHandler<ActionItemsEventArgs<IActionItem>> TransactionDiscarded { add { } remove { } }

        /// <inheritdoc/>
        public event EventHandler<DiscardedActionItemsEventArgs<IActionItem>> ActionItemsDiscarded { add { } remove { } }

        /// <inheritdoc/>
        public event EventHandler<ActionItemsEventArgs<IActionItem>> Undone { add { } remove { } }

        /// <inheritdoc/>
        public event EventHandler<ActionItemsEventArgs<IActionItem>> Redone { add { } remove { } }

        /// <inheritdoc/>
        public IDisposable BeginEndTransaction(string name)
        {
            return new NullDisposable();
        }

        /// <inheritdoc/>
        public IDisposable BeginEndTransaction(Func<string> getName)
        {
            return new NullDisposable();
        }

        /// <inheritdoc/>
        public IDisposable BeginDiscardTransaction()
        {
            return new NullDisposable();
        }

        /// <inheritdoc/>
        public void BeginTransaction()
        {
        }

        /// <inheritdoc/>
        public void EndTransaction(string displayName, bool reverseOrderOnUndo = true)
        {
        }

        /// <inheritdoc/>
        public void EndTransaction(string displayName, AggregateActionItemDelegate aggregateActionItems, bool reverseOrderOnUndo = true)
        {
        }

        /// <inheritdoc/>
        public void DiscardTransaction()
        {
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IActionItem> GetCurrentTransactions()
        {
            // Returns an empty list
            return ActionItems.ToList();
        }

        /// <inheritdoc/>
        public void Add(IActionItem item)
        {           
        }

        /// <inheritdoc/>
        public void AddRange(IEnumerable<IActionItem> items)
        {
        }

        /// <inheritdoc/>
        public void Clear()
        {
        }

        /// <inheritdoc/>
        public SavePoint CreateSavePoint(bool markActionsAsSaved)
        {
            return SavePoint.Empty;
        }

        /// <inheritdoc/>
        public bool Undo()
        {
            return false;
        }

        /// <inheritdoc/>
        public bool Redo()
        {
            return false;
        }
    }
}
