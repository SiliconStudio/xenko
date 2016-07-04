// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Presentation.Quantum
{
    public abstract class ObservableModelNode : SingleObservableNode
    {
        public readonly IGraphNode SourceNode;
        protected readonly GraphNodePath SourceNodePath;
        private readonly bool isPrimitive;
        private bool isInitialized;
        private int? customOrder;

        static ObservableModelNode()
        {
            typeof(ObservableModelNode).GetProperties().Select(x => x.Name).ForEach(x => ReservedNames.Add(x));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableModelNode"/> class.
        /// </summary>
        /// <param name="ownerViewModel">The <see cref="ObservableViewModel"/> that owns the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="baseName">The base name of this node. Can be null if <see cref="index"/> is not. If so a name will be automatically generated from the index.</param>
        /// <param name="isPrimitive">Indicate whether this node should be considered as a primitive node.</param>
        /// <param name="sourceNode">The model node bound to the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="graphNodePath">The <see cref="GraphNodePath"/> corresponding to the given <see cref="sourceNode"/>.</param>
        /// <param name="index">The index of this content in the model node, when this node represent an item of a collection. <see cref="Index.Empty"/> must be passed otherwise</param>
        protected ObservableModelNode(ObservableViewModel ownerViewModel, string baseName, bool isPrimitive, IGraphNode sourceNode, GraphNodePath graphNodePath, Index index)
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
        /// Create an <see cref="ObservableModelNode{T}"/> that matches the given content type.
        /// </summary>
        /// <param name="ownerViewModel">The <see cref="ObservableViewModel"/> that owns the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="baseName">The base name of this node. Can be null if <see cref="index"/> is not. If so a name will be automatically generated from the index.</param>
        /// <param name="isPrimitive">Indicate whether this node should be considered as a primitive node.</param>
        /// <param name="sourceNode">The model node bound to the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="graphNodePath">The <see cref="GraphNodePath"/> corresponding to the given node.</param>
        /// <param name="contentType">The type of content contained by the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="index">The index of this content in the model node, when this node represent an item of a collection. <see cref="Index.Empty"/> must be passed otherwise</param>
        /// <returns>A new instance of <see cref="ObservableModelNode{T}"/> instanced with the given content type as generic argument.</returns>
        internal static ObservableModelNode Create(ObservableViewModel ownerViewModel, string baseName, bool isPrimitive, IGraphNode sourceNode, GraphNodePath graphNodePath, Type contentType, Index index)
        {
            var node = (ObservableModelNode)Activator.CreateInstance(typeof(ObservableModelNode<>).MakeGenericType(contentType), ownerViewModel, baseName, isPrimitive, sourceNode, graphNodePath, index);
            return node;
        }

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
                var targetNodePath = GetTargetNodePath(SourceNode, Index, SourceNodePath);
                if (targetNodePath == null || !targetNodePath.IsValid)
                    throw new InvalidOperationException("Unable to retrieve the path of the given model node.");

                GenerateChildren(targetNode, targetNodePath);
            }

            isInitialized = true;

            Owner.ObservableViewModelService?.NotifyNodeInitialized(this);

            FinalizeChildrenInitialization();

            CheckDynamicMemberConsistency();
        }

        /// <inheritdoc/>
        public override int? Order => CustomOrder ?? (SourceNode.Content is MemberContent && Index.IsEmpty ? ((MemberContent)SourceNode.Content).Member.Order : null);

        /// <summary>
        /// Gets or sets a custom value for the <see cref="Order"/> of this node.
        /// </summary>
        public int? CustomOrder { get { return customOrder; } set { SetValue(ref customOrder, value, nameof(CustomOrder), nameof(Order)); } }

        /// <inheritdoc/>
        public sealed override bool IsPrimitive => isPrimitive;

        /// <inheritdoc/>
        public sealed override bool HasList => CollectionDescriptor.IsCollection(Type);

        /// <inheritdoc/>
        public sealed override bool HasDictionary => DictionaryDescriptor.IsDictionary(Type);

        // The previous way to compute HasList and HasDictionary was quite complex, but let's keep it here for history. 
        // To distinguish between lists and items of a list (which have the same TargetNode if the items are primitive types), we check whether the TargetNode is
        // the same of the one of its parent. If so, we're likely in an item of a list of primitive objects. 
        //public sealed override bool HasList => (targetNode.Content.Descriptor is CollectionDescriptor && (Parent == null || (ModelNodeParent != null && ModelNodeParent.targetNode.Content.Value != targetNode.Content.Value))) || (targetNode.Content.ShouldProcessReference && targetNode.Content.Reference is ReferenceEnumerable);
        // To distinguish between dictionaries and items of a dictionary (which have the same TargetNode if the value type is a primitive type), we check whether the TargetNode is
        // the same of the one of its parent. If so, we're likely in an item of a dictionary of primitive objects. 
        //public sealed override bool HasDictionary => (targetNode.Content.Descriptor is DictionaryDescriptor && (Parent == null || (ModelNodeParent != null && ModelNodeParent.targetNode.Content.Value != targetNode.Content.Value))) || (targetNode.Content.ShouldProcessReference && targetNode.Content.Reference is ReferenceEnumerable && ((ReferenceEnumerable)targetNode.Content.Reference).IsDictionary);

        internal Guid ModelGuid => SourceNode.Guid;
   
        /// <summary>
        /// Indicates whether this <see cref="ObservableModelNode"/> instance corresponds to the given <see cref="IGraphNode"/>.
        /// </summary>
        /// <param name="node">The node to match.</param>
        /// <returns><c>true</c> if the node matches, <c>false</c> otherwise.</returns>
        public bool MatchNode(IGraphNode node)
        {
            return SourceNode == node;
        }

        // TODO: If possible, make this private, it's not a good thing to expose
        public IMemberDescriptor GetMemberDescriptor()
        {
            var memberContent = SourceNode.Content as MemberContent;
            return memberContent?.Member;
        }

        internal void CheckConsistency()
        {
#if DEBUG
            var targetNode = GetTargetNode(SourceNode, Index);
            if (SourceNode != targetNode)
            {
                var objectReference = SourceNode.Content.Reference as ObjectReference;
                var referenceEnumerable = SourceNode.Content.Reference as ReferenceEnumerable;
                if (objectReference != null && targetNode != objectReference.TargetNode)
                {
                    throw new ObservableViewModelConsistencyException(this, "The target node does not match the target of the source node object reference.");
                }
                if (referenceEnumerable != null && !Index.IsEmpty)
                {
                    if (!referenceEnumerable.ContainsIndex(Index))
                        throw new ObservableViewModelConsistencyException(this, "The Index of this node does not exist in the reference of its source node.");

                    if (targetNode != referenceEnumerable[Index].TargetNode)
                    {
                        throw new ObservableViewModelConsistencyException(this, "The target node does not match the target of the source node object reference.");
                    }
                }
            }

            var modelContentValue = GetModelContentValue();
            if (!Equals(modelContentValue, Value))
            {
                // TODO: I had this exception with a property that is returning a new IEnumerable each time - we should have a way to notice this, maybe by correctly transfering and checking the IsReadOnly property
                //throw new ObservableViewModelConsistencyException(this, "The value of this node does not match the value of its source node content.");
            }

            foreach (var child in Children.OfType<ObservableModelNode>())
            {
                if (targetNode.Content.IsReference)
                {
                    var objectReference = targetNode.Content.Reference as ObjectReference;
                    if (objectReference != null)
                    {
                        throw new ObservableViewModelConsistencyException(this, "The target node does not match the target of the source node object reference.");
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

        protected void AssertInit()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("Accessing a property of a non-initialized ObservableNode.");
            }
        }

        /// <summary>
        /// Retrieve the value of the model content associated to this <see cref="ObservableModelNode"/>.
        /// </summary>
        /// <returns>The value of the model content associated to this <see cref="ObservableModelNode"/>.</returns>
        protected object GetModelContentValue()
        {
            return SourceNode.Content.Retrieve(Index);
        }

        /// <summary>
        /// Sets the value of the model content associated to this <see cref="ObservableModelNode"/>. The value is actually modified only if the new value is different from the previous value.
        /// </summary>
        /// <returns><c>True</c> if the value has been modified, <c>false</c> otherwise.</returns>
        protected bool SetModelContentValue(IGraphNode node, object newValue)
        {
            var oldValue = node.Content.Retrieve(Index);
            if (!Equals(oldValue, newValue))
            {
                node.Content.Update(newValue, Index);
                return true;
            }
            return false;
        }

        private void GenerateChildren(IGraphNode targetNode, GraphNodePath targetNodePath)
        {
            // Node representing a member with a reference to another object
            if (SourceNode != targetNode && SourceNode.Content.IsReference)
            {
                var objectReference = SourceNode.Content.Reference as ObjectReference;
                // Discard the children of the referenced object if requested by the property provider
                if (objectReference != null && !Owner.PropertiesProvider.ShouldExpandReference(SourceNode.Content as MemberContent, objectReference))
                    return;
            }

            var dictionary = targetNode.Content.Descriptor as DictionaryDescriptor;
            var list = targetNode.Content.Descriptor as CollectionDescriptor;

            // Node containing a collection of references to other objects
            if (targetNode.Content.IsReference)
            {
                var referenceEnumerable = targetNode.Content.Reference as ReferenceEnumerable;
                if (referenceEnumerable != null)
                {
                    // We create one node per item of the collection, unless requested by the property provide to not expand the reference.
                    foreach (var reference in referenceEnumerable)
                    {
                        // The type might be a boxed primitive type, such as float, if the collection has object as generic argument.
                        // In this case, we must set the actual type to have type converter working, since they usually can't convert
                        // a boxed float to double for example. Otherwise, we don't want to have a node type that is value-dependent.
                        var type = reference.TargetNode != null && reference.TargetNode.Content.IsPrimitive ? reference.TargetNode.Content.Type : reference.Type;
                        var actualPath = targetNodePath.PushIndex(reference.Index);
                        if (Owner.PropertiesProvider.ShouldExpandReference(SourceNode.Content as MemberContent, reference))
                        {
                            var observableNode = Owner.ObservableViewModelService.ObservableNodeFactory(Owner, null, false, targetNode, targetNodePath, type, reference.Index);
                            AddChild(observableNode);
                            observableNode.Initialize();
                        }
                    }
                }
            }
            // Node containing a dictionary of primitive values
            else if (dictionary != null && targetNode.Content.Value != null)
            {
                // TODO: there is no way to discard items of such collections, without discarding the collection itself. Could this be needed at some point?
                // We create one node per item of the collection.
                foreach (var key in dictionary.GetKeys(targetNode.Content.Value))
                {
                    var index = new Index(key);
                    var observableChild = Owner.ObservableViewModelService.ObservableNodeFactory(Owner, null, true, targetNode, targetNodePath, dictionary.ValueType, index);
                    AddChild(observableChild);
                    observableChild.Initialize();
                }
            }
            // Node containing a list of primitive values
            else if (list != null && targetNode.Content.Value != null)
            {
                // TODO: there is no way to discard items of such collections, without discarding the collection itself. Could this be needed at some point?
                // We create one node per item of the collection.
                for (int i = 0; i < list.GetCollectionCount(targetNode.Content.Value); ++i)
                {
                    var index = new Index(i);
                    var observableChild = Owner.ObservableViewModelService.ObservableNodeFactory(Owner, null, true, targetNode, targetNodePath, list.ElementType, index);
                    AddChild(observableChild);
                    observableChild.Initialize();
                }
            }
            // Node containing a single non-reference primitive object
            else
            {
                foreach (var child in targetNode.Children)
                {
                    var memberContent = (MemberContent)child.Content;
                    var descriptor = (MemberDescriptorBase)memberContent.Member;
                    var displayAttribute = TypeDescriptorFactory.Default.AttributeRegistry.GetAttribute<DisplayAttribute>(descriptor.MemberInfo);
                    if (displayAttribute == null || displayAttribute.Browsable)
                    {
                        // The path is the source path here - the target path might contain the target resolution that we don't want at that point
                        if (Owner.PropertiesProvider.ShouldConstructMember(memberContent))
                        {
                            var childPath = targetNodePath.PushMember(child.Name);
                            var observableChild = Owner.ObservableViewModelService.ObservableNodeFactory(Owner, child.Name, child.Content.IsPrimitive, child, childPath, child.Content.Type, Index.Empty);
                            AddChild(observableChild);
                            observableChild.Initialize();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes the node commands and children. The source and target model nodes must have been updated first.
        /// </summary>
        protected void Refresh()
        {
            if (Parent == null) throw new InvalidOperationException("The node to refresh can't be a root node.");
            
            OnPropertyChanging(nameof(IsPrimitive), nameof(HasList), nameof(HasDictionary));

            // Clean the current node so it can be re-initialized (associatedData are overwritten in Initialize)
            ClearCommands();

            // Dispose all children and remove them
            Children.SelectDeep(x => x.Children).ForEach(x => x.Destroy());
            foreach (var child in Children.Cast<ObservableNode>().ToList())
            {
                RemoveChild(child);
            }

            foreach (var key in AssociatedData.Keys.ToList())
            {
                RemoveAssociatedData(key);
            }

            Initialize();

            if (DisplayNameProvider != null)
            {
                DisplayName = DisplayNameProvider();
            }
            OnPropertyChanged(nameof(IsPrimitive), nameof(HasList), nameof(HasDictionary));
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

            var objectReference = sourceNode.Content.Reference as ObjectReference;
            if (objectReference != null)
            {
                return objectReference.TargetNode;
            }

            var referenceEnumerable = sourceNode.Content.Reference as ReferenceEnumerable;
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

            var objectReference = sourceNode.Content.Reference as ObjectReference;
            if (objectReference != null)
            {
                return sourceNodePath.PushTarget();
            }

            var referenceEnumerable = sourceNode.Content.Reference as ReferenceEnumerable;
            if (referenceEnumerable != null && !index.IsEmpty)
            {
                return sourceNodePath.PushIndex(index);
            }

            return sourceNodePath.Clone();
        }
    }

    public class ObservableModelNode<T> : ObservableModelNode
    {
        /// <summary>
        /// Construct a new <see cref="ObservableModelNode"/>.
        /// </summary>
        /// <param name="ownerViewModel">The <see cref="ObservableViewModel"/> that owns the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="baseName">The base name of this node. Can be null if <see cref="index"/> is not. If so a name will be automatically generated from the index.</param>
        /// <param name="isPrimitive">Indicate whether this node should be considered as a primitive node.</param>
        /// <param name="modelNode">The model node bound to the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="graphNodePath">The <see cref="GraphNodePath"/> corresponding to the given <see cref="modelNode"/>.</param>
        /// <param name="index">The index of this content in the model node, when this node represent an item of a collection.<see cref="Index.Empty"/> must be passed otherwise</param>
        public ObservableModelNode(ObservableViewModel ownerViewModel, string baseName, bool isPrimitive, IGraphNode modelNode, GraphNodePath graphNodePath, Index index)
            : base(ownerViewModel, baseName, isPrimitive, modelNode, graphNodePath, index)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            DependentProperties.Add(nameof(TypedValue), new[] { nameof(Value) });
            SourceNode.Content.Changing += ContentChanging;
            SourceNode.Content.Changed += ContentChanged;
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
            SourceNode.Content.Changing -= ContentChanging;
            SourceNode.Content.Changed -= ContentChanged;
            base.Destroy();
        }

        private void ContentChanging(object sender, ContentChangeEventArgs e)
        {
            if (IsValidChange(e))
            {
                ((ObservableNode)Parent)?.NotifyPropertyChanging(Name);
                OnPropertyChanging(nameof(TypedValue));
            }
        }

        private void ContentChanged(object sender, ContentChangeEventArgs e)
        {
            if (IsValidChange(e))
            {
                ((ObservableNode)Parent)?.NotifyPropertyChanged(Name);

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

        private bool IsValidChange(ContentChangeEventArgs e)
        {
            switch (e.ChangeType)
            {
                case ContentChangeType.ValueChange:
                    return Equals(e.Index, Index);
                case ContentChangeType.CollectionAdd:
                case ContentChangeType.CollectionRemove:
                    return HasList || HasDictionary; // TODO: probably not sufficent
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
