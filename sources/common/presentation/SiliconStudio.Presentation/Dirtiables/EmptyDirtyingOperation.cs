using System.Collections.Generic;

namespace SiliconStudio.Presentation.Dirtiables
{
    public sealed class EmptyDirtyingOperation : DirtyingOperation
    {
        public EmptyDirtyingOperation(IEnumerable<IDirtiable> dirtiables)
            : base(dirtiables)
        {
        }

        /// <inheritdoc/>
        protected override void Undo()
        {
        }

        /// <inheritdoc/>
        protected override void Redo()
        {
        }
    }
}