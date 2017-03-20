using System;
using System.Collections.Generic;
using System.Linq;
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

        protected NodePresenterBase([NotNull] INodePresenterFactoryInternal factory, [CanBeNull] IPropertyProviderViewModel propertyProvider, [CanBeNull] INodePresenter parent)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            this.factory = factory;
            Parent = parent;
            PropertyProvider = propertyProvider;
        }

        public virtual void Dispose()
        {
            // Do nothing by default
        }

        public INodePresenter this[string childName] => children.First(x => string.Equals(x.Name, childName, StringComparison.Ordinal));

        public INodePresenter Root => Parent?.Root ?? Parent ?? this;

        public INodePresenter Parent { get; private set; }

        public IReadOnlyList<INodePresenter> Children => children;

        public string DisplayName { get; set; }
        public abstract string Name { get; }
        public abstract List<INodePresenterCommand> Commands { get; }
        public abstract Type Type { get; }
        public abstract bool IsPrimitive { get; }
        public abstract bool IsEnumerable { get; }

        public bool IsVisible { get; set; } = true;
        public bool IsReadOnly { get; }

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

        public IPropertyProviderViewModel PropertyProvider { get; }

        public INodePresenterFactory Factory => factory;

        public void ChangeParent(INodePresenter newParent)
        {
            if (newParent == null) throw new ArgumentNullException(nameof(newParent));

            var parent = (NodePresenterBase)Parent;
            parent?.children.Remove(this);

            parent = (NodePresenterBase)newParent;
            parent.children.Add(this);

            Parent = newParent;
        }

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
