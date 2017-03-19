using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class ItemNodePresenter : NodePresenterBase
    {
        protected readonly IObjectNode Container;

        public ItemNodePresenter([NotNull] INodePresenterFactoryInternal factory, [NotNull] INodePresenter parent, [NotNull] IObjectNode container, Index index)
            : base(factory, parent)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (container == null) throw new ArgumentNullException(nameof(container));
            Container = container;
            OwnerCollection = parent;
            Type = (container.Descriptor as CollectionDescriptor)?.ElementType ?? (container.Descriptor as DictionaryDescriptor)?.ValueType;
            Index = index;
            Name = index.ToString();
            container.ItemChanging += OnItemChanging;
            container.ItemChanged += OnItemChanged;
            AttachCommands();
        }

        public override void Dispose()
        {
            Container.ItemChanging -= OnItemChanging;
            Container.ItemChanged -= OnItemChanged;
        }

        public override string Name { get; }

        public sealed override List<INodePresenterCommand> Commands { get; } = new List<INodePresenterCommand>();

        public INodePresenter OwnerCollection { get; }

        public override Index Index { get; }

        public override Type Type { get; }

        public override bool IsPrimitive => Container.ItemReferences != null;

        public override bool IsEnumerable => Container.IsEnumerable;

        public override ITypeDescriptor Descriptor { get; }

        public override int? Order { get; }

        public override object Value => Container.Retrieve(Index);

        public override event EventHandler<ValueChangingEventArgs> ValueChanging;

        public override event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected override IObjectNode ParentingNode => Container.ItemReferences != null ? Container.IndexedTarget(Index) : null;

        public override void UpdateValue(object newValue)
        {
            try
            {
                Container.Update(newValue, Index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while updating the value of the node, see the inner exception for more information.", e);
            }
        }

        public override void AddItem(object value)
        {
            if (Container.IndexedTarget(Index) == null || !Container.IndexedTarget(Index).IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                Container.IndexedTarget(Index).Add(value);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void AddItem(object value, Index index)
        {
            if (Container.IndexedTarget(Index) == null || !Container.IndexedTarget(Index).IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                Container.IndexedTarget(Index).Add(value, index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void RemoveItem(object value, Index index)
        {
            if (Container.IndexedTarget(Index) == null || !Container.IndexedTarget(Index).IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                Container.IndexedTarget(Index).Remove(value, index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while removing an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void UpdateItem(object newValue, Index index)
        {
            if (Container.IndexedTarget(Index) == null || !Container.IndexedTarget(Index).IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                Container.IndexedTarget(Index).Update(newValue, index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while updating an item of the node, see the inner exception for more information.", e);
            }
        }

        private void OnItemChanging(object sender, ItemChangeEventArgs e)
        {
            if (IsValidChange(e))
                ValueChanging?.Invoke(this, new ValueChangingEventArgs(e.NewValue));
        }

        private void OnItemChanged(object sender, ItemChangeEventArgs e)
        {
            Refresh();
            ValueChanged?.Invoke(this, new ValueChangedEventArgs(e.OldValue));
        }

        private bool IsValidChange([NotNull] INodeChangeEventArgs e)
        {
            switch (e.ChangeType)
            {
                case ContentChangeType.CollectionUpdate:
                    return Equals(e.Index, Index);
                case ContentChangeType.CollectionAdd:
                case ContentChangeType.CollectionRemove:
                    return true; // TODO: probably not sufficent
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
