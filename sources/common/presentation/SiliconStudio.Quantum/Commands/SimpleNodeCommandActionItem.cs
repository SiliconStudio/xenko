using System.Collections.Generic;
using SiliconStudio.ActionStack;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    public abstract class SimpleNodeCommandActionItem : NodeCommandActionItem
    {
        protected SimpleNodeCommandActionItem(string name, IContent content, object index, IEnumerable<IDirtiable> dirtiables)
            : base(name, content, index, dirtiables)
        {
        }

        protected sealed override void RedoAction()
        {
            Do();
        }
    }
}