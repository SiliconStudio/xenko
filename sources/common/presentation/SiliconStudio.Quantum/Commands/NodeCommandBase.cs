using SiliconStudio.ActionStack;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Commands
{
    /// <summary>
    /// Base class for node commands.
    /// </summary>
    public abstract class NodeCommandBase : INodeCommand
    {
        public struct TokenData
        {
            public readonly object Parameter;
            public readonly UndoToken Token;

            public TokenData(object parameter, UndoToken token)
            {
                Parameter = parameter;
                Token = token;
            }
        }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public abstract CombineMode CombineMode { get; }

        /// <inheritdoc/>
        public abstract bool CanAttach(ITypeDescriptor typeDescriptor, MemberDescriptorBase memberDescriptor);

        public abstract object Execute(object currentValue, object parameter, out UndoToken undoToken);

        public abstract object Undo(object currentValue, UndoToken undoToken, out RedoToken redoToken);

        public abstract object Redo(object currentValue, RedoToken redoToken, out UndoToken undoToken);

        /// <inheritdoc/>
        public virtual void StartCombinedInvoke()
        {
            // Intentionally do nothing
        }

        /// <inheritdoc/>
        public virtual void EndCombinedInvoke()
        {
            // Intentionally do nothing
        }
    }
}