// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// Base class for action items.
    /// </summary>
    /// <remarks>
    /// An ActionItem represents an action that can be undone or redone. Action items are usually stacked in an <see cref="IActionStack"/>.
    /// </remarks>
    public abstract class ActionItem : IActionItem
    {
        private bool undoRedoInProgress;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionItem"/> class with the given name.
        /// </summary>
        /// <param name="name">The name of this action item.</param>
        protected ActionItem(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionItem"/> class.
        /// </summary>
        protected ActionItem()
        {
        }

        /// <inheritdoc/>
        public Guid Identifier { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public virtual bool IsSaved { get; set; }

        /// <inheritdoc/>
        public virtual bool IsDone { get; protected set; } = true;

        /// <inheritdoc/>
        public bool IsFrozen { get; private set; }

        /// <inheritdoc/>
        public void Freeze()
        {
            if (!IsFrozen)
            {
                FreezeMembers();
                IsFrozen = true;
            }
        }
        
        /// <inheritdoc/>
        public void Undo()
        {
            if (undoRedoInProgress) throw new InvalidOperationException(string.Format(Properties.ExceptionMessages.InvokingUndoRedoWhileAlreadyInProgress, nameof(Undo)));
            if (IsFrozen) throw new InvalidOperationException(string.Format(Properties.ExceptionMessages.UndoRedoOnFrozenItem, nameof(Undo)));
            undoRedoInProgress = true;
            UndoAction();
            IsDone = false;
            undoRedoInProgress = false;
        }

        /// <inheritdoc/>
        public void Redo()
        {
            if (undoRedoInProgress) throw new InvalidOperationException(string.Format(Properties.ExceptionMessages.InvokingUndoRedoWhileAlreadyInProgress, nameof(Redo)));
            if (IsFrozen) throw new InvalidOperationException(string.Format(Properties.ExceptionMessages.UndoRedoOnFrozenItem, nameof(Redo)));
            undoRedoInProgress = true;
            RedoAction();
            IsDone = true;
            undoRedoInProgress = false;
        }

        /// <summary>
        /// Invoked by <see cref="Freeze"/> before setting <see cref="IsFrozen"/> to true.
        /// </summary>
        /// <remarks>This method will not be invoked if <see cref="IsFrozen"/> is already true.</remarks>
        protected abstract void FreezeMembers();

        /// <summary>
        /// Invoked by <see cref="Undo"/> after setting <see cref="IsDone"/> to true.
        /// </summary>
        protected abstract void UndoAction();

        /// <summary>
        /// Invoked by <see cref="Redo"/> after setting <see cref="IsDone"/> to true.
        /// </summary>
        protected abstract void RedoAction();
    }
}
