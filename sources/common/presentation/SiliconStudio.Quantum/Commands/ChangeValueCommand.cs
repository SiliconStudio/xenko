// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.ActionStack;
using SiliconStudio.Quantum.ActionStack;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    /// <summary>
    /// A <see cref="INodeCommand"/> abstract implementation that can be used for commands that simply intent to change the value of the associated node.
    /// This class will manage undo itself, creating a cancellable undo token only if the value returned by the command is different from the initial value.
    /// </summary>
    public abstract class ChangeValueCommand : NodeCommandBase
    {
        [Obsolete]
        public override object Execute(object currentValue, object parameter, out UndoToken undoToken)
        {
            undoToken = new UndoToken(false);
            return null;
        }

        [Obsolete]
        public override object Undo(object currentValue, UndoToken undoToken, out RedoToken redoToken)
        {
            redoToken = new RedoToken();
            return null;
        }

        [Obsolete]
        public override object Redo(object currentValue, RedoToken redoToken, out UndoToken undoToken)
        {
            undoToken = new UndoToken(false);
            return null;
        }

        public override Task<IActionItem> Execute2(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables)
        {
            var currentValue = content.Retrieve(index);
            var newValue = ChangeValue(currentValue, parameter, false);
            IActionItem actionItem = null;
            if (!Equals(newValue, currentValue))
            {
                content.Update(index);
                actionItem = new ContentValueChangedActionItem("Change property via command '{Name}'", content, index, currentValue, dirtiables);
            }
            return Task.FromResult(actionItem);
        }

        protected abstract object ChangeValue(object currentValue, object parameter, bool isRedo);
    }
}