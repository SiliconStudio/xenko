using System;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.Quantum.Presenters;

namespace SiliconStudio.Presentation.Quantum
{
    public class ValueChangingEventArgs : EventArgs
    {
        private bool coerced;

        public ValueChangingEventArgs(object newValue)
        {
            NewValue = newValue;
        }

        public object NewValue { get; private set; }

        //public bool Cancel { get; set; }

        //public void Coerce(object value)
        //{
        //    NewValue = value;
        //    coerced = true;
        //}
    }

    public class ValueChangedEventArgs : EventArgs
    {
        public ValueChangedEventArgs(object oldValue)
        {
            OldValue = oldValue;
        }

        public object OldValue { get; }
    }

    public class NodeViewModel2<T> : NodeViewModel2
    {
        public NodeViewModel2(GraphViewModel ownerViewModel, NodeViewModel2 parent, string baseName, INodePresenter nodePresenter)
            : base(ownerViewModel, parent, baseName, nodePresenter)
        {
        }

        public virtual T TypedValue { get { return (T)NodePresenter.Value; } set { NodePresenter.UpdateValue(value); } }

        /// <inheritdoc/>
        public sealed override object Value { get { return TypedValue; } set { TypedValue = (T)value; } }
    }

    public class GraphViewModelFactory
    {
        public NodeViewModel2 CreateGraph(GraphViewModel owner, INodePresenter rootNode)
        {
            var rootViewModelNode = CreateNodeViewModel(owner, null, rootNode);
            return rootViewModelNode;
        }

        protected NodeViewModel2 CreateNodeViewModel(GraphViewModel owner, NodeViewModel2 parent, INodePresenter nodePresenter)
        {
            var viewModelType = typeof(NodeViewModel2<>).MakeGenericType(nodePresenter.Type);
            // TODO: assert the constructor!
            var viewModel = (NodeViewModel2)Activator.CreateInstance(viewModelType, owner, nodePresenter.Name, nodePresenter);
            GenerateChildren(owner, viewModel, nodePresenter);
            return viewModel;
        }

        private void GenerateChildren(GraphViewModel owner, NodeViewModel2 parent, INodePresenter nodePresenter)
        {
            foreach (var child in nodePresenter.Children)
            {
                CreateNodeViewModel(owner, parent, child);
            }
        }
    }

    public abstract class NodeViewModel2 : SingleNodeViewModel
    {
        protected readonly INodePresenter NodePresenter;
        private int? customOrder;

        protected NodeViewModel2(GraphViewModel ownerViewModel, NodeViewModel2 parent, string baseName, INodePresenter nodePresenter)
            : base(ownerViewModel, baseName, nodePresenter.Index)
        {
            NodePresenter = nodePresenter;
            parent.AddChild(this);
        }

        public override Type Type => NodePresenter.Type;

        /// <summary>
        /// Gets or sets a custom value for the <see cref="Order"/> of this node.
        /// </summary>
        public int? CustomOrder { get { return customOrder; } set { SetValue(ref customOrder, value, nameof(CustomOrder), nameof(Order)); } }

        /// <inheritdoc/>
        public override int? Order => CustomOrder ?? NodePresenter.Order;

        /// <inheritdoc/>
        public sealed override bool HasCollection => CollectionDescriptor.IsCollection(Type);

        /// <inheritdoc/>
        public sealed override bool HasDictionary => DictionaryDescriptor.IsDictionary(Type);

        [Obsolete]
        public override bool IsPrimitive => false;

        protected override void Refresh()
        {
            throw new NotImplementedException();
        }
    }
}
