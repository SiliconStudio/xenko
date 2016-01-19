using System;
using System.Collections.Generic;
using SiliconStudio.ActionStack;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Commands
{
    public abstract class NodeCommandActionItem : DirtiableActionItem
    {
        protected IContent Content;
        protected object Index;

        protected NodeCommandActionItem(string name, IContent content, object index, IEnumerable<IDirtiable> dirtiables) : base(name, dirtiables)
        {
            Content = content;
            Index = index;
        }

        public abstract bool Do();

        protected override void FreezeMembers()
        {
            Content = null;
            Index = null;
        }
    }
}