// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.ActionStack;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    [Obsolete("This class will be replaced by its base class NodeCommandBase (or will be kept with a different usage)")]
    public abstract class SimpleNodeCommand : NodeCommandBase
    {
        [Obsolete]
        public sealed override object Execute(object currentValue, object parameter, out UndoToken undoToken)
        {
            UndoToken token;
            var newValue = Do(currentValue, parameter, out token);
            undoToken = new UndoToken(token.CanUndo, new TokenData(parameter, token));
            return newValue;
        }

        public sealed override Task<IActionItem> Execute2(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables)
        {
            var actionItem = CreateActionItem(content, index, parameter, dirtiables);
            return actionItem?.Do() ?? false ? Task.FromResult<IActionItem>(actionItem) : null;
        }

        // TODO: this method must be abstract.
        protected virtual SimpleNodeCommandActionItem CreateActionItem(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables)
        {
            return null;
        }

        [Obsolete]
        public sealed override object Undo(object currentValue, UndoToken undoToken, out RedoToken redoToken)
        {
            var tokenData = (TokenData)undoToken.TokenValue;
            redoToken = new RedoToken(tokenData.Parameter);
            return Undo(currentValue, tokenData.Token);
        }

        [Obsolete]
        public sealed override object Redo(object currentValue, RedoToken redoToken, out UndoToken undoToken)
        {
            return Execute(currentValue, redoToken.TokenValue, out undoToken);
        }

        [Obsolete]
        protected virtual object Do(object currentValue, object parameter, out UndoToken undoToken)
        {
            undoToken = new UndoToken(false);
            return null;
        }

        [Obsolete]
        protected virtual object Undo(object currentValue, UndoToken undoToken)
        {
            return null;
        }
    }
}