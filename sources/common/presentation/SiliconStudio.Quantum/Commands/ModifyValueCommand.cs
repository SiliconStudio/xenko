using SiliconStudio.ActionStack;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Commands
{
    // TODO: This class is temporary to workaround the cancellation of commands that just modify the value of the node
    public abstract class ModifyValueCommand : NodeCommand
    {
        public sealed override object Invoke(object currentValue, ITypeDescriptor descriptor, object parameter, out UndoToken undoToken)
        {
            var newValue = ModifyValue(currentValue, descriptor, parameter);
            undoToken = !Equals(newValue, currentValue) ? new UndoToken(true, currentValue) : new UndoToken(false);
            return newValue;
        }

        public sealed override object Undo(object currentValue, ITypeDescriptor descriptor, UndoToken undoToken)
        {
            return undoToken.TokenValue;
        }

        protected abstract object ModifyValue(object currentValue, ITypeDescriptor descriptor, object parameter);
    }
}