using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class ItemNodePresenter : NodePresenterBase
    {
        private readonly IObjectNode container;

        public ItemNodePresenter([NotNull] INodePresenterFactoryInternal factory, INodePresenter parent, IObjectNode container, Index index)
            : base(factory, parent)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            this.container = container;
            Index = index;
            Name = index.ToString();
            container.ItemChanging += OnItemChanging;
            container.ItemChanged += OnItemChanged;
        }

        public override void Dispose()
        {
            container.ItemChanging -= OnItemChanging;
            container.ItemChanged -= OnItemChanged;
        }

        public override string Name { get; }

        public override List<INodeCommand> Commands { get; }

        public override Index Index { get; }

        public override Type Type { get; }

        public override bool IsPrimitive => container.ItemReferences != null;

        public override bool IsEnumerable => container.IsEnumerable;

        public override ITypeDescriptor Descriptor { get; }

        public override int? Order { get; }

        public override object Value => container.Retrieve(Index);

        public override event EventHandler<ValueChangingEventArgs> ValueChanging;

        public override event EventHandler<ValueChangedEventArgs> ValueChanged;

        protected override IObjectNode ParentingNode => container.ItemReferences != null ? container.IndexedTarget(Index) : null;

        public override void UpdateValue(object newValue)
        {
            try
            {
                container.Update(newValue, Index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while updating the value of the node, see the inner exception for more information.", e);
            }
        }

        public override void AddItem(object value)
        {
            if (container.IndexedTarget(Index) == null || !container.IndexedTarget(Index).IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                container.IndexedTarget(Index).Add(value);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void AddItem(object value, Index index)
        {
            if (container.IndexedTarget(Index) == null || !container.IndexedTarget(Index).IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                container.IndexedTarget(Index).Add(value, index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void RemoveItem(object value, Index index)
        {
            if (container.IndexedTarget(Index) == null || !container.IndexedTarget(Index).IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                container.IndexedTarget(Index).Remove(value, index);
                Refresh();
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while removing an item to the node, see the inner exception for more information.", e);
            }
        }

        public override void UpdateItem(object newValue, Index index)
        {
            if (container.IndexedTarget(Index) == null || !container.IndexedTarget(Index).IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                container.IndexedTarget(Index).Update(newValue, index);
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
