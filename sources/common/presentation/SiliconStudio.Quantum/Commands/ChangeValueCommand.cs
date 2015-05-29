using SiliconStudio.ActionStack;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Commands
{
    /// <summary>
    /// A <see cref="INodeCommand"/> abstract implementation that can be used for commands that simply intent to change the value of the associated node.
    /// This class will manage undo itself, creating a cancellable undo token only if the value returned by the command is different from the initial value.
    /// </summary>
    public abstract class ChangeValueCommand : NodeCommand
    {
        public sealed override object Invoke(object currentValue, object parameter, out UndoToken undoToken)
        {
            var newValue = ChangeValue(currentValue, parameter, false);
            undoToken = !Equals(newValue, currentValue) ? new UndoToken(true, currentValue) : new UndoToken(false);
            return newValue;
        }

        public sealed override object Undo(object currentValue, UndoToken undoToken)
        {
            return undoToken.TokenValue;
        }

        public sealed override object Redo(object currentValue, object parameter, out UndoToken undoToken)
        {
            var newValue = ChangeValue(currentValue, parameter, true);
            undoToken = !Equals(newValue, currentValue) ? new UndoToken(true, currentValue) : new UndoToken(false);
            return newValue;
        }

        protected abstract object ChangeValue(object currentValue, object parameter, bool isRedo);
    }
}