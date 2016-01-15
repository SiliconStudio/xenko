using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.ActionStack;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

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

        public Task<ActionItem> Execute2(IContent content, object index, object parameter)
        {
            return Execute2(content, index, parameter, Enumerable.Empty<IDirtiable>());
        }

        public virtual Task<ActionItem> Execute2(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables)
        {
            return Task.FromResult<ActionItem>(null);
        }

        [Obsolete]
        public abstract object Undo(object currentValue, UndoToken undoToken, out RedoToken redoToken);

        [Obsolete]
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