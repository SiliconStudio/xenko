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
        public DefaultModelBuilder()
        {
            PrimitiveTypes = new List<Type>();
            AvailableCommands = new List<INodeCommand>();
        }

        /// <inheritdoc/>
        public ICollection<Type> PrimitiveTypes { get; private set; }
        
        /// <inheritdoc/>
        public ICollection<INodeCommand> AvailableCommands { get; private set; }

        /// <inheritdoc/>
        public IEnumerable<IContent> ReferenceContents { get { return referenceContents; } }

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
        public IModelNode Build(object obj, Type type, Guid guid)
        {
            Reset();
            rootGuid = guid;
            var typeDescriptor = TypeDescriptorFactory.Find(obj != null ? obj.GetType() : type);
            VisitObject(obj, typeDescriptor as ObjectDescriptor, true);

            return rootNode;
        }

        /// <inheritdoc/>
        public override void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers)
        {
            bool isRootNode = contextStack.Count == 0;
            if (isRootNode)
            {
                if (!NotifyNodeConstructing(descriptor))
                    return;

                // If we are in the case of a collection of collections, we might have a root node that is actually an enumerable reference
                // This would be the case for each collection within the base collection.
                IReference reference = CreateReferenceForNode(descriptor.Type, obj);
                reference = reference is ReferenceEnumerable ? reference : null;
                IContent content = descriptor.Type.IsStruct() ? new BoxedContent(obj, descriptor, IsPrimitiveType(descriptor.Type)) : new ObjectContent(obj, descriptor, IsPrimitiveType(descriptor.Type), reference);
                rootNode = new ModelNode(descriptor.Type.Name, content, rootGuid);
                if (reference != null && descriptor.Type.IsStruct())
                    throw new QuantumConsistencyException("A collection type", "A structure type", rootNode);

                if (reference != null)
                    referenceContents.Add(content);

                AvailableCommands.Where(x => x.CanAttach(rootNode.Content.Descriptor, null)).ForEach(rootNode.AddCommand);
                NotifyNodeConstructed(content);

                if (obj == null)
                {
                    rootNode.Seal();
                    return;
                }
                PushContextNode(rootNode);
            }

            if (!IsPrimitiveType(descriptor.Type))
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
            // Don't visit items unless they are primitive or enumerable (collections within collections)
            if (IsPrimitiveType(descriptor.ElementType, false) || IsEnumerable(descriptor.ElementType))
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
            if (IsPrimitiveType(descriptor.ValueType, false) || IsEnumerable(descriptor.ValueType))
            {
                base.VisitDictionary(dictionary, descriptor);
            }
        }

        /// <summary>
        /// Raises the <see cref="NodeConstructing"/> event.
        /// </summary>
        /// <param name="descriptor">The descriptor of the root object being constructed.</param>
        /// <returns><c>true</c> if the node should be constructed, <c>false</c> if it should be discarded.</returns>
        /// <remarks>This method is internal so it can be used by the <see cref="ModelConsistencyCheckVisitor"/>.</remarks>
        internal bool NotifyNodeConstructing(ObjectDescriptor descriptor)
        {
            var handler = NodeConstructing;
            if (handler != null)
            {
                var args = new NodeConstructingArgs(descriptor, null);
                handler(this, args);
                return !args.Discard;
            }
            return true;
        }

        /// <summary>
        /// Raises the <see cref="NodeConstructing"/> event.
        /// </summary>
        /// <param name="containerDescriptor">The descriptor of the container of the member being constructed, or of the object itself it is a root object.</param>
        /// <param name="member">The member descriptor of the member being constructed.</param>
        /// <returns><c>true</c> if the node should be constructed, <c>false</c> if it should be discarded.</returns>
        /// <remarks>This method is internal so it can be used by the <see cref="ModelConsistencyCheckVisitor"/>.</remarks>
        internal bool NotifyNodeConstructing(ObjectDescriptor containerDescriptor, IMemberDescriptor member)
        {
            var handler = NodeConstructing;
            if (handler != null)
            {
                var args = new NodeConstructingArgs(containerDescriptor, (MemberDescriptorBase)member);
                handler(this, args);
                return !args.Discard;
            }
            return true;
        }

        /// <summary>
        /// Raises the <see cref="NodeConstructed"/> event.
        /// </summary>
        /// <param name="content">The content of the node that has been constructed.</param>
        /// <remarks>This method is internal so it can be used by the <see cref="ModelConsistencyCheckVisitor"/>.</remarks>
        internal void NotifyNodeConstructed(IContent content)
        {
            var handler = NodeConstructed;
            if (handler != null)
            {
                var args = new NodeConstructedArgs(content);
                handler(this, args);
            }
        }

        /// <inheritdoc/>
        public override void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
        {
            if (!NotifyNodeConstructing(containerDescriptor, member))
                return;

            // If this member should contains a reference, create it now.
            IReference reference = CreateReferenceForNode(member.Type, value);
            ModelNode containerNode = GetContextNode();
            ITypeDescriptor typeDescriptor = TypeDescriptorFactory.Find(member.Type);
            IContent content = new MemberContent(containerNode.Content, member, typeDescriptor, IsPrimitiveType(member.Type), reference);
            var node = new ModelNode(member.Name, content, Guid.NewGuid());
            containerNode.AddChild(node);

            if (reference != null)
                referenceContents.Add(content);

            if (!(reference is ObjectReference))
            {
                // For enumerable references, we visit the member to allow VisitCollection or VisitDictionary to enrich correctly the node.
                PushContextNode(node);
                Visit(value);
                PopContextNode();
            }

            AvailableCommands.Where(x => x.CanAttach(node.Content.Descriptor, (MemberDescriptorBase)member)).ForEach(node.AddCommand);
            NotifyNodeConstructed(content);

            node.Seal();
        }

        private IReference CreateReferenceForNode(Type type, object value)
        {
            // Is it a reference?
            if ((!type.IsClass && !type.IsInterface) || IsPrimitiveType(type))
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

        private static bool IsEnumerable(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
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
            if (dictionaryDescriptor != null)
            {
                return dictionaryDescriptor.ValueType;
            }
            return collectionDescriptor != null ? collectionDescriptor.ElementType : null;
        }
    }
}
