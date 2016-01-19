// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// This class represents a thread-safe stack of action items that can be undone/redone.
    /// </summary>
    public class ActionStack : IActionStack
    {
        private readonly object lockObject = new object();
        private readonly List<IActionItem> actionItems = new List<IActionItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionStack"/> class with the given capacity.
        /// </summary>
        /// <param name="capacity">The stack capacity. If negative, the action stack will have an unlimited capacity.</param>
        public ActionStack(int capacity)
            : this(capacity, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionStack"/> class with the given capacity and existing action items.
        /// </summary>
        /// <param name="capacity">The stack capacity. If negative, the action stack will have an unlimited capacity.</param>
        /// <param name="initialActionsItems">The action items to add to the stack.</param>
        public ActionStack(int capacity, IEnumerable<IActionItem> initialActionsItems)
        {
            Capacity = capacity;

            if (initialActionsItems != null)
            {
                foreach (var originalActionItem in initialActionsItems)
                actionItems.Add(originalActionItem);
            }
            // setup inital index
            ResetIndexOnTop();
        }

        /// <inheritdoc/>
        public IEnumerable<IActionItem> ActionItems => actionItems;

        /// <summary>
        /// Gets the capacity of this action stack.
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// Gets whether an undo operation can be executed.
        /// </summary>
        public bool CanUndo => CurrentIndex > 0;

        /// <summary>
        /// Gets whether an redo operation can be executed.
        /// </summary>
        public bool CanRedo => CurrentIndex < actionItems.Count;

        /// <summary>
        /// Raised whenever action items are added to the stack.
        /// </summary>
        public event EventHandler<ActionItemsEventArgs<IActionItem>> ActionItemsAdded;

        /// <summary>
        /// Raised whenever the action stack is cleared.
        /// </summary>
        public event EventHandler ActionItemsCleared;

        /// <summary>
        /// Raised whenever action items are discarded from the stack.
        /// </summary>
        public event EventHandler<DiscardedActionItemsEventArgs<IActionItem>> ActionItemsDiscarded;

        /// <summary>
        /// Raised when an action item is undone.
        /// </summary>
        public event EventHandler<ActionItemsEventArgs<IActionItem>> Undone;

        /// <summary>
        /// Raised when an action item is redone.
        /// </summary>
        public event EventHandler<ActionItemsEventArgs<IActionItem>> Redone;

        /// <summary>
        /// Gets the index at which the next action item will be added.
        /// </summary>
        protected int CurrentIndex { get; private set; }

        /// <summary>
        /// Gets whether an undo/redo operation is currently in progress.
        /// </summary>
        public bool UndoRedoInProgress { get; private set; }

        /// <inheritdoc/>
        public virtual void Add(IActionItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var items = new[] { item };
            if (UndoRedoInProgress)
            {
                OnActionItemsDiscarded(new DiscardedActionItemsEventArgs<IActionItem>(ActionItemDiscardType.UndoRedoInProgress, items));
                return;
            }

            InternalAddRange(items);
        }

        /// <inheritdoc/>
        public void AddRange(IEnumerable<IActionItem> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var cachedItems = items.ToArray();
            if (cachedItems.Length == 0)
                return;

            InternalAddRange(cachedItems);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            InternalClear();
            OnActionItemsCleared();
        }

        /// <inheritdoc/>
        public virtual SavePoint CreateSavePoint(bool markActionsAsSaved)
        {
            lock (lockObject)
            {
                if (markActionsAsSaved)
                {
                    int i = 0;
                    foreach (var action in actionItems)
                    {
                        action.IsSaved = i++ < CurrentIndex;
                    }
                }

                return CanUndo ? new SavePoint(actionItems[CurrentIndex - 1].Identifier) : SavePoint.Empty;
            }
        }

        /// <inheritdoc/>
        public virtual bool Undo()
        {
            UndoRedoInProgress = true;
            try
            {
                IActionItem action;
                lock (lockObject)
                {
                    if (CanUndo == false)
                        return false;

                    action = actionItems[--CurrentIndex];
                    action.Undo();
                    // If the action is still done after the undo, reset the current index to its initial value.
                    if (action.IsDone)
                    {
                        ++CurrentIndex;
                        return false;
                    }
                }

                OnUndone(new ActionItemsEventArgs<IActionItem>(action));
                return true;
            }
            finally
            {
                UndoRedoInProgress = false;
            }
        }

        /// <inheritdoc/>
        public virtual bool Redo()
        {
            UndoRedoInProgress = true;
            try
            {
                IActionItem action;
                lock (lockObject)
                {
                    if (CanRedo == false)
                        return false;

                    action = actionItems[CurrentIndex++];
                    action.Redo();
                    // If the action is still done after the undo, reset the current index to its initial value.
                    if (!action.IsDone)
                    {
                        --CurrentIndex;
                        return false;
                    }
                }
                OnRedone(new ActionItemsEventArgs<IActionItem>(action));
                return true;
            }
            finally
            {
                UndoRedoInProgress = false;
            }
        }

        /// <summary>
        /// Invoked whenever action items are discarded from the stack.
        /// </summary>
        /// <param name="e">The arguments that will be passed to the <see cref="ActionItemsDiscarded"/> event raised by this method.</param>
        protected virtual void OnActionItemsDiscarded(DiscardedActionItemsEventArgs<IActionItem> e)
        {
            ActionItemsDiscarded?.Invoke(this, e);
        }

        /// <summary>
        /// Invoked whenever action items are added to the stack.
        /// </summary>
        /// <param name="e">The arguments that will be passed to the <see cref="ActionItemsAdded"/> event raised by this method.</param>
        protected virtual void OnActionItemsAdded(ActionItemsEventArgs<IActionItem> e)
        {
            ActionItemsAdded?.Invoke(this, e);
        }

        /// <summary>
        /// Invoked whenever the action stack is cleared.
        /// </summary>
        protected virtual void OnActionItemsCleared()
        {
            ActionItemsCleared?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Invoked Raised when an action item is undone.
        /// </summary>
        /// <param name="e">The arguments that will be passed to the <see cref="Undone"/> event raised by this method.</param>
        protected virtual void OnUndone(ActionItemsEventArgs<IActionItem> e)
        {
            Undone?.Invoke(this, e);
        }

        /// <summary>
        /// Invoked Raised when an action item is redone.
        /// </summary>
        /// <param name="e">The arguments that will be passed to the <see cref="Redone"/> event raised by this method.</param>
        protected virtual void OnRedone(ActionItemsEventArgs<IActionItem> e)
        {
            Redone?.Invoke(this, e);
        }
        
        private void InternalClear()
        {
            lock (lockObject)
            {
                actionItems.Clear();
                ResetIndexOnTop();
            }
        }

        private void InternalAddRange(IActionItem[] items)
        {
            lock (lockObject)
            {
                // stack pre-cleanup
                UnsafeDisbranchedCleanup();

                // add items
                foreach (var item in items)
                    actionItems.Add(item);

                // post-cleanup
                IActionItem[] discarded = null;
                if (Capacity >= 0 && actionItems.Count > Capacity)
                {
                    // stack is overloaded
                    discarded = actionItems
                        .Take(actionItems.Count - Capacity)
                        .ToArray();
                    int itemsToRemove = actionItems.Count - Capacity;
                    for (int i = 0; i < itemsToRemove; ++i)
                    {
                        actionItems[0].Freeze();
                        actionItems.RemoveAt(0);
                    }
                }

                ResetIndexOnTop();

                // raise event to notify of cleaned up items
                if (discarded != null)
                {
                    OnActionItemsDiscarded(new DiscardedActionItemsEventArgs<IActionItem>(
                        ActionItemDiscardType.Swallowed,
                        discarded));
                }

                // raise event to notify of added items
                var added = items;
                if (Capacity >= 0 && items.Length > Capacity)
                    added = items.Skip(items.Length - Capacity).ToArray();

                OnActionItemsAdded(new ActionItemsEventArgs<IActionItem>(added));
            }
        }

        private void UnsafeDisbranchedCleanup()
        {
            if (CurrentIndex < 0)
            {
                // the whole stack has been undone and index is back to the beginning
                var discarded = actionItems.ToArray();
                // clean the stack
                InternalClear();

                OnActionItemsDiscarded(new DiscardedActionItemsEventArgs<IActionItem>(ActionItemDiscardType.Disbranched, discarded));
            }
            else if (actionItems.Count - CurrentIndex > 0)
            {
                // the stack has been undone a bit and index points before the last item
                // copy the discarded items, from CurrentIndex to the action item count
                var discardedItems = actionItems.Skip(CurrentIndex).Take(actionItems.Count - CurrentIndex).ToArray();

                // remove items that are in range
                int itemsToRemove = actionItems.Count - CurrentIndex;
                for (int i = 0; i < itemsToRemove; ++i)
                    actionItems.RemoveAt(CurrentIndex);

                ResetIndexOnTop();

                OnActionItemsDiscarded(new DiscardedActionItemsEventArgs<IActionItem>(ActionItemDiscardType.Disbranched, discardedItems));
            }
        }

        private void ResetIndexOnTop()
        {
            CurrentIndex = actionItems.Count;
        }
    }
}
