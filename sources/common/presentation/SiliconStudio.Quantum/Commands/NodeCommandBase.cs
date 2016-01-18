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

        public Task<IActionItem> Execute2(IContent content, object index, object parameter)
        {
            return Execute2(content, index, parameter, Enumerable.Empty<IDirtiable>());
        }

        public abstract Task<IActionItem> Execute2(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables);

        [Obsolete]
        public object Undo(object currentValue, UndoToken undoToken, out RedoToken redoToken)
        {
            var tokenData = (TokenData)undoToken.TokenValue;
            redoToken = new RedoToken(tokenData.Parameter);
            return tokenData.Token.TokenValue;
        }

        [Obsolete]
        public object Redo(object currentValue, RedoToken redoToken, out UndoToken undoToken)
        {
            undoToken = new UndoToken(false);
            return null;
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