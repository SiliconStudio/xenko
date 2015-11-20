// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Commands;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    /// <summary>
    /// The default <see cref="INodeBuilder"/> implementation that construct a model from a data object.
    /// </summary>
    internal class DefaultModelBuilder : DataVisitorBase, INodeBuilder
    {
        private readonly Stack<ModelNode> contextStack = new Stack<ModelNode>();
        private readonly HashSet<IContent> referenceContents = new HashSet<IContent>();
        private ModelNode rootNode;
        private Guid rootGuid;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultModelBuilder"/> class that can be used to construct a model for a data object.
        /// </summary>
        /// <param name="modelContainer"></param>
        public DefaultModelBuilder(ModelContainer modelContainer)
        {
            ModelContainer = modelContainer;
        }

        /// <inheritdoc/>
        public ModelContainer ModelContainer { get; }
        
        /// <inheritdoc/>
        public ICollection<Type> PrimitiveTypes { get; } = new List<Type>();

        /// <inheritdoc/>
        public ICollection<INodeCommand> AvailableCommands { get; } = new List<INodeCommand>();

        /// <inheritdoc/>
        public IContentFactory ContentFactory { get; set; } = new DefaultContentFactory();

        /// <inheritdoc/>
        public Func<string, IContent, Guid, IGraphNode> NodeFactory { get; set; } = (name, content, guid) => new ModelNode(name, content, guid);

        /// <inheritdoc/>
        public event EventHandler<NodeConstructingArgs> NodeConstructing;

        /// <inheritdoc/>
        public event EventHandler<NodeConstructedArgs> NodeConstructed;

        /// <summary>
        /// Reset the visitor in order to use it to generate another model.
        /// </summary>
        public override void Reset()
        {
            rootNode = null;
            rootGuid = Guid.Empty;
            contextStack.Clear();
            referenceContents.Clear();
            base.Reset();
        }

        /// <inheritdoc/>
        public IGraphNode Build(object obj, Guid guid)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            Reset();
            rootGuid = guid;
            var typeDescriptor = TypeDescriptorFactory.Find(obj.GetType());
            VisitObject(obj, typeDescriptor as ObjectDescriptor, true);

            return rootNode;
        }

        /// <inheritdoc/>
        public override void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers)
        {
            ITypeDescriptor currentDescriptor = descriptor;

            bool isRootNode = contextStack.Count == 0;
            if (isRootNode)
            {
                bool shouldProcessReference;
                if (!NotifyNodeConstructing(descriptor, out shouldProcessReference))
                    return;

                // If we are in the case of a collection of collections, we might have a root node that is actually an enumerable reference
                // This would be the case for each collection within the base collection.
                IContent content = descriptor.Type.IsStruct() ? ContentFactory.CreateBoxedContent(this, obj, descriptor, IsPrimitiveType(descriptor.Type))
                                                : ContentFactory.CreateObjectContent(this, obj, descriptor, IsPrimitiveType(descriptor.Type), shouldProcessReference);
                currentDescriptor = content.Descriptor;
                rootNode = (ModelNode)NodeFactory(currentDescriptor.Type.Name, content, rootGuid);
                if (content.IsReference && currentDescriptor.Type.IsStruct())
                    throw new QuantumConsistencyException("A collection type", "A structure type", rootNode);

                if (content.IsReference)
                    referenceContents.Add(content);

                AvailableCommands.Where(x => x.CanAttach(currentDescriptor, null)).ForEach(rootNode.AddCommand);
                NotifyNodeConstructed(content);

                if (obj == null)
                {
                    rootNode.Seal();
                    return;
                }
                PushContextNode(rootNode);
            }

            if (!IsPrimitiveType(currentDescriptor.Type))
            {
                base.VisitObject(obj, descriptor, true);
            }

            if (isRootNode)
            {
                PopContextNode();
                rootNode.Seal();
            }
        }

        /// <inheritdoc/>
        public override void VisitCollection(IEnumerable collection, CollectionDescriptor descriptor)
        {
            if (!descriptor.HasIndexerAccessors)
                throw new NotSupportedException("Collections that do not have indexer accessors are not supported in Quantum.");

            // Don't visit items unless they are primitive or enumerable (collections within collections)
            if (IsPrimitiveType(descriptor.ElementType, false) || IsCollection(descriptor.ElementType))
            {
                base.VisitCollection(collection, descriptor);
            }
        }

        /// <inheritdoc/>
        public override void VisitDictionary(object dictionary, DictionaryDescriptor descriptor)
        {
            if (!IsPrimitiveType(descriptor.KeyType))
                throw new InvalidOperationException("The type of dictionary key must be a primary type.");

            // Don't visit items unless they are primitive or enumerable (collections within collections)
            if (IsPrimitiveType(descriptor.ValueType, false) || IsCollection(descriptor.ValueType))
            {
                base.VisitDictionary(dictionary, descriptor);
            }
        }

        /// <summary>
        /// Raises the <see cref="NodeConstructing"/> event.
        /// </summary>
        /// <param name="descriptor">The descriptor of the root object being constructed.</param>
        /// <param name="shouldProcessReference">Indicates whether the reference that will be created in the node should be processed or not.</param>
        /// <returns><c>true</c> if the node should be constructed, <c>false</c> if it should be discarded.</returns>
        /// <remarks>This method is internal so it can be used by the <see cref="ModelConsistencyCheckVisitor"/>.</remarks>
        internal bool NotifyNodeConstructing(ObjectDescriptor descriptor, out bool shouldProcessReference)
        {
            var handler = NodeConstructing;
            if (handler != null)
            {
                var args = new NodeConstructingArgs(descriptor, null);
                handler(this, args);
                shouldProcessReference = !args.Discard && args.ShouldProcessReference;
                return !args.Discard;
            }
            shouldProcessReference = true;
            return true;
        }

        /// <summary>
        /// Raises the <see cref="NodeConstructing"/> event.
        /// </summary>
        /// <param name="containerDescriptor">The descriptor of the container of the member being constructed, or of the object itself it is a root object.</param>
        /// <param name="member">The member descriptor of the member being constructed.</param>
        /// <param name="shouldProcessReference">Indicates whether the reference that will be created in the node should be processed or not.</param>
        /// <returns><c>true</c> if the node should be constructed, <c>false</c> if it should be discarded.</returns>
        /// <remarks>This method is internal so it can be used by the <see cref="ModelConsistencyCheckVisitor"/>.</remarks>
        internal bool NotifyNodeConstructing(ObjectDescriptor containerDescriptor, IMemberDescriptor member, out bool shouldProcessReference)
        {
            var handler = NodeConstructing;
            if (handler != null)
            {
                var args = new NodeConstructingArgs(containerDescriptor, (MemberDescriptorBase)member);
                handler(this, args);
                shouldProcessReference = !args.Discard && args.ShouldProcessReference;
                return !args.Discard;
            }
            shouldProcessReference = true;
            return true;
        }

        /// <summary>
        /// Raises the <see cref="NodeConstructed"/> event.
        /// </summary>
        /// <param name="content">The content of the node that has been constructed.</param>
        /// <remarks>This method is internal so it can be used by the <see cref="ModelConsistencyCheckVisitor"/>.</remarks>
        internal void NotifyNodeConstructed(IContent content)
        {
            NodeConstructed?.Invoke(this, new NodeConstructedArgs(content));
        }

        /// <inheritdoc/>
        public override void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
        {
            bool shouldProcessReference;
            if (!NotifyNodeConstructing(containerDescriptor, member, out shouldProcessReference))
                return;

            // If this member should contains a reference, create it now.
            ModelNode containerNode = GetContextNode();
            IContent content = ContentFactory.CreateMemberContent(this, containerNode.Content, member, IsPrimitiveType(member.Type), value, shouldProcessReference);
            var node = (ModelNode)NodeFactory(member.Name, content, Guid.NewGuid());
            containerNode.AddChild(node);

            if (content.IsReference)
                referenceContents.Add(content);

            PushContextNode(node);
            if (!(content.Reference is ObjectReference))
            {
                // For enumerable references, we visit the member to allow VisitCollection or VisitDictionary to enrich correctly the node.
                Visit(content.Value);
            }
            PopContextNode();

            AvailableCommands.Where(x => x.CanAttach(node.Content.Descriptor, (MemberDescriptorBase)member)).ForEach(node.AddCommand);
            NotifyNodeConstructed(content);

            node.Seal();
        }

        public IReference CreateReferenceForNode(Type type, object value)
        {
            // We don't create references for primitive types
            if (IsPrimitiveType(type))
                return null;

            ITypeDescriptor descriptor = value != null ? TypeDescriptorFactory.Find(value.GetType()) : null;
            var valueType = GetElementValueType(descriptor);

            // This is either an object reference or a enumerable reference of non-primitive type (excluding custom primitive type)
            if (valueType == null || !IsPrimitiveType(valueType, false))
                return Reference.CreateReference(value, type, Reference.NotInCollection);

            return null;
        }
        
        private void PushContextNode(ModelNode node)
        {
            contextStack.Push(node);
        }

        private void PopContextNode()
        {
            contextStack.Pop();
        }

        private ModelNode GetContextNode()
        {
            return contextStack.Peek();
        }

        private static bool IsCollection(Type type)
        {
            return typeof(ICollection).IsAssignableFrom(type);
        }

        private bool IsPrimitiveType(Type type, bool includeAdditionalPrimitiveTypes = true)
        {
            if (type == null)
                return false;

            if (type.IsNullable())
                type = Nullable.GetUnderlyingType(type);

            return type.IsPrimitive || type == typeof(string) || type.IsEnum || (includeAdditionalPrimitiveTypes && PrimitiveTypes.Any(x => x.IsAssignableFrom(type)));
        }

        private static Type GetElementValueType(ITypeDescriptor descriptor)
        {
            var dictionaryDescriptor = descriptor as DictionaryDescriptor;
            var collectionDescriptor = descriptor as CollectionDescriptor;
            return dictionaryDescriptor != null ? dictionaryDescriptor.ValueType : collectionDescriptor?.ElementType;
        }
    }
}
