// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    /// <summary>
    /// A <see cref="INodeCommand"/> abstract implementation that can be used for commands that simply intent to change the value of the associated node.
    /// This class will manage undo itself, creating a cancellable undo token only if the value returned by the command is different from the initial value.
    /// </summary>
    public abstract class ChangeValueCommand : NodeCommandBase
    {
        public override Task Execute(IContent content, Index index, object parameter)
        {
            var currentValue = content.Retrieve(index);
            var newValue = ChangeValue(currentValue, parameter);
            if (!Equals(newValue, currentValue))
            {
                content.Update(newValue, index);
            }
            return Task.FromResult(0);
        }

        protected abstract object ChangeValue(object currentValue, object parameter);
    }
}
