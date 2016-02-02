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
        /// <param name="modelNode">The model node bound to the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="graphNodePath">The <see cref="GraphNodePath"/> corresponding to the given <see cref="modelNode"/>.</param>
        /// <param name="index">The index of this content in the model node, when this node represent an item of a collection. <c>null</c> must be passed otherwise</param>
        protected ObservableModelNode(ObservableViewModel ownerViewModel, string baseName, bool isPrimitive, IGraphNode modelNode, GraphNodePath graphNodePath, object index = null)
            : base(ownerViewModel, baseName, index)
        {
            if (modelNode == null) throw new ArgumentNullException(nameof(modelNode));
            if (baseName == null && index == null)
                throw new ArgumentException("baseName and index can't be both null.");

            this.isPrimitive = isPrimitive;
            SourceNode = modelNode;
            // By default we will always combine items of list of primitive items.
            CombineMode = index != null && isPrimitive ? CombineMode.AlwaysCombine : CombineMode.CombineOnlyForAll;
            SourceNodePath = graphNodePath;

            // Override display name if available
            var memberDescriptor = GetMemberDescriptor() as MemberDescriptorBase;
            if (memberDescriptor != null)
            {
                if (index == null)
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
        /// <param name="modelNode">The model node bound to the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="graphNodePath">The <see cref="GraphNodePath"/> corresponding to the given node.</param>
        /// <param name="contentType">The type of content contained by the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="index">The index of this content in the model node, when this node represent an item of a collection. <c>null</c> must be passed otherwise</param>
        /// <returns>A new instance of <see cref="ObservableModelNode{T}"/> instanced with the given content type as generic argument.</returns>
        internal static ObservableModelNode Create(ObservableViewModel ownerViewModel, string baseName, bool isPrimitive, IGraphNode modelNode, GraphNodePath graphNodePath, Type contentType, object index)
        {
            var node = (ObservableModelNode)Activator.CreateInstance(typeof(ObservableModelNode<>).MakeGenericType(contentType), ownerViewModel, baseName, isPrimitive, modelNode, graphNodePath, index);
            return node;
        }

        internal protected virtual void Initialize()
        {
            var targetNode = GetTargetNode(SourceNode, Index);
            var targetNodePath = SourceNodePath.GetChildPath(SourceNode, targetNode);
            if (targetNodePath == null || !targetNodePath.IsValid)
                throw new InvalidOperationException("Unable to retrieve the path of the given model node.");

            var commandPath = targetNodePath;
            if ((!SourceNode.Content.ShouldProcessReference || targetNode == SourceNode || targetNode == null) && Index != null)
            {
                // When the references are not processed or when the value is null, there is no actual target node.
                // However, the commands need the index to be able to properly set the modified value
                commandPath = targetNodePath.PushElement(Index, GraphNodePath.ElementType.Index);
            }

            if (targetNode != SourceNode && targetNode != null)
            {
                foreach (var command in targetNode.Commands)
                {
                    var commandWrapper = new ModelNodeCommandWrapper(ServiceProvider, command, commandPath, Owner.Dirtiables);
                    AddCommand(commandWrapper);
                }
            }

            var targetCommandNames = Commands.Select(x => x.Name).ToList();
            foreach (var command in SourceNode.Commands)
            {
                // Add source commands that are not already provided by the target node
                if (!targetCommandNames.Contains(command.Name))
                {
                    var commandWrapper = new ModelNodeCommandWrapper(ServiceProvider, command, commandPath, Owner.Dirtiables);
                    AddCommand(commandWrapper);
                }
            }

            if (!isPrimitive && targetNode != null)
            {
                GenerateChildren(targetNode, targetNodePath);
            }

            isInitialized = true;

            Owner.ObservableViewModelService?.NotifyNodeInitialized(this);

            FinalizeChildrenInitialization();

            CheckDynamicMemberConsistency();
        }

        /// <inheritdoc/>
        public override int? Order => CustomOrder ?? (SourceNode.Content is MemberContent && Index == null ? ((MemberContent)SourceNode.Content).Member.Order : null);

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

        // TODO: would be better to put the override info in an associated data or in a specialized derived class.
        public OverrideType Override => (SourceNode.Content as OverridableMemberContent)?.Override ?? OverrideType.Base;

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
                if (referenceEnumerable != null && Index != null)
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
                else if (!targetNode.Children.Contains(child.SourceNode))
                {
                    if (child.Index == null || !child.IsPrimitive)
                    {
                        throw new ObservableViewModelConsistencyException(child, "The source node of this node is not a child of the target node of its parent.");
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
            var dictionary = SourceNode.Content.Descriptor as DictionaryDescriptor;
            var list = SourceNode.Content.Descriptor as CollectionDescriptor;

            if (Index != null && dictionary != null)
                return dictionary.GetValue(SourceNode.Content.Value, Index);

            if (Index != null && list != null)
                return list.GetValue(SourceNode.Content.Value, Index);

            return SourceNode.Content.Value;
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

        private void GenerateChildren(IGraphNode modelNode, GraphNodePath graphNodePath)
        {
            if (modelNode.Content.IsReference && modelNode.Content.ShouldProcessReference)
            {
                var referenceEnumerable = modelNode.Content.Reference as ReferenceEnumerable;
                if (referenceEnumerable != null)
                {
                    // If the reference should not be processed, we still need to create an observable node for each entry of the enumerable.
                    // These observable nodes will have the same source node that their parent so we use this information to prevent
                    // the infinite recursion that could occur due to the fact that these child nodes will have the same model nodes (like primitive types)
                    // while holding an enumerable reference.
                    //if (modelNode.Content.ShouldProcessReference || ModelNodeParent.sourceNode != modelNode)
                    {
                        // Note: we are making a copy of the reference list because it can be updated from the Initialize method of the
                        // observable node in the case of scene objects. Doing this is a hack, but parts of this framework will be redesigned later to improve this
                        foreach (var reference in referenceEnumerable.ToList())
                        {
                            // The type might be a boxed primitive type, such as float, if the collection has object as generic argument.
                            // In this case, we must set the actual type to have type converter working, since they usually can't convert
                            // a boxed float to double for example. Otherwise, we don't want to have a node type that is value-dependent.
                            var type = reference.TargetNode != null && reference.TargetNode.Content.IsPrimitive ? reference.TargetNode.Content.Type : reference.Type;
                            var observableNode = Owner.ObservableViewModelService.ObservableNodeFactory(Owner, null, false, modelNode, graphNodePath, type, reference.Index);
                            AddChild(observableNode);
                            observableNode.Initialize();
                        }
                    }
                }
            }
            else
            {
                var dictionary = modelNode.Content.Descriptor as DictionaryDescriptor;
                var list = modelNode.Content.Descriptor as CollectionDescriptor;
                if (dictionary != null && modelNode.Content.Value != null)
                {
                    // Dictionary of primitive objects
                    foreach (var key in dictionary.GetKeys(modelNode.Content.Value))
                    {
                        var observableChild = Owner.ObservableViewModelService.ObservableNodeFactory(Owner, null, true, modelNode, graphNodePath, dictionary.ValueType, key);
                        AddChild(observableChild);
                        observableChild.Initialize();
                    }
                }
                else if (list != null && modelNode.Content.Value != null)
                {
                    // List of primitive objects
                    for (int i = 0; i < list.GetCollectionCount(modelNode.Content.Value); ++i)
                    {
                        var observableChild = Owner.ObservableViewModelService.ObservableNodeFactory(Owner, null, true, modelNode, graphNodePath, list.ElementType, i);
                        AddChild(observableChild);
                        observableChild.Initialize();
                    }
                }
                else
                {
                    // Single non-reference primitive object
                    foreach (var child in modelNode.Children)
                    {
                        var childPath = graphNodePath.GetChildPath(modelNode, child);
                        var observableChild = Owner.ObservableViewModelService.ObservableNodeFactory(Owner, child.Name, child.Content.IsPrimitive, child, childPath, child.Content.Type, null);
                        AddChild(observableChild);
                        observableChild.Initialize();
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
            Children.SelectDeep(x => x.Children).ForEach(x => x.Dispose());
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
        /// <param name="index">The index of the target node to retrieve, if the source node contains a sequence of references. <c>null</c> otherwise.</param>
        /// <returns>The corresponding target node if available, or the source node itself if it does not contain any reference or if its content should not process references.</returns>
        /// <remarks>This method can return null if the target node is null.</remarks>
        /// <seealso cref="IContent.ShouldProcessReference"/>
        protected static IGraphNode GetTargetNode(IGraphNode sourceNode, object index)
        {
            if (sourceNode == null) throw new ArgumentNullException(nameof(sourceNode));
            var objectReference = sourceNode.Content.Reference as ObjectReference;
            var referenceEnumerable = sourceNode.Content.Reference as ReferenceEnumerable;
            if (objectReference != null && sourceNode.Content.ShouldProcessReference)
            {
                return objectReference.TargetNode;
            }
            if (referenceEnumerable != null && sourceNode.Content.ShouldProcessReference && index != null)
            {
                return referenceEnumerable[index].TargetNode;
            }
            return sourceNode;
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
        /// <param name="index">The index of this content in the model node, when this node represent an item of a collection. <c>null</c> must be passed otherwise</param>
        public ObservableModelNode(ObservableViewModel ownerViewModel, string baseName, bool isPrimitive, IGraphNode modelNode, GraphNodePath graphNodePath, object index)
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
        public override sealed object Value { get { return TypedValue; } set { TypedValue = (T)value; } }

        public override void Dispose()
        {
            SourceNode.Content.Changing -= ContentChanging;
            SourceNode.Content.Changed -= ContentChanged;
            base.Dispose();
        }

        private void ContentChanging(object sender, ContentChangeEventArgs e)
        {
            if (IsValidChange(e))
            {
                ((ObservableNode)Parent)?.NotifyPropertyChanging(Name);
                OnPropertyChanging(nameof(TypedValue));
                OnPropertyChanging(nameof(Override));
            }
        }

        private void ContentChanged(object sender, ContentChangeEventArgs e)
        {
            if (IsValidChange(e))
            {
                ((ObservableNode)Parent)?.NotifyPropertyChanged(Name);

                // This node can have been disposed by its parent already (if its parent is being refreshed and share the same source node)
                // In this case, let's trigger the notifications gracefully before being discarded, but skip refresh
                if (!IsPrimitive && !IsDisposed)
                {
                    Refresh();
                }

                OnPropertyChanged(nameof(Override));
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