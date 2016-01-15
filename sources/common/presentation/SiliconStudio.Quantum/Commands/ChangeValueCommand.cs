// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
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
    public abstract class ChangeValueCommand : SimpleNodeCommand
    {
        public override Task<ActionItem> Execute2(IContent content, object index, object parameter, IEnumerable<IDirtiable> dirtiables)
        {
            var currentValue = content.Retrieve(index);
            var newValue = ChangeValue(currentValue, parameter, false);
            var actionItem = !Equals(newValue, currentValue) ? new ContentValueChangedActionItem("Change property via command '{Name}'", content, index, currentValue, dirtiables) : null;
            return Task.FromResult<ActionItem>(actionItem);
        }

        protected override object Do(object currentValue, object parameter, out UndoToken undoToken)
        {
            undoToken = new UndoToken(false);
            return currentValue;
        }

        protected override object Undo(object currentValue, UndoToken undoToken)
        {
            return undoToken.TokenValue;
        }

        protected abstract object ChangeValue(object currentValue, object parameter, bool isRedo);
    }
}