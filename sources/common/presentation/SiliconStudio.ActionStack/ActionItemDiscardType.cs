// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// This enum describes how an action item is being discarded when the <see cref="ActionStack.ActionItemsDiscarded"/> event is raised.
    /// </summary>
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
        /// Item discarded because an undo/redo operation is currently in progress.
        /// </summary>
        UndoRedoInProgress,
    }
}