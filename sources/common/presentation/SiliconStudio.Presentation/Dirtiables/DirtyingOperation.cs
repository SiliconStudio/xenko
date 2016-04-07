using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Transactions;

namespace SiliconStudio.Presentation.Dirtiables
{
    public abstract class DirtyingOperation : Operation, IDirtyingOperation
    {
        protected DirtyingOperation(IEnumerable<IDirtiable> dirtiables)
        {
            if (dirtiables == null) throw new ArgumentNullException(nameof(dirtiables));
            IsDone = true;
            Dirtiables = dirtiables.ToList();
        }

        public bool IsDone { get; private set; }

        public IReadOnlyCollection<IDirtiable> Dirtiables { get; }

        protected sealed override void Rollback()
        {
            Undo();
            IsDone = false;
        }

        protected sealed override void Rollforward()
        {
            Redo();
            IsDone = true;
        }

        protected abstract void Undo();

        protected abstract void Redo();
    }
}
