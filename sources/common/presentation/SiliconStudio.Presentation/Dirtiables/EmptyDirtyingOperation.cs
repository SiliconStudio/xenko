using System.Collections.Generic;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.Dirtiables
{
    public sealed class EmptyDirtyingOperation : DirtyingOperation
    {
        public EmptyDirtyingOperation([NotNull] IEnumerable<IDirtiable> dirtiables)
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