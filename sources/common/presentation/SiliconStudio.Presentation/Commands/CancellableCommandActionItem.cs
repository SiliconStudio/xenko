using System.Collections.Generic;
using SiliconStudio.ActionStack;

namespace SiliconStudio.Presentation.Commands
{
    public class CancellableCommandActionItem : DirtiableActionItem
    {
        private UndoToken undoToken;
        private RedoToken redoToken;

        public CancellableCommandActionItem(ICancellableCommandBase command, UndoToken undoToken, IEnumerable<IDirtiable> dirtiables)
            : base("Executing " + command.Name, dirtiables)
        {
            Command = command;
            this.undoToken = undoToken;
        }

        public ICancellableCommandBase Command { get; private set; }

        protected override void FreezeMembers()
        {
            Command = null;
            undoToken = default(UndoToken);
            redoToken = default(RedoToken);
        }

        protected override void UndoAction()
        {
            redoToken = Command.Undo(undoToken);
            undoToken = default(UndoToken);
        }

        protected override void RedoAction()
        {
            undoToken = Command.Redo(redoToken);
            redoToken = default(RedoToken);
        }
    }
}