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

        /// <inheritdocs/>
        public IDisposable BeginEndTransaction(string name)
        {
            return new NullDisposable();
        }

        /// <inheritdocs/>
        public IDisposable BeginEndTransaction(Func<string> getName)
        {
            return new NullDisposable();
        }

        /// <inheritdocs/>
        public IDisposable BeginCancelTransaction()
        {
            return new NullDisposable();
        }

        /// <inheritdocs/>
        public IDisposable BeginDiscardTransaction()
        {
            return new NullDisposable();
        }

        /// <inheritdocs/>
        public void BeginTransaction()
        {
        }

        /// <inheritdocs/>
        public void EndTransaction(string displayName)
        {
        }

        /// <inheritdocs/>
        public void EndTransaction(string displayName, Func<IReadOnlyCollection<IActionItem>, IActionItem> aggregateActionItems)
        {
        }

        /// <inheritdocs/>
        public void CancelTransaction()
        {
        }

        /// <inheritdocs/>
        public void DiscardTransaction()
        {
        }

        /// <inheritdocs/>
        public IEnumerable<IActionItem> ActionItems
        {
            get { return Enumerable.Empty<IActionItem>(); }
        }

        // The following events are intentionally never invoked.
#pragma warning disable 67

        /// <inheritdocs/>
        public event EventHandler<ActionItemsEventArgs<IActionItem>> ActionItemsAdded;

        /// <inheritdocs/>
        public event EventHandler ActionItemsCleared;

        /// <inheritdocs/>
        public event EventHandler<DiscardedActionItemsEventArgs<IActionItem>> ActionItemsDiscarded;

        /// <inheritdocs/>
        public event EventHandler<ActionItemsEventArgs<IActionItem>> Undone;

        /// <inheritdocs/>
        public event EventHandler<ActionItemsEventArgs<IActionItem>> Redone;

#pragma warning restore 67

        /// <inheritdocs/>
        public void Add(IActionItem item)
        {           
        }

        /// <inheritdocs/>
        public void AddRange(IEnumerable<IActionItem> items)
        {
        }

        /// <inheritdocs/>
        public void Clear()
        {
        }

        /// <inheritdocs/>
        public SavePoint CreateSavePoint(bool markActionsAsSaved)
        {
            return SavePoint.Empty;
        }

        /// <inheritdocs/>
        public bool Undo()
        {
            return false;
        }

        /// <inheritdocs/>
        public bool Redo()
        {
            return false;
        }
    }
}
