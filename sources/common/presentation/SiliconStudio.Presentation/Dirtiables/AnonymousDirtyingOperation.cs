using System;
using System.Collections.Generic;

namespace SiliconStudio.Presentation.Dirtiables
{
    public class AnonymousDirtyingOperation : DirtyingOperation
    {
        private Action undo;
        private Action redo;

        public AnonymousDirtyingOperation(IEnumerable<IDirtiable> dirtiables, Action undo, Action redo)
            : base(dirtiables)
        {
            this.undo = undo;
            this.redo = redo;
        }

        /// <inheritdoc/>
        protected override void FreezeContent()
        {
            undo = null;
            redo = null;
        }

        /// <inheritdoc/>
        protected override void Undo()
        {
            undo?.Invoke();
        }

        /// <inheritdoc/>
        protected override void Redo()
        {
            redo?.Invoke();
        }
    }
}
