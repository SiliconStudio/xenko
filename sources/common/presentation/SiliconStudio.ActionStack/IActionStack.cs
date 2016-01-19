// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// Base interface to for an action stack.
    /// </summary>
    public interface IActionStack
    {
        /// <summary>
        /// Gets the action items currently stored in the action stack, including undone items that have not been disbranched.
        /// </summary>
        IEnumerable<IActionItem> ActionItems { get; }

        /// <summary>
        /// Gets whether an undo operation can be executed.
        /// </summary>
        bool CanUndo { get; }

        /// <summary>
        /// Gets whether a redo operation can be executed.
        /// </summary>
        bool CanRedo { get; }

        /// <summary>
        /// Gets whether an undo or a redo operation is currently in progress.
        /// </summary>
        bool UndoRedoInProgress { get; }

        /// <summary>
        /// Raised whenever action items are added to the stack.
        /// </summary>
        event EventHandler<ActionItemsEventArgs<IActionItem>> ActionItemsAdded;

        /// <summary>
        /// Raised whenever the action stack is cleared.
        /// </summary>
        event EventHandler ActionItemsCleared;

        /// <summary>
        /// Raised whenever action items are discarded from the stack.
        /// </summary>
        event EventHandler<DiscardedActionItemsEventArgs<IActionItem>> ActionItemsDiscarded;

        /// <summary>
        /// Raised when an action item is undone.
        /// </summary>
        event EventHandler<ActionItemsEventArgs<IActionItem>> Undone;

        /// <summary>
        /// Raised when an action item is redone.
        /// </summary>
        event EventHandler<ActionItemsEventArgs<IActionItem>> Redone;

        /// <summary>
        /// Adds an action item to the stack. Discards any action item that is currently undone.
        /// </summary>
        /// <param name="item">The action item to add to the stack.</param>
        void Add(IActionItem item);

        /// <summary>
        /// Adds multiple action items on the stack. Discards any action item that is currently undone.
        /// </summary>
        /// <param name="items">The action items to add on the stack.</param>
        void AddRange(IEnumerable<IActionItem> items);

        /// <summary>
        /// Clears the action stack.
        /// </summary>
        void Clear();

        /// <summary>
        /// Creates a save point at the current index of the action stack.
        /// </summary>
        /// <param name="markActionsAsSaved">Indicate whether to set the <see cref="IActionItem.IsSaved"/> of all preceding action items to true.</param>
        /// <returns>A <see cref="SavePoint"/> object corresponding to the created save point.</returns>
        SavePoint CreateSavePoint(bool markActionsAsSaved);

        /// <summary>
        /// Undoes the last action item that is currently done.
        /// </summary>
        /// <returns><c>True</c> if an action could be undone, <c>False</c> otherwise.</returns>
        bool Undo();

        /// <summary>
        /// Redoes the first action item that is currently undone.
        /// </summary>
        /// <returns><c>True</c> if an action could be redone, <c>False</c> otherwise.</returns>
        bool Redo();
    }
}
