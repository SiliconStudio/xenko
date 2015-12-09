// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.ActionStack;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Commands
{
    /// <summary>
    /// A <see cref="INodeCommand"/> abstract implementation that can be used for commands that simply intent to change the value of the associated node.
    /// This class will manage undo itself, creating a cancellable undo token only if the value returned by the command is different from the initial value.
    /// </summary>
    public abstract class ChangeValueCommand : SimpleNodeCommand
    {
        protected override object Do(object currentValue, object parameter, out UndoToken undoToken)
        {
            var newValue = ChangeValue(currentValue, parameter, false);
            undoToken = !Equals(newValue, currentValue) ? new UndoToken(true, currentValue) : new UndoToken(false);
            return newValue;
        }

        protected override object Undo(object currentValue, UndoToken undoToken)
        {
            return undoToken.TokenValue;
        }

        protected abstract object ChangeValue(object currentValue, object parameter, bool isRedo);
    }
}