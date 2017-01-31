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
        public override Task Execute(IContentNode node, Index index, object parameter)
        {
            var shouldUpdate = node is IMemberNode || ((IObjectNode)node).Indices == null || ((IObjectNode)node).Indices.Contains(index);
            var currentValue = shouldUpdate ? node.Retrieve(index) : (node.Type.IsValueType ? Activator.CreateInstance(node.Type) : null);
            var newValue = ChangeValue(currentValue, parameter);
            if (shouldUpdate)
            {
                if (!Equals(newValue, currentValue))
                {
                    node.Update(newValue, index);
                }
            }
            else
            {
                node.Add(newValue, index);
            }
            return Task.FromResult(0);
        }

        protected abstract object ChangeValue(object currentValue, object parameter);
    }
}
