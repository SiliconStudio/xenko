// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.ActionStack;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Commands
{
    public abstract class UncancellableCommand : INodeCommand
    {
        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public abstract CombineMode CombineMode { get; }

        /// <inheritdoc/>
        public abstract bool CanAttach(ITypeDescriptor typeDescriptor, MemberDescriptorBase memberDescriptor);

        public object Invoke(object currentValue, ITypeDescriptor descriptor, object parameter, out UndoToken undoToken)
        {
            undoToken = new UndoToken(false);
            return InvokeUncancellable(currentValue, descriptor, parameter);
        }

        public object Undo(object currentValue, ITypeDescriptor descriptor, UndoToken undoToken)
        {
            throw new NotSupportedException("This command is not cancellable.");
        }

        protected abstract object InvokeUncancellable(object currentValue, ITypeDescriptor descriptor, object parameter);
    }
}
