// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.ActionStack;

namespace SiliconStudio.Presentation.ViewModel.ActionStack
{
    /// <summary>
    /// An implementation of the <see cref="DirtiableActionItem"/> that uses delegate for undo and redo.
    /// </summary>
    public class AnonymousViewModelActionItem : DirtiableActionItem
    {
        private readonly Action undo;
        private readonly Action redo;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousViewModelActionItem"/> class.
        /// </summary>
        /// <param name="name">The name of the action item.</param>
        /// <param name="dirtiables">The dirtiable objects associated to this action item.</param>
        /// <param name="undo">The <see cref="Action"/> to invoke on undo.</param>
        /// <param name="redo">The <see cref="Action"/> to invoke on redo.</param>
        public AnonymousViewModelActionItem(string name, IEnumerable<IDirtiable> dirtiables, Action undo, Action redo)
            : base(name, dirtiables)
        {
            if (undo == null) throw new ArgumentNullException(nameof(undo));
            if (redo == null) throw new ArgumentNullException(nameof(redo));
            this.undo = undo;
            this.redo = redo;
        }

        /// <inheritdoc/>
        protected override void FreezeMembers()
        {
        }

        /// <inheritdoc/>
        protected override void UndoAction()
        {
            undo();
        }

        /// <inheritdoc/>
        protected override void RedoAction()
        {
            redo();
        }
    }
}