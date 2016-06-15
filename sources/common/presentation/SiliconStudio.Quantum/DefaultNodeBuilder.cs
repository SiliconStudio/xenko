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
    internal class DefaultNodeBuilder : DataVisitorBase, INodeBuilder
    {
        private readonly Stack<GraphNode> contextStack = new Stack<GraphNode>();
        private readonly HashSet<IContent> referenceContents = new HashSet<IContent>();
        private static readonly Type[] InternalPrimitiveTypes = { typeof(decimal), typeof(string), typeof(Guid) };
        private GraphNode rootNode;
        private Guid rootGuid;
        private NodeFactoryDelegate currentNodeFactory;

        public DefaultNodeBuilder(NodeContainer nodeContainer)
        {
            NodeContainer = nodeContainer;
            primitiveTypes.AddRange(InternalPrimitiveTypes);
        }

        /// <inheritdoc/>
        public NodeContainer NodeContainer { get; }
        
        /// <inheritdoc/>
        private readonly List<Type> primitiveTypes = new List<Type>();

        /// <inheritdoc/>
        public ICollection<INodeCommand> AvailableCommands { get; } = new List<INodeCommand>();

        /// <inheritdoc/>
        public IContentFactory ContentFactory { get; set; } = new DefaultContentFactory();

        public bool DiscardUnbrowsable { get; set; } = true;

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

        public void RegisterPrimitiveType(Type type)
        {
            if (type.IsPrimitive || type.IsEnum || primitiveTypes.Contains(type))
                return;

            primitiveTypes.Add(type);
        }

        public void UnregisterPrimitiveType(Type type)
        {
            if (type.IsPrimitive || type.IsEnum || InternalPrimitiveTypes.Contains(type))
                throw new InvalidOperationException("The given type cannot be unregistered from the list of primitive types");

            primitiveTypes.Remove(type);
        }

        public bool IsPrimitiveType(Type type)
        {
            if (type == null)
                return false;

            if (type.IsNullable())
                type = Nullable.GetUnderlyingType(type);

            return type.IsPrimitive || type.IsEnum || primitiveTypes.Any(x => x.IsAssignableFrom(type));
        }

        /// <inheritdoc/>
        public IGraphNode Build(object obj, Guid guid, NodeFactoryDelegate nodeFactory)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (nodeFactory == null) throw new ArgumentNullException(nameof(nodeFactory));
            Reset();
            rootGuid = guid;
            var typeDescriptor = TypeDescriptorFactory.Find(obj.GetType());
            currentNodeFactory = nodeFactory;
            VisitObject(obj, typeDescriptor as ObjectDescriptor, true);
            currentNodeFactory = null;
            return rootNode;
        }

        /// <inheritdoc/>
        public override void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers)
        {
            ITypeDescriptor currentDescriptor = descriptor;

            bool isRootNode = contextStack.Count == 0;
            if (isRootNode)
            {
                // If we are in the case of a collection of collections, we might have a root node that is actually an enumerable reference
                // This would be the case for each collection within the base collection.
                var content = descriptor.Type.IsStruct() ? ContentFactory.CreateBoxedContent(this, obj, descriptor, IsPrimitiveType(descriptor.Type))
                                : ContentFactory.CreateObjectContent(this, obj, descriptor, IsPrimitiveType(descriptor.Type));
                currentDescriptor = content.Descriptor;
                rootNode = (GraphNode)currentNodeFactory(currentDescriptor.Type.Name, content, rootGuid);
                if (content.IsReference && currentDescriptor.Type.IsStruct())
                    throw new QuantumConsistencyException("A collection type", "A structure type", rootNode);

                if (content.IsReference)
                    referenceContents.Add(content);

                AvailableCommands.Where(x => x.CanAttach(currentDescriptor, null)).ForEach(rootNode.AddCommand);

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
            if (IsCollection(descriptor.ElementType))
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
            if (IsCollection(descriptor.ValueType))
            {
                base.VisitDictionary(dictionary, descriptor);
            }
        }

        /// <inheritdoc/>
        public override void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
        {
            // If this member should contains a reference, create it now.
            GraphNode containerNode = GetContextNode();
            IContent content = ContentFactory.CreateMemberContent(this, (ContentBase)containerNode.Content, member, IsPrimitiveType(member.Type), value);
            var node = (GraphNode)currentNodeFactory(member.Name, content, Guid.NewGuid());
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

            node.Seal();
        }

        public IReference CreateReferenceForNode(Type type, object value)
        {
            // We don't create references for primitive types and structs
            if (IsPrimitiveType(type) || type.IsStruct())
                return null;

            // At this point it is either a reference type or a collection
            ITypeDescriptor descriptor = value != null ? TypeDescriptorFactory.Find(value.GetType()) : null;
            var valueType = GetElementValueType(descriptor);

            // We create reference only for structs (in case of collection of structs) and classes (in a collection or not) 
            if (valueType == null || !IsPrimitiveType(valueType))
                return Reference.CreateReference(value, type, Index.Empty);

            return null;
        }
        
        private void PushContextNode(GraphNode node)
        {
            contextStack.Push(node);
        }

        private void PopContextNode()
        {
            contextStack.Pop();
        }

        private GraphNode GetContextNode()
        {
            return contextStack.Peek();
        }

        private static bool IsCollection(Type type)
        {
            return typeof(ICollection).IsAssignableFrom(type);
        }

        private static Type GetElementValueType(ITypeDescriptor descriptor)
        {
            var dictionaryDescriptor = descriptor as DictionaryDescriptor;
            var collectionDescriptor = descriptor as CollectionDescriptor;
            return dictionaryDescriptor != null ? dictionaryDescriptor.ValueType : collectionDescriptor?.ElementType;
        }
    }
}
