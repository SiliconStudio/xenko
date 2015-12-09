using System.Collections.Generic;
using SiliconStudio.ActionStack;
using SiliconStudio.Presentation.ViewModel;

namespace SiliconStudio.Presentation.Commands
{
    public abstract class SimpleCancellableCommand : CancellableCommandBase
    {
        private struct TokenData
        {
            public TokenData(object parameter, object tokenValue)
            {
                Parameter = parameter;
                TokenValue = tokenValue;
            }

            public readonly object Parameter;
            public readonly object TokenValue;
        }

        protected SimpleCancellableCommand(IViewModelServiceProvider serviceProvider, IEnumerable<IDirtiable> dirtiables)
            : base(serviceProvider, dirtiables)
        {
        }

        public override RedoToken Undo(UndoToken undoToken)
        {
            var token = (TokenData)undoToken.TokenValue;
            Undo(token.Parameter, token.TokenValue);
            return new RedoToken(token.Parameter);
        }

        public override UndoToken Redo(RedoToken redoToken)
        {
            var parameter = redoToken.TokenValue;
            var tokenValue = Redo(parameter);
            return new UndoToken(true, new TokenData(parameter, tokenValue));
        }

        protected sealed override UndoToken Do(object parameter)
        {
            var tokenValue = Redo(parameter);
            return new UndoToken(true, new TokenData(parameter, tokenValue));
        }

        protected abstract void Undo(object parameter, object undoTokenValue);

        protected abstract object Redo(object parameter);
    }
}