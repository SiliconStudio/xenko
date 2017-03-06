using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class RootNodePresenter : IInitializingNodePresenter
    {
        private readonly IObjectNode rootNode;
        private readonly List<INodePresenter> children = new List<INodePresenter>();

        public RootNodePresenter(IObjectNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public void Dispose()
        {

        }

        public string Name => "Root";
        public INodePresenter Parent => null;
        public IReadOnlyList<INodePresenter> Children => children;
        public List<INodeCommand> Commands { get; }
        public Type Type => rootNode.Type;
        public Index Index => Index.Empty;
        public bool IsPrimitive => false;
        public ITypeDescriptor Descriptor { get; }
        public int? Order => null;
        public object Value { get { return rootNode.Retrieve(); } set { throw new InvalidOperationException("A RootNodePresenter value cannot be modified"); } }
        public event EventHandler<ValueChangingEventArgs> ValueChanging;
        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        void IInitializingNodePresenter.AddChild([NotNull] IInitializingNodePresenter child)
        {
            children.Add(child);
        }

        void IInitializingNodePresenter.FinalizeInitialization()
        {
            children.Sort(GraphNodePresenter.CompareChildren);
        }

    }
}
