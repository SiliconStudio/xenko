using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;

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

    public interface INodePresenter : IDisposable
    {
        string Name { get; }

        INodePresenter Parent { get; }

        IReadOnlyList<INodePresenter> Children { get; }

        List<INodeCommand> Commands { get; }

        Type Type { get; }

        bool IsPrimitive { get; }

        Index Index { get; }

        ITypeDescriptor Descriptor { get; }

        int? Order { get; }

        object Value { get; set; }

        event EventHandler<ValueChangingEventArgs> ValueChanging;

        event EventHandler<ValueChangedEventArgs> ValueChanged;
    }

    public interface IInitializingNodePresenter : INodePresenter
    {
        void AddChild(IInitializingNodePresenter child);

        void FinalizeInitialization();
    }

    public class GraphNodePresenter
    {
        public static int CompareChildren(INodePresenter a, INodePresenter b)
        {
            // Order has the best priority for comparison, if set.
            if ((a.Order ?? 0) != (b.Order ?? 0))
                return (a.Order ?? 0).CompareTo(b.Order ?? 0);

            // Then we use index, if they are set and comparable.
            if (!a.Index.IsEmpty && !b.Index.IsEmpty)
            {
                if (a.Index.Value.GetType() == b.Index.Value.GetType())
                {
                    return a.Index.CompareTo(b.Index);
                }
            }

            // Then we use name, only if both orders are unset.
            if (a.Order == null && b.Order == null)
            {
                return string.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase);
            }

            // Otherwise, the first child would be the one who have an order value.
            return a.Order == null ? 1 : -1;
        }
    }

    public class MemberNodePresenter : IInitializingNodePresenter
    {
        private readonly IMemberNode member;
        private readonly List<INodePresenter> children = new List<INodePresenter>();
        private readonly List<Attribute> memberAttributes = new List<Attribute>();

        public MemberNodePresenter(INodePresenter parent, [NotNull] IMemberNode member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            this.member = member;
            Name = member.Name;
            Parent = parent;

            memberAttributes.AddRange(TypeDescriptorFactory.Default.AttributeRegistry.GetAttributes(member.MemberDescriptor.MemberInfo));

            member.Changing += OnMemberChanging;
            member.Changed += OnMemberChanged;

            if (member.Target != null)
            {
                // If we have a target node, register commands attached to it. They will override the commands of the member node by name.
                Commands.AddRange(member.Target.Commands);
            }

            // Register commands from the member node, skipping those already registered from the target node.
            var targetCommandNames = Commands.Select(x => x.Name).ToList();
            Commands.AddRange(member.Commands.Where(x => !targetCommandNames.Contains(x.Name)));

            var displayAttribute = memberAttributes.OfType<DisplayAttribute>().FirstOrDefault();
            Order = displayAttribute?.Order ?? member.MemberDescriptor.Order;
        }

        public void Dispose()
        {
            member.Changing -= OnMemberChanging;
            member.Changed -= OnMemberChanged;
        }

        public string Name { get; }

        public INodePresenter Parent { get; }

        public IReadOnlyList<INodePresenter> Children => children;

        public List<INodeCommand> Commands { get; } = new List<INodeCommand>();

        public Type Type => member.Type;

        public bool IsPrimitive => member.IsPrimitive;

        public Index Index => Index.Empty;

        public ITypeDescriptor Descriptor { get; }

        public int? Order { get; }

        public object Value { get { return member.Retrieve(); } set { member.Update(value); } }

        public IReadOnlyList<Attribute> MemberAttributes => memberAttributes;

        public event EventHandler<ValueChangingEventArgs> ValueChanging;

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        private void OnMemberChanging(object sender, MemberNodeChangeEventArgs e)
        {
            ValueChanging?.Invoke(this, new ValueChangingEventArgs(e.NewValue));
        }

        private void OnMemberChanged(object sender, MemberNodeChangeEventArgs e)
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs(e.OldValue));
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

    public class ItemNodePresenter : IInitializingNodePresenter
    {
        private readonly IObjectNode container;
        private readonly List<INodePresenter> children = new List<INodePresenter>();

        public ItemNodePresenter(INodePresenter parent, IObjectNode container, Index index)
        {
            this.container = container;
            this.Index = index;
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

        void IInitializingNodePresenter.AddChild([NotNull] IInitializingNodePresenter child)
        {
            children.Add(child);
        }

        void IInitializingNodePresenter.FinalizeInitialization()
        {
            children.Sort(GraphNodePresenter.CompareChildren);
        }
    }

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

    public interface INodePresenterFactory
    {
        INodePresenter CreateNodeTree(string rootName, bool isPrimitive, IObjectNode rootNode, GraphNodePath rootNodePath, Type contentType, Index index);
    }

    public class NodePresenterFactory //: INodePresenterFactory
    {
        [NotNull]
        public INodePresenter CreateNodeTree(IObjectNode rootNode, GraphNodePath rootNodePath)
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

        public virtual T TypedValue { get { return (T)NodePresenter.Value; } set { NodePresenter.Value = value; } }

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
