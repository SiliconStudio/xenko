// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace SiliconStudio.Quantum.Commands
{
    /// <summary>
    /// A <see cref="INodeCommand"/> abstract implementation that can be used for commands that simply intent to change the value of the associated node.
    /// This class will manage undo itself, creating a cancellable undo token only if the value returned by the command is different from the initial value.
    /// </summary>
    public abstract class ChangeValueCommand : NodeCommandBase
    {
        public override Task Execute(IContentNode content, Index index, object parameter)
        {
            var hasIndex = content.Indices == null || content.Indices.Contains(index);
            var currentValue = hasIndex ? content.Retrieve(index) : (content.Type.IsValueType ? Activator.CreateInstance(content.Type) : null);
            var newValue = ChangeValue(currentValue, parameter);
            if (hasIndex)
            {
                if (!Equals(newValue, currentValue))
                {
                    content.Update(newValue, index);
                }
            }
            else
            {
                content.Add(newValue, index);
            }
            return Task.FromResult(0);
        }

        protected abstract object ChangeValue(object currentValue, object parameter);
    }
}
