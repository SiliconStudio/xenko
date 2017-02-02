// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.Contents;

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

    public abstract class GraphNodeViewModel : SingleNodeViewModel
    {
        public readonly IContentNode SourceNode;
        private readonly bool isPrimitive;
        private bool isInitialized;
        private int? customOrder;

        static GraphNodeViewModel()
        {
            typeof(GraphNodeViewModel).GetProperties().Select(x => x.Name).ForEach(x => ReservedNames.Add(x));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNodeViewModel"/> class.
        /// </summary>
        /// <param name="ownerViewModel">The <see cref="GraphViewModel"/> that owns the new <see cref="GraphNodeViewModel"/>.</param>
        /// <param name="baseName">The base name of this node. Can be null if <see cref="index"/> is not. If so a name will be automatically generated from the index.</param>
        /// <param name="isPrimitive">Indicate whether this node should be considered as a primitive node.</param>
        /// <param name="sourceNode">The model node bound to the new <see cref="GraphNodeViewModel"/>.</param>
        /// <param name="graphNodePath">The <see cref="GraphNodePath"/> corresponding to the given <see cref="sourceNode"/>.</param>
        /// <param name="index">The index of this content in the model node, when this node represent an item of a collection. <see cref="Index.Empty"/> must be passed otherwise</param>
        protected GraphNodeViewModel(GraphViewModel ownerViewModel, string baseName, bool isPrimitive, IContentNode sourceNode, GraphNodePath graphNodePath, Index index)
            : base(ownerViewModel, baseName, index)
        {
            if (sourceNode == null) throw new ArgumentNullException(nameof(sourceNode));
            if (baseName == null && index == null)
                throw new ArgumentException("baseName and index can't be both null.");

            this.isPrimitive = isPrimitive;
            SourceNode = sourceNode;
            // By default we will always combine items of list of primitive items.
            CombineMode = !index.IsEmpty && isPrimitive ? CombineMode.AlwaysCombine : CombineMode.CombineOnlyForAll;
            SourceNodePath = graphNodePath;

            // Override display name if available
            var memberDescriptor = GetMemberDescriptor() as MemberDescriptorBase;
            if (memberDescriptor != null)
            {
                if (index.IsEmpty)
                {
                    var displayAttribute = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<DisplayAttribute>(memberDescriptor.MemberInfo);
                    if (!string.IsNullOrEmpty(displayAttribute?.Name))
                    {
                        DisplayName = displayAttribute.Name;
                    }
                    IsReadOnly = !memberDescriptor.HasSet;
                }
            }
        }

        /// <summary>
        /// Create an <see cref="GraphNodeViewModel{T}"/> that matches the given content type.
        /// </summary>
        /// <param name="ownerViewModel">The <see cref="GraphViewModel"/> that owns the new <see cref="GraphNodeViewModel"/>.</param>
        /// <param name="baseName">The base name of this node. Can be null if <see cref="index"/> is not. If so a name will be automatically generated from the index.</param>
        /// <param name="isPrimitive">Indicate whether this node should be considered as a primitive node.</param>
        /// <param name="sourceNode">The model node bound to the new <see cref="GraphNodeViewModel"/>.</param>
        /// <param name="graphNodePath">The <see cref="GraphNodePath"/> corresponding to the given node.</param>
        /// <param name="contentType">The type of content contained by the new <see cref="GraphNodeViewModel"/>.</param>
        /// <param name="index">The index of this content in the model node, when this node represent an item of a collection. <see cref="Index.Empty"/> must be passed otherwise</param>
        /// <returns>A new instance of <see cref="GraphNodeViewModel{T}"/> instanced with the given content type as generic argument.</returns>
        internal static GraphNodeViewModel Create(GraphViewModel ownerViewModel, string baseName, bool isPrimitive, IContentNode sourceNode, GraphNodePath graphNodePath, Type contentType, Index index)
        {
            var node = (GraphNodeViewModel)Activator.CreateInstance(typeof(GraphNodeViewModel<>).MakeGenericType(contentType), ownerViewModel, baseName, isPrimitive, sourceNode, graphNodePath, index);
            return node;
        }

        /// <summary>
        /// Initializes this node. This method is called right after construction of the node, and after <see cref="NodeViewModel.AddChild"/> as been called on its parent if this node has a parent.
        /// </summary>
        protected internal void Initialize()
        {
            var targetNode = GetTargetNode(SourceNode, Index);

            if (targetNode != SourceNode && targetNode != null)
            {
                foreach (var command in targetNode.Commands)
                {
                    var commandWrapper = new ModelNodeCommandWrapper(ServiceProvider, command, SourceNodePath, Index);
                    AddCommand(commandWrapper);
                }
            }

            var targetCommandNames = Commands.Select(x => x.Name).ToList();
            foreach (var command in SourceNode.Commands)
            {
                // Add source commands that are not already provided by the target node
                if (!targetCommandNames.Contains(command.Name))
                {
                    var commandWrapper = new ModelNodeCommandWrapper(ServiceProvider, command, SourceNodePath, Index);
                    AddCommand(commandWrapper);
                }
            }

            if (!isPrimitive && targetNode != null)
            {
                var targetNodePath = GetTargetNodePath();
                if (targetNodePath == null || !targetNodePath.IsValid)
                    throw new InvalidOperationException("Unable to retrieve the path of the given model node.");

                GenerateChildren(targetNode, targetNodePath, Index);
            }

            isInitialized = true;

            CheckDynamicMemberConsistency();
        }

        /// <inheritdoc/>
        protected internal override void FinalizeInitialization()
        {
            base.FinalizeInitialization();
            Owner.GraphViewModelService?.NotifyNodeInitialized(this);
        }

        public GraphNodePath SourceNodePath { get; }

        /// <inheritdoc/>
        public override int? Order
        {
            get
            {
                if (CustomOrder != null)
                    return CustomOrder;

                var memberContent = SourceNode as MemberContent;
                if (memberContent == null || !Index.IsEmpty)
                    return null;

                var descriptor = (MemberDescriptorBase)memberContent.MemberDescriptor;
                var displayAttribute = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<DisplayAttribute>(descriptor.MemberInfo);
                return displayAttribute?.Order ?? descriptor.Order;
            }
        }

        public override MemberInfo MemberInfo
        {
            get
            {
                var memberContent = SourceNode as MemberContent;
                var memberDescriptorBase = memberContent?.MemberDescriptor as MemberDescriptorBase;
                return memberDescriptorBase?.MemberInfo;
            }
        }

        /// <summary>
        /// Gets or sets a custom value for the <see cref="Order"/> of this node.
        /// </summary>
        public int? CustomOrder { get { return customOrder; } set { SetValue(ref customOrder, value, nameof(CustomOrder), nameof(Order)); } }

        /// <inheritdoc/>
        public sealed override bool IsPrimitive => isPrimitive;

        /// <inheritdoc/>
        public sealed override bool HasCollection => CollectionDescriptor.IsCollection(Type);

        /// <inheritdoc/>
        public sealed override bool HasDictionary => DictionaryDescriptor.IsDictionary(Type);

        // The previous way to compute HasList and HasDictionary was quite complex, but let's keep it here for history. 
        // To distinguish between lists and items of a list (which have the same TargetNode if the items are primitive types), we check whether the TargetNode is
        // the same of the one of its parent. If so, we're likely in an item of a list of primitive objects. 
        //public sealed override bool HasList => (targetNode.Descriptor is CollectionDescriptor && (Parent == null || (ModelNodeParent != null && ModelNodeParent.targetNode.Value != targetNode.Value))) || (targetNode.ShouldProcessReference && targetNode.Reference is ReferenceEnumerable);
        // To distinguish between dictionaries and items of a dictionary (which have the same TargetNode if the value type is a primitive type), we check whether the TargetNode is
        // the same of the one of its parent. If so, we're likely in an item of a dictionary of primitive objects. 
        //public sealed override bool HasDictionary => (targetNode.Descriptor is DictionaryDescriptor && (Parent == null || (ModelNodeParent != null && ModelNodeParent.targetNode.Value != targetNode.Value))) || (targetNode.ShouldProcessReference && targetNode.Reference is ReferenceEnumerable && ((ReferenceEnumerable)targetNode.Reference).IsDictionary);

        internal Guid ModelGuid => SourceNode.Guid;

        /// <summary>
        /// Indicates whether this <see cref="GraphNodeViewModel"/> instance corresponds to the given <see cref="IContentNode"/>.
        /// </summary>
        /// <param name="node">The node to match.</param>
        /// <returns><c>true</c> if the node matches, <c>false</c> otherwise.</returns>
        public bool MatchNode(IContentNode node)
        {
            return SourceNode == node;
        }

        // TODO: If possible, make this private, it's not a good thing to expose
        public IMemberDescriptor GetMemberDescriptor()
        {
            var memberContent = SourceNode as MemberContent;
            return memberContent?.MemberDescriptor;
        }

        internal void CheckConsistency()
        {
#if DEBUG
            var targetNode = GetTargetNode(SourceNode, Index);
            if (SourceNode != targetNode)
            {
                var objectReference = (SourceNode as IMemberNode)?.TargetReference;
                var referenceEnumerable = (SourceNode as IObjectNode)?.ItemReferences;
                if (objectReference != null && targetNode != objectReference.TargetNode)
                {
                    throw new GraphViewModelConsistencyException(this, "The target node does not match the target of the source node object reference.");
                }
                if (referenceEnumerable != null && !Index.IsEmpty)
                {
                    if (!referenceEnumerable.HasIndex(Index))
                        throw new GraphViewModelConsistencyException(this, "The Index of this node does not exist in the reference of its source node.");

                    if (targetNode != referenceEnumerable[Index].TargetNode)
                    {
                        throw new GraphViewModelConsistencyException(this, "The target node does not match the target of the source node object reference.");
                    }
                }
            }

            var modelContentValue = GetModelContentValue();
            if (!Equals(modelContentValue, Value))
            {
                // TODO: I had this exception with a property that is returning a new IEnumerable each time - we should have a way to notice this, maybe by correctly transfering and checking the IsReadOnly property
                //throw new GraphViewModelConsistencyException(this, "The value of this node does not match the value of its source node content.");
            }

            foreach (var child in Children.OfType<GraphNodeViewModel>())
            {
                if (targetNode.IsReference)
                {
                    var objectReference = (targetNode as IMemberNode)?.TargetReference;
                    if (objectReference != null)
                    {
                        throw new GraphViewModelConsistencyException(this, "The target node does not match the target of the source node object reference.");
                    }
                }
                child.CheckConsistency();
            }
#endif
        }

        public new void ClearCommands()
        {
            base.ClearCommands();
        }
        
        /// <summary>
        /// Retrieves the path of the target node if the source node content holds a reference or a sequence of references, or the source node path otherwise.
        /// </summary>
        /// <returns>The path to the corresponding target node if available, or the path to source node itself if it does not contain any reference or if its content should not process references.</returns>
        /// <remarks>This method can return null if the target node is null.</remarks>
        public GraphNodePath GetTargetNodePath()
        {
            return GetTargetNodePath(SourceNode, Index, SourceNodePath);
        }

        protected void AssertInit()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("Accessing a property of a non-initialized NodeViewModel.");
            }
        }

        /// <summary>
        /// Retrieve the value of the model content associated to this <see cref="GraphNodeViewModel"/>.
        /// </summary>
        /// <returns>The value of the model content associated to this <see cref="GraphNodeViewModel"/>.</returns>
        protected object GetModelContentValue()
        {
            return SourceNode.Retrieve(Index);
        }

        /// <summary>
        /// Sets the value of the model content associated to this <see cref="GraphNodeViewModel"/>. The value is actually modified only if the new value is different from the previous value.
        /// </summary>
        /// <returns><c>True</c> if the value has been modified, <c>false</c> otherwise.</returns>
        protected virtual bool SetModelContentValue(IContentNode node, object newValue)
        {
            //if (Index == Index.Empty)
            //{
            //    var oldValue = node.Retrieve();
            //    if (!Equals(oldValue, newValue))
            //    {
            //        ((IMemberNode)node).Update(newValue);
            //        return true;
            //    }
            //}
            //else
            //{
            //    var targetNode = GetTargetNode(node, Index.Empty);
            //    var oldValue = node.Retrieve(Index);
            //    if (!Equals(oldValue, newValue))
            //    {
            //        targetNode.Update(newValue, Index);
            //        return true;
            //    }
            //}
            return false;
        }

        private void GenerateChildren(IContentNode targetNode, GraphNodePath targetNodePath, Index index)
        {
            //// Set the default policy for expanding reference children.
            //ExpandReferencePolicy = ExpandReferencePolicy.Full;

            //    var objectReference = SourceNode.TargetReference ?? SourceNode.ItemReferences?[index];
            //    // Discard the children of the referenced object if requested by the property provider
            //    if (objectReference != null)
            //    {
            //        ExpandReferencePolicy = Owner.PropertiesProvider.ShouldExpandReference(SourceNode as MemberContent, objectReference);
            //        if (ExpandReferencePolicy == ExpandReferencePolicy.None)
            //            return;
            //    }
            //}

            //var dictionary = targetNode.Descriptor as DictionaryDescriptor;
            //var list = targetNode.Descriptor as CollectionDescriptor;
            //var initializedChildren = new List<NodeViewModel>();

            // Node containing a collection of references to other objects
            //if (SourceNode == targetNode && targetNode.IsReference)
            //{
            //    var referenceEnumerable = targetNode.ItemReferences;
            //    if (referenceEnumerable != null)
            //    {
            //        // We create one node per item of the collection, we will check later if the reference should be expanded.
            //        foreach (var reference in referenceEnumerable)
            //        {
            //            // The type might be a boxed primitive type, such as float, if the collection has object as generic argument.
            //            // In this case, we must set the actual type to have type converter working, since they usually can't convert
            //            // a boxed float to double for example. Otherwise, we don't want to have a node type that is value-dependent.
            //            var type = reference.TargetNode != null && reference.TargetNode.IsPrimitive ? reference.TargetNode.Type : referenceEnumerable.ElementType;
            //            var child = Owner.GraphViewModelService.GraphNodeViewModelFactory(Owner, null, false, targetNode, targetNodePath, type, reference.Index);
            //            AddChild(child);
            //            child.Initialize();
            //            initializedChildren.Add(child);
            //        }
            //    }
            //}
            //// Node containing a dictionary of primitive values
            //else if (dictionary != null && targetNode.Retrieve() != null)
            //{
            //    // TODO: there is no way to discard items of such collections, without discarding the collection itself. Could this be needed at some point?
            //    // We create one node per item of the collection.
            //    foreach (var key in dictionary.GetKeys(targetNode.Retrieve()))
            //    {
            //        var newIndex = new Index(key);
            //        var child = Owner.GraphViewModelService.GraphNodeViewModelFactory(Owner, null, true, targetNode, targetNodePath, dictionary.ValueType, newIndex);
            //        AddChild(child);
            //        child.Initialize();
            //        initializedChildren.Add(child);
            //    }
            //}
            //// Node containing a list of primitive values
            //else if (list != null && targetNode.Retrieve() != null)
            //{
            //    // TODO: there is no way to discard items of such collections, without discarding the collection itself. Could this be needed at some point?
            //    // We create one node per item of the collection.
            //    for (int i = 0; i < list.GetCollectionCount(targetNode.Retrieve()); ++i)
            //    {
            //        var newIndex = new Index(i);
            //        var child = Owner.GraphViewModelService.GraphNodeViewModelFactory(Owner, null, true, targetNode, targetNodePath, list.ElementType, newIndex);
            //        AddChild(child);
            //        child.Initialize();
            //        initializedChildren.Add(child);
            //    }
            //}
            //// Node containing a single non-reference primitive object
            //else
            //{
            //    var objectContent = (IObjectNode)targetNode;
            //    foreach (var memberContent in objectContent.Members)
            //    {
            //        var descriptor = (MemberDescriptorBase)memberContent.MemberDescriptor;
            //        var displayAttribute = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<DisplayAttribute>(descriptor.MemberInfo);
            //        if (displayAttribute == null || displayAttribute.Browsable)
            //        {
            //            // The path is the source path here - the target path might contain the target resolution that we don't want at that point
            //            if (Owner.PropertiesProvider.ShouldConstructMember(memberContent, ExpandReferencePolicy))
            //            {
            //                var childPath = targetNodePath.PushMember(memberContent.Name);
            //                var child = Owner.GraphViewModelService.GraphNodeViewModelFactory(Owner, memberContent.Name, memberContent.IsPrimitive, memberContent, childPath, memberContent.Type, Index.Empty);
            //                AddChild(child);
            //                child.Initialize();
            //                initializedChildren.Add(child);
            //            }
            //        }
            //    }
            //}

            //// Call FinalizeInitialization on all created nodes after they were all initialized.
            //foreach (var child in initializedChildren)
            //{
            //    child.FinalizeInitialization();
            //}
        }

        /// <summary>
        /// Refreshes the node commands and children. The source and target model nodes must have been updated first.
        /// </summary>
        protected override void Refresh()
        {
            if (Parent == null) throw new InvalidOperationException("The node to refresh can't be a root node.");
            
            OnPropertyChanging(nameof(IsPrimitive), nameof(HasCollection), nameof(HasDictionary));

            // Clean the current node so it can be re-initialized (associatedData are overwritten in Initialize)
            ClearCommands();

            // Dispose all children and remove them
            Children.SelectDeep(x => x.Children).ForEach(x => x.Destroy());
            foreach (var child in Children.Cast<NodeViewModel>().ToList())
            {
                RemoveChild(child);
            }

            foreach (var key in AssociatedData.Keys.ToList())
            {
                RemoveAssociatedData(key);
            }

            Initialize();
            FinalizeInitialization();

            if (DisplayNameProvider != null)
            {
                DisplayName = DisplayNameProvider();
            }
            OnPropertyChanged(nameof(IsPrimitive), nameof(HasCollection), nameof(HasDictionary));
        }

        /// <summary>
        /// Retrieves the target node if the given source node content holds a reference or a sequence of references, or the given source node otherwise.
        /// </summary>
        /// <param name="sourceNode">The source node for which to retrieve the target node.</param>
        /// <param name="index">The index of the target node to retrieve, if the source node contains a sequence of references. <see cref="Index.Empty"/> otherwise.</param>
        /// <returns>The corresponding target node if available, or the source node itself if it does not contain any reference or if its content should not process references.</returns>
        /// <remarks>This method can return null if the target node is null.</remarks>
        protected static IContentNode GetTargetNode(IContentNode sourceNode, Index index)
        {
            //if (sourceNode == null) throw new ArgumentNullException(nameof(sourceNode));

            //var objectReference = (sourceNode as IMemberNode)?.TargetReference;
            //if (objectReference != null)
            //{
            //    return objectReference.TargetNode;
            //}

            //var referenceEnumerable = sourceNode.ItemReferences;
            //if (referenceEnumerable != null && !index.IsEmpty)
            //{
            //    return referenceEnumerable[index].TargetNode;
            //}

            return sourceNode;
        }

        /// <summary>
        /// Retrieves the path of the target node if the given source node content holds a reference or a sequence of references, or the given source node path otherwise.
        /// </summary>
        /// <param name="sourceNode">The source node for which to retrieve the target node.</param>
        /// <param name="index">The index of the target node to retrieve, if the source node contains a sequence of references. <see cref="Index.Empty"/> otherwise.</param>
        /// <param name="sourceNodePath">The path to the given <paramref name="sourceNode"/>.</param>
        /// <returns>The path to the corresponding target node if available, or the path to source node itself if it does not contain any reference or if its content should not process references.</returns>
        /// <remarks>This method can return null if the target node is null.</remarks>
        protected static GraphNodePath GetTargetNodePath(IContentNode sourceNode, Index index, GraphNodePath sourceNodePath)
        {
            //if (sourceNode == null) throw new ArgumentNullException(nameof(sourceNode));
            //if (sourceNodePath == null) throw new ArgumentNullException(nameof(sourceNodePath));

            //var objectReference = (sourceNode as IMemberNode)?.TargetReference;
            //if (objectReference != null)
            //{
            //    return sourceNodePath.PushTarget();
            //}

            //var referenceEnumerable = sourceNode.ItemReferences;
            //if (referenceEnumerable != null && !index.IsEmpty)
            //{
            //    return sourceNodePath.PushIndex(index);
            //}

            return sourceNodePath.Clone();
        }
    }

    public class GraphNodeViewModel<T> : GraphNodeViewModel
    {
        /// <summary>
        /// Construct a new <see cref="GraphNodeViewModel"/>.
        /// </summary>
        /// <param name="ownerViewModel">The <see cref="GraphViewModel"/> that owns the new <see cref="GraphNodeViewModel"/>.</param>
        /// <param name="baseName">The base name of this node. Can be null if <see cref="index"/> is not. If so a name will be automatically generated from the index.</param>
        /// <param name="isPrimitive">Indicate whether this node should be considered as a primitive node.</param>
        /// <param name="modelNode">The model node bound to the new <see cref="GraphNodeViewModel"/>.</param>
        /// <param name="graphNodePath">The <see cref="GraphNodePath"/> corresponding to the given <see cref="modelNode"/>.</param>
        /// <param name="index">The index of this content in the model node, when this node represent an item of a collection.<see cref="Index.Empty"/> must be passed otherwise</param>
        public GraphNodeViewModel(GraphViewModel ownerViewModel, string baseName, bool isPrimitive, IContentNode modelNode, GraphNodePath graphNodePath, Index index)
            : base(ownerViewModel, baseName, isPrimitive, modelNode, graphNodePath, index)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            DependentProperties.Add(nameof(TypedValue), new[] { nameof(Value) });
            SourceNode.RegisterChanging(ContentChanging);
            SourceNode.RegisterChanged(ContentChanged);
        }

        /// <summary>
        /// Gets or sets the value of this node through a correctly typed property, which is more adapted to binding.
        /// </summary>
        public virtual T TypedValue { get { return (T)GetModelContentValue(); } set { AssertInit(); SetModelContentValue(SourceNode, value); } }

        /// <inheritdoc/>
        public override Type Type => typeof(T);

        /// <inheritdoc/>
        public sealed override object Value { get { return TypedValue; } set { TypedValue = (T)value; } }

        /// <inheritdoc/>
        public override void Destroy()
        {
            SourceNode.UnregisterChanging(ContentChanging);
            SourceNode.UnregisterChanged(ContentChanged);
            base.Destroy();
        }

        private void ContentChanging(object sender, INodeChangeEventArgs e)
        {
            if (IsValidChange(e))
            {
                ((NodeViewModel)Parent)?.NotifyPropertyChanging(Name);
                OnPropertyChanging(nameof(TypedValue));
            }
        }

        private void ContentChanged(object sender, INodeChangeEventArgs e)
        {
            if (IsValidChange(e))
            {
                ((NodeViewModel)Parent)?.NotifyPropertyChanged(Name);

                // This node can have been disposed by its parent already (if its parent is being refreshed and share the same source node)
                // In this case, let's trigger the notifications gracefully before being discarded, but skip refresh
                if (!IsPrimitive && !IsDestroyed && !(Value?.GetType().IsStruct() ?? false))
                {
                    Refresh();
                }

                OnPropertyChanged(nameof(TypedValue));
                OnValueChanged();
                Owner.NotifyNodeChanged(Path);
            }
        }

        private bool IsValidChange(INodeChangeEventArgs e)
        {
            switch (e.ChangeType)
            {
                case ContentChangeType.ValueChange:
                case ContentChangeType.CollectionUpdate:
                    return Equals(e.Index, Index);
                case ContentChangeType.CollectionAdd:
                case ContentChangeType.CollectionRemove:
                    return HasCollection || HasDictionary; // TODO: probably not sufficent
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
