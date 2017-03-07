using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

namespace SiliconStudio.Presentation.Quantum.Presenters
{
    public class ItemNodePresenter : IInitializingNodePresenter
    {
        private readonly IObjectNode container;
        private readonly List<INodePresenter> children = new List<INodePresenter>();

        public ItemNodePresenter(INodePresenter parent, IObjectNode container, Index index)
        {
            this.container = container;
            Index = index;
            Name = index.ToString();
            Parent = parent;
            container.ItemChanging += OnItemChanging;
            container.ItemChanged += OnItemChanged;
        }

        public void Dispose()
        {
            container.ItemChanging -= OnItemChanging;
            container.ItemChanged -= OnItemChanged;
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

        public string Name { get; }

        public INodePresenter Parent { get; }

        public IReadOnlyList<INodePresenter> Children => children;

        public List<INodeCommand> Commands { get; }

        public Index Index { get; }

        public Type Type { get; }

        public bool IsPrimitive => container.ItemReferences != null;

        public bool IsEnumerable => container.IsEnumerable;

        public ITypeDescriptor Descriptor { get; }

        public int? Order { get; }

        public object Value { get { return container.Retrieve(Index); } set { container.Update(value, Index); } }

        public event EventHandler<ValueChangingEventArgs> ValueChanging;
        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        private bool IsValidChange(INodeChangeEventArgs e)
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

        public void UpdateValue(object newValue)
        {
            try
            {
                container.Update(newValue, Index);
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while updating the value of the node, see the inner exception for more information.", e);
            }
        }

        public void AddItem(object value)
        {
            if (container.IndexedTarget(Index) == null || !container.IndexedTarget(Index).IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                container.IndexedTarget(Index).Add(value);
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public void AddItem(object value, Index index)
        {
            if (container.IndexedTarget(Index) == null || !container.IndexedTarget(Index).IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                container.IndexedTarget(Index).Add(value, index);
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while adding an item to the node, see the inner exception for more information.", e);
            }
        }

        public void RemoveItem(object value, Index index)
        {
            if (container.IndexedTarget(Index) == null || !container.IndexedTarget(Index).IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                container.IndexedTarget(Index).Remove(value, index);
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while removing an item to the node, see the inner exception for more information.", e);
            }
        }

        public void UpdateItem(object newValue, Index index)
        {
            if (container.IndexedTarget(Index) == null || !container.IndexedTarget(Index).IsEnumerable)
                throw new NodePresenterException($"{nameof(MemberNodePresenter)}.{nameof(AddItem)} cannot be invoked on members that are not collection.");

            try
            {
                container.IndexedTarget(Index).Update(newValue, index);
            }
            catch (Exception e)
            {
                throw new NodePresenterException("An error occurred while updating an item of the node, see the inner exception for more information.", e);
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
