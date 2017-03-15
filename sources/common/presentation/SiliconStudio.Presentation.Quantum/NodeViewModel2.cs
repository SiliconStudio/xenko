using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.Quantum
{
    [Obsolete("This interface is temporary to share properties while both GraphNodeViewModel and NodeViewModel2 exist")]
    public interface IGraphNodeViewModel : INodeViewModel
    {
        int? CustomOrder { get; set; }

        IMemberDescriptor GetMemberDescriptor();

        void AddAssociatedData(string key, object value);
    }

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

    public interface INodeViewModelFactory
    {
        NodeViewModel2 CreateGraph(GraphViewModel owner, IEnumerable<INodePresenter> rootNodes);
    }

    public class NodeViewModelFactory : INodeViewModelFactory
    {
        public NodeViewModel2 CreateGraph(GraphViewModel owner, IEnumerable<INodePresenter> rootNodes)
        {
            throw new NotImplementedException();
        }

        public NodeViewModel2 CreateGraph(GraphViewModel owner, INodePresenter rootNode)
        {
            var rootViewModelNode = CreateNodeViewModel(owner, null, rootNode, true);
            return rootViewModelNode;
        }

        protected NodeViewModel2 CreateNodeViewModel(GraphViewModel owner, NodeViewModel2 parent, INodePresenter nodePresenter, bool isRootNode = false)
        {
            var viewModel = new NodeViewModel2(owner, parent, nodePresenter.Name, nodePresenter);
            if (isRootNode)
            {
                owner.RootNode = viewModel;
            }
            GenerateChildren(owner, viewModel, nodePresenter);
            owner.GraphViewModelService?.NotifyNodeInitialized(viewModel);
            return viewModel;
        }

        private void GenerateChildren(GraphViewModel owner, NodeViewModel2 parent, INodePresenter nodePresenter)
        {
            foreach (var child in nodePresenter.Children)
            {
                if (ShouldConstructViewModel(child))
                {
                    CreateNodeViewModel(owner, parent, child);
                }
            }
        }

        private static bool ShouldConstructViewModel(INodePresenter nodePresenter)
        {
            var member = nodePresenter as MemberNodePresenter;
            var displayAttribute = member?.MemberAttributes.OfType<DisplayAttribute>().FirstOrDefault();
            if (displayAttribute != null && !displayAttribute.Browsable)
                return false;

            return true;
        }
    }

    public class NodeViewModel2 : SingleNodeViewModel, IGraphNodeViewModel
    {
        protected readonly INodePresenter NodePresenter;
        private int? customOrder;

        protected internal NodeViewModel2(GraphViewModel ownerViewModel, NodeViewModel2 parent, string baseName, INodePresenter nodePresenter)
            : base(ownerViewModel, nodePresenter.Type, baseName, nodePresenter.Index)
        {
            NodePresenter = nodePresenter;
            var member = nodePresenter as MemberNodePresenter;
            var displayAttribute = member?.MemberAttributes.OfType<DisplayAttribute>().FirstOrDefault();
            if (displayAttribute != null)
                DisplayName = displayAttribute.Name;

            parent?.AddChild(this);
        }

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

        protected internal override object InternalNodeValue { get { return NodePresenter.Value; } set { SetNodeValue(value); } }

        [Obsolete]
        public override bool IsPrimitive => NodePresenter.IsPrimitive;

        protected override void Refresh()
        {

        }

        protected virtual void SetNodeValue(object newValue)
        {
            using (var transaction = ServiceProvider.TryGet<IUndoRedoService>()?.CreateTransaction())
            {
                NodePresenter.UpdateValue(newValue);
                // TODO: move this in the (future) derived class
                if (transaction != null)
                {
                    ServiceProvider.TryGet<IUndoRedoService>()?.SetName(transaction, $"Update property {DisplayPath}");
                }
            }
        }

        IMemberDescriptor IGraphNodeViewModel.GetMemberDescriptor()
        {
            var member = NodePresenter as MemberNodePresenter;
            return member?.MemberDescriptor;
        }
    }
}
