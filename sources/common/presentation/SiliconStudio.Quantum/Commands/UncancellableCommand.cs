// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Commands
{
    public abstract class UncancellableCommand : NodeCommand
    {
        public sealed override object Invoke(object currentValue, object parameter, out UndoToken undoToken)
        {
            undoToken = new UndoToken(false);
            InvokeUncancellable(currentValue, parameter);
            return currentValue;
        }

        public sealed override object Undo(object currentValue, UndoToken undoToken)
        {
            throw new NotSupportedException("This command is not cancellable.");
        }

        protected abstract void InvokeUncancellable(object currentValue, object parameter);
    }
}
