using System;
using System.Collections.Generic;

namespace SiliconStudio.Presentation.ViewModel.ActionStack
{
    /// <summary>
    /// An implementation of the <see cref="ViewModelActionItem"/> that uses delegate for undo and redo.
    /// </summary>
    public class AnonymousViewModelActionItem : ViewModelActionItem
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
        public AnonymousViewModelActionItem(string name, IEnumerable<IDirtiableViewModel> dirtiables, Action undo, Action redo)
            : base(name, dirtiables)
        {
            if (undo == null) throw new ArgumentNullException("undo");
            if (redo == null) throw new ArgumentNullException("redo");
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