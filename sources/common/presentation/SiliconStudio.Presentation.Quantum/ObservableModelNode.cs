// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Presentation.Quantum
{
    public abstract class ObservableModelNode : SingleObservableNode
    {
        private readonly bool isPrimitive;
        private readonly IModelNode sourceNode;
        protected readonly ModelNodePath SourceNodePath;
        private IModelNode targetNode;
        private IDictionary<string, object> associatedData;
        private bool isInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableModelNode"/> class.
        /// </summary>
        /// <param name="ownerViewModel">The <see cref="ObservableViewModel"/> that owns the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="baseName">The base name of this node. Can be null if <see cref="index"/> is not. If so a name will be automatically generated from the index.</param>
        /// <param name="isPrimitive">Indicate whether this node should be considered as a primitive node.</param>
        /// <param name="parentNode">The parent node of the new <see cref="ObservableModelNode"/>, or <c>null</c> if the node being created is the root node of the view model.</param>
        /// <param name="modelNode">The model node bound to the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="modelNodePath">The <see cref="ModelNodePath"/> corresponding to the given <see cref="modelNode"/>.</param>
        /// <param name="index">The index of this content in the model node, when this node represent an item of a collection. <c>null</c> must be passed otherwise</param>
        protected ObservableModelNode(ObservableViewModel ownerViewModel, string baseName, bool isPrimitive, SingleObservableNode parentNode, IModelNode modelNode, ModelNodePath modelNodePath, object index = null)
            : base(ownerViewModel, baseName, parentNode, index)
        {
            if (modelNode == null) throw new ArgumentNullException("modelNode");
            if (baseName == null && index == null)
                throw new ArgumentException("baseName and index can't be both null.");

            this.isPrimitive = isPrimitive;
            sourceNode = modelNode;
            // By default we will always combine items of list of primitive items.
            CombineMode = index != null && isPrimitive ? CombineMode.AlwaysCombine : CombineMode.CombineOnlyForAll;
            targetNode = GetTargetNode(modelNode, index);
            SourceNodePath = modelNodePath;
        }

        /// <summary>
        /// Create an <see cref="ObservableModelNode{T}"/> that matches the given content type.
        /// </summary>
        /// <param name="ownerViewModel">The <see cref="ObservableViewModel"/> that owns the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="baseName">The base name of this node. Can be null if <see cref="index"/> is not. If so a name will be automatically generated from the index.</param>
        /// <param name="isPrimitive">Indicate whether this node should be considered as a primitive node.</param>
        /// <param name="parentNode">The parent node of the new <see cref="ObservableModelNode"/>, or <c>null</c> if the node being created is the root node of the view model.</param>
        /// <param name="modelNode">The model node bound to the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="modelNodePath">The <see cref="ModelNodePath"/> corresponding to the given node.</param>
        /// <param name="contentType">The type of content contained by the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="index">The index of this content in the model node, when this node represent an item of a collection. <c>null</c> must be passed otherwise</param>
        /// <returns>A new instance of <see cref="ObservableModelNode{T}"/> instanced with the given content type as generic argument.</returns>
        internal static ObservableModelNode Create(ObservableViewModel ownerViewModel, string baseName, bool isPrimitive, SingleObservableNode parentNode, IModelNode modelNode, ModelNodePath modelNodePath, Type contentType, object index)
        {
            var node = (ObservableModelNode)Activator.CreateInstance(typeof(ObservableModelNode<>).MakeGenericType(contentType), ownerViewModel, baseName, isPrimitive, parentNode, modelNode, modelNodePath, index);
            return node;
        }

        internal void Initialize()
        {
            Initialize(false);
        }

        private void Initialize(bool isUpdating)
        {
            var targetNodePath = ModelNodePath.GetChildPath(SourceNodePath, sourceNode, targetNode);
            if (targetNodePath == null || !targetNodePath.IsValid)
                throw new InvalidOperationException("Unable to retrieve the path of the given model node.");
            
            foreach (var command in targetNode.Commands)
            {
                var commandWrapper = new ModelNodeCommandWrapper(ServiceProvider, command, Path, Owner.Identifier, targetNodePath, Owner.ModelContainer, Owner.GetDirtiableViewModels(this));
                AddCommand(commandWrapper);
            }

            if (!isPrimitive)
                GenerateChildren(targetNode, targetNodePath, isUpdating);

            isInitialized = true;

            if (Owner.ObservableViewModelService != null)
            {
                var data = Owner.ObservableViewModelService.RequestAssociatedData(this, isUpdating);
                SetValue(ref associatedData, data, "AssociatedData");
            }
            
            CheckDynamicMemberConsistency();
        }

        /// <inheritdoc/>
        public override int? Order { get { return sourceNode.Content is MemberContent && (!(sourceNode.Content.Reference is ReferenceEnumerable) && Index == null) ? ((MemberContent)sourceNode.Content).Member.Order : null; } }

        /// <inheritdoc/>
        public sealed override bool IsPrimitive { get { AssertInit(); return isPrimitive; } }
        
        // To distinguish between lists and items of a list (which have the same TargetNode if the items are primitive types), we check whether the TargetNode is
        // the same of the one of its parent. If so, we're likely in an item of a list of primitive objects. 
        /// <inheritdoc/>
        public sealed override bool HasList { get { AssertInit(); return (targetNode.Content.Descriptor is CollectionDescriptor && (Parent == null || (ModelNodeParent != null && ModelNodeParent.targetNode.Content.Value != targetNode.Content.Value))) || targetNode.Content.Reference is ReferenceEnumerable; } }

        // To distinguish between dictionaries and items of a dictionary (which have the same TargetNode if the value type is a primitive type), we check whether the TargetNode is
        // the same of the one of its parent. If so, we're likely in an item of a dictionary of primitive objects. 
        /// <inheritdoc/>
        public sealed override bool HasDictionary { get { AssertInit(); return (targetNode.Content.Descriptor is DictionaryDescriptor && (Parent == null || (ModelNodeParent != null && ModelNodeParent.targetNode.Content.Value != targetNode.Content.Value))) || (targetNode.Content.Reference is ReferenceEnumerable && ((ReferenceEnumerable)targetNode.Content.Reference).IsDictionary); } }

        /// <inheritdoc/>
        public sealed override IDictionary<string, object> AssociatedData { get { return associatedData; } }

        internal Guid ModelGuid { get { return targetNode.Guid; } }

        private ObservableModelNode ModelNodeParent { get { AssertInit(); for (var p = Parent; p != null; p = p.Parent) { var mp = p as ObservableModelNode; if (mp != null) return mp; } return null; } }
                
        /// <summary>
        /// Indicates whether this <see cref="ObservableModelNode"/> instance corresponds to the given <see cref="IModelNode"/>.
        /// </summary>
        /// <param name="node">The node to match.</param>
        /// <returns><c>true</c> if the node matches, <c>false</c> otherwise.</returns>
        public bool MatchNode(IModelNode node)
        {
            return sourceNode == node;
        }

        public IMemberDescriptor GetMemberDescriptor()
        {
            var memberContent = sourceNode.Content as MemberContent;
            return memberContent != null ? memberContent.Member : null;
        }

        internal void CheckConsistency()
        {
#if DEBUG
            if (sourceNode != targetNode)
            {
                var objectReference = sourceNode.Content.Reference as ObjectReference;
                var referenceEnumerable = sourceNode.Content.Reference as ReferenceEnumerable;
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
                else if (!targetNode.Children.Contains(child.sourceNode))
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
            var dictionary = sourceNode.Content.Descriptor as DictionaryDescriptor;
            var list = sourceNode.Content.Descriptor as CollectionDescriptor;

            if (Index != null && dictionary != null)
                return dictionary.GetValue(sourceNode.Content.Value, Index);

            if (Index != null && list != null)
                return list.GetValue(sourceNode.Content.Value, Index);

            return sourceNode.Content.Value;
        }

        /// <summary>
        /// Sets the value of the model content associated to this <see cref="ObservableModelNode"/>. The value is actually modified only if the new value is different from the previous value.
        /// </summary>
        /// <returns><c>True</c> if the value has been modified, <c>false</c> otherwise.</returns>
        protected bool SetModelContentValue(object newValue)
        {
            var dictionary = sourceNode.Content.Descriptor as DictionaryDescriptor;
            var list = sourceNode.Content.Descriptor as CollectionDescriptor;
            bool result = false;
            if (Index != null && dictionary != null)
            {
                if (!Equals(dictionary.GetValue(sourceNode.Content.Value, Index), newValue))
                {
                    result = true;
                    dictionary.SetValue(sourceNode.Content.Value, Index, newValue);
                }
            }
            else if (Index != null && list != null)
            {
                if (!Equals(list.GetValue(sourceNode.Content.Value, Index), newValue))
                {
                    result = true;
                    list.SetValue(sourceNode.Content.Value, Index, newValue);
                }
            }
            else
            {
                if (!Equals(sourceNode.Content.Value, newValue))
                {
                    result = true;
                    sourceNode.Content.Value = newValue;
                }
            }

            if (!IsPrimitive)
            {
                Owner.ModelContainer.UpdateReferences(sourceNode);
                Refresh();
            }
            return result;
        }

        private void GenerateChildren(IModelNode modelNode, ModelNodePath modelNodePath, bool isUpdating)
        {
            if (modelNode.Content.IsReference)
            {
                var referenceEnumerable = modelNode.Content.Reference as ReferenceEnumerable;
                if (referenceEnumerable != null)
                {
                    foreach (var reference in referenceEnumerable)
                    {
                        // The type might be a boxed primitive type, such as float, if the collection has object as generic argument.
                        // In this case, we must set the actual type to have type converter working, since they usually can't convert
                        // a boxed float to double for example. Otherwise, we don't want to have a node type that is value-dependent.
                        var type = reference.TargetNode != null && reference.TargetNode.Content.IsPrimitive ? reference.TargetNode.Content.Type : reference.Type;
                        var observableNode = Create(Owner, null, false, this, modelNode, modelNodePath, type, reference.Index);
                        observableNode.Initialize(isUpdating);
                        AddChild(observableNode);
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
                        var observableChild = Create(Owner, null, true, this, modelNode, modelNodePath, dictionary.ValueType, key);
                        observableChild.Initialize(isUpdating);
                        AddChild(observableChild);
                    }
                }
                else if (list != null && modelNode.Content.Value != null)
                {
                    // List of primitive objects
                    for (int i = 0; i < list.GetCollectionCount(modelNode.Content.Value); ++i)
                    {
                        var observableChild = Create(Owner, null, true, this, modelNode, modelNodePath, list.ElementType, i);
                        observableChild.Initialize(isUpdating);
                        AddChild(observableChild);
                    }
                }
                else
                {
                    // Single non-reference primitive object
                    foreach (var child in modelNode.Children)
                    {
                        var childPath = ModelNodePath.GetChildPath(modelNodePath, modelNode, child);
                        var observableChild = Create(Owner, child.Name, child.Content.IsPrimitive, this, child, childPath, child.Content.Type, null);
                        observableChild.Initialize(isUpdating);
                        AddChild(observableChild);
                    }
                }
            }
        }

        internal void Refresh()
        {
            if (Parent == null) throw new InvalidOperationException("The node to refresh can't be a root node.");
            
            OnPropertyChanging("IsPrimitive", "HasList", "HasDictionary");

            targetNode = GetTargetNode(sourceNode, Index);

            // Clean the current node so it can be re-initialized (associatedData are overwritten in Initialize)
            ClearCommands();
            foreach (var child in Children.ToList())
                RemoveChild(child);

            Initialize(true);

            OnPropertyChanged("IsPrimitive", "HasList", "HasDictionary");
        }

        private static IModelNode GetTargetNode(IModelNode sourceNode, object index)
        {
            var objectReference = sourceNode.Content.Reference as ObjectReference;
            var referenceEnumerable = sourceNode.Content.Reference as ReferenceEnumerable;
            if (objectReference != null)
            {
                return objectReference.TargetNode;
            }
            if (referenceEnumerable != null && index != null)
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
        /// <param name="parentNode">The parent node of the new <see cref="ObservableModelNode"/>, or <c>null</c> if the node being created is the root node of the view model.</param>
        /// <param name="modelNode">The model node bound to the new <see cref="ObservableModelNode"/>.</param>
        /// <param name="modelNodePath">The <see cref="ModelNodePath"/> corresponding to the given <see cref="modelNode"/>.</param>
        /// <param name="index">The index of this content in the model node, when this node represent an item of a collection. <c>null</c> must be passed otherwise</param>
        public ObservableModelNode(ObservableViewModel ownerViewModel, string baseName, bool isPrimitive, SingleObservableNode parentNode, IModelNode modelNode, ModelNodePath modelNodePath, object index)
            : base(ownerViewModel, baseName, isPrimitive, parentNode, modelNode, modelNodePath, index)
        {
            DependentProperties.Add("TypedValue", new[] { "Value" });
        }

        /// <summary>
        /// Gets or sets the value of this node through a correctly typed property, which is more adapted to binding.
        /// </summary>
        public T TypedValue
        {
            get
            {
                AssertInit();
                return (T)GetModelContentValue();
            }
            set
            {
                AssertInit();
                var previousValue = (T)GetModelContentValue();
                bool hasChanged = !Equals(previousValue, value);
                if (hasChanged)
                {
                    OnPropertyChanging("TypedValue");
                }
                
                // We set the value even if it has not changed in case it's a reference value and a refresh might be required (new node in a list, etc.)
                SetModelContentValue(value);

                if (hasChanged)
                {
                    OnPropertyChanged("TypedValue");
                    string displayName = Owner.FormatSingleUpdateMessage(this, value);
                    var dirtiables = Owner.GetDirtiableViewModels(this);
                    Owner.RegisterAction(displayName, SourceNodePath, Path, Index, dirtiables, value, previousValue);
                }
            }
        }

        /// <inheritdoc/>
        public override Type Type { get { return typeof(T); } }

        /// <inheritdoc/>
        public override sealed object Value { get { return TypedValue; } set { TypedValue = (T)value; } }
    }
}