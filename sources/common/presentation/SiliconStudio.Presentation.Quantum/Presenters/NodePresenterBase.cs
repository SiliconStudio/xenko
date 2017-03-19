using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public abstract class NodePresenterBase : IInitializingNodePresenter
    {
        private readonly INodePresenterFactoryInternal factory;
        private readonly List<INodePresenter> children = new List<INodePresenter>();
        private readonly PropertyContainer attachedProperties;

        protected NodePresenterBase([NotNull] INodePresenterFactoryInternal factory, INodePresenter parent)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            this.factory = factory;
            Parent = parent;
        }

        public virtual void Dispose()
        {
            // Do nothing by default
        }

        public INodePresenter Parent { get; }

        public IReadOnlyList<INodePresenter> Children => children;

        public abstract string Name { get; }
        public abstract List<INodePresenterCommand> Commands { get; }
        public abstract Type Type { get; }
        public abstract bool IsPrimitive { get; }
        public abstract bool IsEnumerable { get; }
        public abstract Index Index { get; }
        public abstract ITypeDescriptor Descriptor { get; }
        public abstract int? Order { get; }
        public abstract object Value { get; }
        public virtual string CombineKey => Name;
        public PropertyContainer AttachedProperties { get; } = new PropertyContainer();

        public abstract event EventHandler<ValueChangingEventArgs> ValueChanging;
        public abstract event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected abstract IObjectNode ParentingNode { get; }

        public abstract void UpdateValue(object newValue);

        public abstract void AddItem(object value);

        public abstract void AddItem(object value, Index index);

        public abstract void RemoveItem(object value, Index index);

        public abstract void UpdateItem(object newValue, Index index);

        public abstract NodeAccessor GetNodeAccessor();

        protected void Refresh()
        {
            children.Clear();
            var parentingNode = ParentingNode;
            if (parentingNode != null)
            {
                factory.CreateChildren(this, parentingNode);
            }            
        }

        protected void AttachCommands()
        {
            foreach (var command in factory.AvailableCommands)
            {
                if (command.CanAttach(this))
                    Commands.Add(command);
            }
        }

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
