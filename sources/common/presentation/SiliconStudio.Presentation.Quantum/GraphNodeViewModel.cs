// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum
{
    public class GraphNodeViewModel : SingleNodeViewModel, IGraphNodeViewModel
    {
        public readonly IGraphNode SourceNode;
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
        protected internal GraphNodeViewModel(GraphViewModel ownerViewModel, Type type, string baseName, bool isPrimitive, IGraphNode sourceNode, GraphNodePath graphNodePath, Index index)
            : base(ownerViewModel, type, baseName, index)
        {
            DependentProperties.Add(nameof(InternalNodeValue), new[] { nameof(NodeValue) });

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

            var memberNode = SourceNode as IMemberNode;
            if (memberNode != null)
            {
                memberNode.Changing += ContentChanging;
                memberNode.Changed += ContentChanged;
                var targetNode = GetTargetNode(memberNode, Index.Empty) as IObjectNode;
                if (targetNode != null)
                {
                    targetNode.ItemChanging += ContentChanging;
                    targetNode.ItemChanged += ContentChanged;
                }
            }
            var objectNode = SourceNode as IObjectNode;
            if (objectNode != null)
            {
                objectNode.ItemChanging += ContentChanging;
                objectNode.ItemChanged += ContentChanged;
            }
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            var memberNode = SourceNode as IMemberNode;
            if (memberNode != null)
            {
                memberNode.Changing -= ContentChanging;
                memberNode.Changed -= ContentChanged;
                var targetNode = GetTargetNode(memberNode, Index.Empty) as IObjectNode;
                if (targetNode != null)
                {
                    targetNode.ItemChanging -= ContentChanging;
                    targetNode.ItemChanged -= ContentChanged;
                }
            }
            var objectNode = SourceNode as IObjectNode;
            if (objectNode != null)
            {
                objectNode.ItemChanging -= ContentChanging;
                objectNode.ItemChanged -= ContentChanged;
            }
            base.Destroy();
        }

        /// <summary>
        /// A function that indicates if the given value can be accepted as new value for this node.
        /// </summary>
        public Func<object, bool> AcceptValueCallback { get; set; }

        /// <summary>
        /// A function that coerces the given value before setting it as new value for this node.
        /// </summary>
        public Func<object, object> CoerceValueCallback { get; set; }

        /// <summary>
        /// Initializes this node. This method is called right after construction of the node, and after <see cref="NodeViewModel.AddChild"/> as been called on its parent if this node has a parent.
        /// </summary>
        protected internal void Initialize()
        {
            var targetNode = GetTargetNode(SourceNode, Index);

            //if (targetNode != SourceNode && targetNode != null)
            //{
            //    foreach (var command in targetNode.Commands)
            //    {
            //        var commandWrapper = new ModelNodeCommandWrapper(ServiceProvider, command, SourceNodePath, Index);
            //        AddCommand(commandWrapper);
            //    }
            //}

            //var targetCommandNames = Commands.Select(x => x.Name).ToList();
            //foreach (var command in SourceNode.Commands)
            //{
            //    // Add source commands that are not already provided by the target node
            //    if (!targetCommandNames.Contains(command.Name))
            //    {
            //        var commandWrapper = new ModelNodeCommandWrapper(ServiceProvider, command, SourceNodePath, Index);
            //        AddCommand(commandWrapper);
            //    }
            //}

            if (!isPrimitive && targetNode != null)
            {
                var targetNodePath = GetTargetNodePath();
                if (targetNodePath == null)
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

        protected GraphNodePath SourceNodePath { get; }

        /// <inheritdoc/>
        public override int? Order
        {
            get
            {
                if (CustomOrder != null)
                    return CustomOrder;

                var memberContent = SourceNode as IMemberNode;
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
                var memberContent = SourceNode as IMemberNode;
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

        /// <inheritdoc/>
        protected internal sealed override object InternalNodeValue { get { return GetNodeValue(); } set { AssertInit(); SetNodeValue(SourceNode, value); } }

        // TODO: If possible, make this private, it's not a good thing to expose
        public IMemberDescriptor GetMemberDescriptor()
        {
            var memberContent = SourceNode as IMemberNode;
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

            var modelContentValue = GetNodeValue();
            if (!Equals(modelContentValue, InternalNodeValue))
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
        protected object GetNodeValue()
        {
            return SourceNode.Retrieve(Index);
        }

        /// <summary>
        /// Sets the value of the model content associated to this <see cref="GraphNodeViewModel"/>. The value is actually modified only if the new value is different from the previous value.
        /// </summary>
        /// <returns><c>True</c> if the value has been modified, <c>false</c> otherwise.</returns>
        protected virtual bool SetNodeValue(IGraphNode node, object newValue)
        {
            // Check to accept the value
            if (AcceptValueCallback?.Invoke(newValue) == false)
                return false;

            // Coerce the value if needed
            var coerceCallback = CoerceValueCallback;
            if (coerceCallback != null)
            {
                newValue = coerceCallback.Invoke(newValue);
            }

            if (Index == Index.Empty)
            {
                var oldValue = node.Retrieve();
                if (!Equals(oldValue, newValue))
                {
                    ((IMemberNode)node).Update(newValue);
                    return true;
                }
            }
            else
            {
                var oldValue = node.Retrieve(Index);
                if (!Equals(oldValue, newValue))
                {
                    ((IObjectNode)node).Update(newValue, Index);
                    return true;
                }
            }
            return false;
        }

        private void GenerateChildren(IGraphNode targetNode, GraphNodePath targetNodePath, Index index)
        {
            // Set the default policy for expanding reference children.
            var initializedChildren = new List<NodeViewModel>();

            var objectNode = targetNode as IObjectNode;
            if (objectNode != null)
            {
                GenerateMembers(objectNode, targetNodePath, initializedChildren);
                GenerateItems(objectNode, targetNodePath, initializedChildren);
            }

            // Call FinalizeInitialization on all created nodes after they were all initialized.
            foreach (var child in initializedChildren)
            {
                child.FinalizeInitialization();
            }
        }

        private void GenerateMembers(IObjectNode objectNode, GraphNodePath targetNodePath, List<NodeViewModel> initializedChildren)
        {
            foreach (var memberContent in objectNode.Members)
            {
                var descriptor = (MemberDescriptorBase)memberContent.MemberDescriptor;
                var displayAttribute = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<DisplayAttribute>(descriptor.MemberInfo);
                if (displayAttribute == null || displayAttribute.Browsable)
                {
                    // The path is the source path here - the target path might contain the target resolution that we don't want at that point
                    if (Owner.PropertiesProvider.ShouldConstructMember(memberContent))
                    {
                        var childPath = targetNodePath.Clone();
                        childPath.PushMember(memberContent.Name);
                        var child = Owner.GraphViewModelService.GraphNodeViewModelFactory(Owner, memberContent.Name, memberContent.IsPrimitive, memberContent, childPath, memberContent.Type, Index.Empty);
                        AddChild(child);
                        child.Initialize();
                        initializedChildren.Add(child);
                    }
                }
            }
        }

        private void GenerateItems(IObjectNode objectNode, GraphNodePath targetNodePath, List<NodeViewModel> initializedChildren)
        {
            var referenceEnumerable = objectNode.ItemReferences;
            var dictionary = objectNode.Descriptor as DictionaryDescriptor;
            var list = objectNode.Descriptor as CollectionDescriptor;

            if (referenceEnumerable != null)
            {
                // Case 1: the target is a collection of non-primitive values
                // We create one node per item of the collection, we will check later if the reference should be expanded.
                foreach (var reference in referenceEnumerable)
                {
                    if (Owner.PropertiesProvider.ShouldConstructItem(objectNode, reference.Index))
                    {
                        // The type might be a boxed primitive type, such as float, if the collection has object as generic argument.
                        // In this case, we must set the actual type to have type converter working, since they usually can't convert
                        // a boxed float to double for example. Otherwise, we don't want to have a node type that is value-dependent.
                        var type = reference.TargetNode != null && reference.TargetNode.IsPrimitive ? reference.TargetNode.Type : referenceEnumerable.ElementType;
                        var child = Owner.GraphViewModelService.GraphNodeViewModelFactory(Owner, null, false, objectNode, targetNodePath, type, reference.Index);
                        AddChild(child);
                        child.Initialize();
                        initializedChildren.Add(child);
                    }
                }
            }
            else if (dictionary != null && objectNode.Retrieve() != null)
            {
                // Case 2: the target is a dictionary of primitive values
                // We create one node per item of the collection.
                foreach (var key in dictionary.GetKeys(objectNode.Retrieve()))
                {
                    var newIndex = new Index(key);
                    if (Owner.PropertiesProvider.ShouldConstructItem(objectNode, newIndex))
                    {
                        var child = Owner.GraphViewModelService.GraphNodeViewModelFactory(Owner, null, true, objectNode, targetNodePath, dictionary.ValueType, newIndex);
                        AddChild(child);
                        child.Initialize();
                        initializedChildren.Add(child);
                    }
                }
            }
            else if (list != null && objectNode.Retrieve() != null)
            {
                // Case 3: the target is a list of primitive values
                // We create one node per item of the collection.
                for (var i = 0; i < list.GetCollectionCount(objectNode.Retrieve()); ++i)
                {
                    var newIndex = new Index(i);
                    if (Owner.PropertiesProvider.ShouldConstructItem(objectNode, newIndex))
                    {
                        var child = Owner.GraphViewModelService.GraphNodeViewModelFactory(Owner, null, true, objectNode, targetNodePath, list.ElementType, newIndex);
                        AddChild(child);
                        child.Initialize();
                        initializedChildren.Add(child);
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes the node commands and children. The source and target model nodes must have been updated first.
        /// </summary>
        protected override void Refresh()
        {
            //if (Parent == null) throw new InvalidOperationException("The node to refresh can't be a root node.");

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
        protected static IGraphNode GetTargetNode(IGraphNode sourceNode, Index index)
        {
            if (sourceNode == null) throw new ArgumentNullException(nameof(sourceNode));

            var objectReference = (sourceNode as IMemberNode)?.TargetReference;
            if (objectReference != null)
            {
                return objectReference.TargetNode;
            }

            var referenceEnumerable = (sourceNode as IObjectNode)?.ItemReferences;
            if (referenceEnumerable != null && !index.IsEmpty)
            {
                return referenceEnumerable[index].TargetNode;
            }

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
        protected static GraphNodePath GetTargetNodePath(IGraphNode sourceNode, Index index, GraphNodePath sourceNodePath)
        {
            if (sourceNode == null) throw new ArgumentNullException(nameof(sourceNode));
            if (sourceNodePath == null) throw new ArgumentNullException(nameof(sourceNodePath));

            var targetPath = sourceNodePath.Clone();
            var objectReference = (sourceNode as IMemberNode)?.TargetReference;
            if (objectReference != null)
            {
                targetPath.PushTarget();
            }

            var referenceEnumerable = (sourceNode as IObjectNode)?.ItemReferences;
            if (referenceEnumerable != null && !index.IsEmpty)
            {
                targetPath.PushIndex(index);
            }

            return targetPath;
        }

        private void ContentChanging(object sender, INodeChangeEventArgs e)
        {
            if (IsValidChange(e))
            {
                ((NodeViewModel)Parent)?.NotifyPropertyChanging(Name);
                OnPropertyChanging(nameof(InternalNodeValue));
            }
        }

        private void ContentChanged(object sender, INodeChangeEventArgs e)
        {
            if (IsValidChange(e))
            {
                ((NodeViewModel)Parent)?.NotifyPropertyChanged(Name);

                // This node can have been disposed by its parent already (if its parent is being refreshed and share the same source node)
                // In this case, let's trigger the notifications gracefully before being discarded, but skip refresh
                if (!IsPrimitive && !IsDestroyed)
                {
                    Refresh();
                }

                OnPropertyChanged(nameof(InternalNodeValue));
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
