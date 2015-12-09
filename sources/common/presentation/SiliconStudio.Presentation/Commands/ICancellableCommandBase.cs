using SiliconStudio.ActionStack;

namespace SiliconStudio.Presentation.Commands
{
    public interface ICancellableCommandBase : ICommandBase
    {
        string Name { get; }

        RedoToken Undo(UndoToken undoToken);

        UndoToken Redo(RedoToken redoToken);
    }
}