// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.ActionStack
{
    /// <summary>
    /// An implementation of the <see cref="DirtiableActionItem"/> that uses delegate for undo and redo.
    /// </summary>
    public class AnonymousActionItem : DirtiableActionItem
    {
        private Action undo;
        private Action redo;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousActionItem"/> class.
        /// </summary>
        /// <param name="name">The name of the action item.</param>
        /// <param name="dirtiables">The dirtiable objects associated to this action item.</param>
        /// <param name="undo">The <see cref="Action"/> to invoke on undo.</param>
        /// <param name="redo">The <see cref="Action"/> to invoke on redo.</param>
        public AnonymousActionItem(string name, IEnumerable<IDirtiable> dirtiables, Action undo, Action redo)
            : base(name, dirtiables)
        {
            this.undo = undo;
            this.redo = redo;
        }

        /// <inheritdoc/>
        protected override void FreezeMembers()
        {
            undo = null;
            redo = null;
        }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
            undo?.Invoke();
        }

        /// <inheritdoc/>
        protected override void RedoAction()
        {
            redo?.Invoke();
        }
    }
}