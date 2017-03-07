using System;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Quantum;

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

    public interface INodePresenterFactory
    {
        INodePresenter CreateNodeTree(string rootName, bool isPrimitive, IObjectNode rootNode, GraphNodePath rootNodePath, Type contentType, Index index);
    }

    public class NodePresenterFactory //: INodePresenterFactory
    {
        [NotNull]
        public INodePresenter CreateNodeHierarchy(IObjectNode rootNode, GraphNodePath rootNodePath)
        {
            var rootPresenter = CreateRootPresenter(rootNode);
            CreateChildren(rootPresenter, rootNode);
            return rootPresenter;
        }

        private void CreateChildren(IInitializingNodePresenter parentPresenter, IObjectNode objectNode)
        {
            CreateMembers(parentPresenter, objectNode);
            CreateItems(parentPresenter, objectNode);
            parentPresenter.FinalizeInitialization();
        }

        private void CreateMembers(IInitializingNodePresenter parentPresenter, IObjectNode objectNode)
        {
            foreach (var member in objectNode.Members)
            {
                if (ShouldCreateMemberPresenter(member))
                {
                    var memberPresenter = CreateMember(parentPresenter, member);
                    if (member.Target != null)
                    {
                        CreateChildren(memberPresenter, member.Target);
                    }
                    parentPresenter.AddChild(memberPresenter);
                }
            }
        }

        private void CreateItems(IInitializingNodePresenter parentPresenter, IObjectNode objectNode)
        {
            if (objectNode.IsEnumerable)
            {
                if (objectNode.ItemReferences != null)
                {
                    foreach (var item in objectNode.ItemReferences)
                    {
                        if (ShouldCreateItemPresenter(objectNode, item.Index))
                        {
                            var itemPresenter = CreateItem(parentPresenter, objectNode, item.Index);
                            if (item.TargetNode != null)
                            {
                                CreateChildren(itemPresenter, item.TargetNode);
                            }
                            parentPresenter.AddChild(itemPresenter);
                        }
                    }
                }
                else
                {
                    foreach (var item in objectNode.Indices)
                    {
                        if (ShouldCreateItemPresenter(objectNode, item))
                        {
                            var itemPresenter = CreateItem(parentPresenter, objectNode, item);
                            parentPresenter.AddChild(itemPresenter);
                        }
                    }
                }
            }
        }

        [NotNull]
        protected virtual IInitializingNodePresenter CreateRootPresenter(IObjectNode rootNode)
        {
            return new RootNodePresenter(rootNode);
        }

        protected virtual bool ShouldCreateMemberPresenter(IMemberNode member)
        {
            return true;
        }

        protected virtual IInitializingNodePresenter CreateMember(INodePresenter parentPresenter, IMemberNode member)
        {
            return new MemberNodePresenter(parentPresenter, member);
        }

        protected virtual bool ShouldCreateItemPresenter(IObjectNode objectNode, Index item)
        {
            return true;
        }

        [NotNull]
        protected virtual IInitializingNodePresenter CreateItem(INodePresenter containerPresenter, IObjectNode containerNode, Index index)
        {
            return new ItemNodePresenter(containerPresenter, containerNode, index);
        }
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
