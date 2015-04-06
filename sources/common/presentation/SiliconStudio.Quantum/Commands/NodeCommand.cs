using SiliconStudio.ActionStack;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Commands
{
    /// <summary>
    /// Base class for node commands.
    /// </summary>
    public abstract class NodeCommand : INodeCommand
    {
        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public abstract CombineMode CombineMode { get; }

        /// <inheritdoc/>
        public abstract bool CanAttach(ITypeDescriptor typeDescriptor, MemberDescriptorBase memberDescriptor);

        /// <inheritdoc/>
        public abstract object Invoke(object currentValue, object parameter, out UndoToken undoToken);
        
        /// <inheritdoc/>
        public abstract object Undo(object currentValue, UndoToken undoToken);

        /// <summary>
        /// Redoes the node command. The default implementation simply calls the <see cref="Invoke"/> method.
        /// </summary>
        /// <param name="currentValue">The current value of the associated object or member.</param>
        /// <param name="parameter">The parameter of the command.</param>
        /// <param name="undoToken">The <see cref="UndoToken"/> that will be passed to the <see cref="Undo"/> method when undoing the execution of this command.</param>
        /// <returns>The new value to assign to the associated object or member.</returns>
        public virtual object Redo(object currentValue, object parameter, out UndoToken undoToken)
        {
            return Invoke(currentValue, parameter, out undoToken);
        }

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