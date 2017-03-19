using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Presentation.Services;
using SiliconStudio.Quantum;

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
        NodeViewModel2 CreateGraph([NotNull] GraphViewModel owner, [NotNull] Type rootType, [NotNull] IEnumerable<INodePresenter> rootNodes);
    }

    public class NodeViewModelFactory : INodeViewModelFactory
    {
        public NodeViewModel2 CreateGraph(GraphViewModel owner, Type rootType, IEnumerable<INodePresenter> rootNodes)
        {
            var rootViewModelNode = CreateNodeViewModel(owner, null, rootType, rootNodes.ToList(), true);
            return rootViewModelNode;
        }

        protected NodeViewModel2 CreateNodeViewModel(GraphViewModel owner, NodeViewModel2 parent, Type nodeType, List<INodePresenter> nodePresenters, bool isRootNode = false)
        {
            // TODO: properly compute the name
            var viewModel = new NodeViewModel2(owner, parent, nodePresenters.First().Name, nodeType, nodePresenters);
            if (isRootNode)
            {
                owner.RootNode = viewModel;
            }
            GenerateChildren(owner, viewModel, nodePresenters);

            foreach (var nodePresenter in nodePresenters)
            {
                foreach (var command in nodePresenter.Commands)
                {
                    // TODO: review algorithm and properly implement CombineMode
                    if (viewModel.Commands.Cast<NodePresenterCommandWrapper>().All(x => x.Command != command))
                    {
                        var commandWrapper = new NodePresenterCommandWrapper(viewModel.ServiceProvider, nodePresenter, command);
                        viewModel.AddCommand(commandWrapper);
                    }
                }
            }

            owner.GraphViewModelService?.NotifyNodeInitialized(viewModel);
            return viewModel;
        }

        protected virtual IEnumerable<List<INodePresenter>> CombineChildren(List<INodePresenter> nodePresenters)
        {
            var dictionary = new Dictionary<string, List<INodePresenter>>();
            foreach (var nodePresenter in nodePresenters)
            {
                foreach (var child in nodePresenter.Children)
                {
                    List<INodePresenter> presenters;
                    // TODO: properly implement CombineKey
                    if (!dictionary.TryGetValue(child.CombineKey, out presenters))
                    {
                        presenters = new List<INodePresenter>();
                        dictionary.Add(child.CombineKey, presenters);
                    }
                    presenters.Add(child);
                }
            }
            return dictionary.Values.Where(x => x.Count == nodePresenters.Count);
        }

        private void GenerateChildren(GraphViewModel owner, NodeViewModel2 parent, List<INodePresenter> nodePresenters)
        {
            foreach (var child in CombineChildren(nodePresenters))
            {
                if (ShouldConstructViewModel(child))
                {
                    // TODO: properly compute the type
                    CreateNodeViewModel(owner, parent, child.First().Type, child);
                }
            }
        }

        private static bool ShouldConstructViewModel(List<INodePresenter> nodePresenters)
        {
            foreach (var nodePresenter in nodePresenters)
            {
                var member = nodePresenter as MemberNodePresenter;
                var displayAttribute = member?.MemberAttributes.OfType<DisplayAttribute>().FirstOrDefault();
                if (displayAttribute != null && !displayAttribute.Browsable)
                    return false;
            }
            return true;
        }
    }

    public class NodeViewModel2 : SingleNodeViewModel, IGraphNodeViewModel
    {
        private readonly List<INodePresenter> nodePresenters;

        private int? customOrder;

        protected internal NodeViewModel2(GraphViewModel ownerViewModel, NodeViewModel2 parent, string baseName, Type nodeType, List<INodePresenter> nodePresenters)
            : base(ownerViewModel, nodeType, baseName, default(Index))
        {
            this.nodePresenters = nodePresenters;
            foreach (var nodePresenter in nodePresenters)
            {
                var member = nodePresenter as MemberNodePresenter;
                var displayAttribute = member?.MemberAttributes.OfType<DisplayAttribute>().FirstOrDefault();
                // TODO: check for discrepencies in the display attribute name
                if (displayAttribute != null)
                    DisplayName = displayAttribute.Name;
            }

            parent?.AddChild(this);
        }

        /// <summary>
        /// Gets or sets a custom value for the <see cref="Order"/> of this node.
        /// </summary>
        public int? CustomOrder { get { return customOrder; } set { SetValue(ref customOrder, value, nameof(CustomOrder), nameof(Order)); } }

        /// <inheritdoc/>
        // FIXME
        public override int? Order => CustomOrder ?? NodePresenters.First().Order;

        /// <inheritdoc/>
        public sealed override bool HasCollection => CollectionDescriptor.IsCollection(Type);

        /// <inheritdoc/>
        public sealed override bool HasDictionary => DictionaryDescriptor.IsDictionary(Type);

        public IReadOnlyCollection<INodePresenter> NodePresenters => nodePresenters;

        // FIXME

        protected internal override object InternalNodeValue { get { return NodePresenters.First().Value; } set { SetNodeValue(value); } }

        [Obsolete]
        // FIXME
        public override bool IsPrimitive => NodePresenters.First().IsPrimitive;

        protected override void Refresh()
        {

        }

        protected virtual void SetNodeValue(object newValue)
        {
            foreach (var nodePresenter in NodePresenters)
            {
                // TODO: normally it shouldn't take that path (since it uses commands), but this is not safe with newly instantiated values
                nodePresenter.UpdateValue(newValue);
            }
        }

        IMemberDescriptor IGraphNodeViewModel.GetMemberDescriptor()
        {
            // FIXME
            var member = NodePresenters.First() as MemberNodePresenter;
            return member?.MemberDescriptor;
        }
    }
}
