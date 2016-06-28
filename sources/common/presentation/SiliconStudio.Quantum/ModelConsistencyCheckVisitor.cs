// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    [Obsolete("This class will be removed soon", true)]
    public class ModelConsistencyCheckVisitor : DataVisitorBase
    {
        private class ReferenceInfo
        {
            public readonly Type ReferenceType;
            public readonly int EnumerableCount;
            public ReferenceInfo(Type referenceType, int enumerableCount)
            {
                ReferenceType = referenceType;
                EnumerableCount = enumerableCount;
            }
        }

        private readonly DefaultNodeBuilder nodeBuilder;
        private readonly Stack<GraphNode> contextStack = new Stack<GraphNode>();
        private readonly Queue<ObjectReference> references = new Queue<ObjectReference>();
        private readonly List<GraphNode> checkedNodes = new List<GraphNode>();
        private GraphNode rootNode;

        public ModelConsistencyCheckVisitor(INodeBuilder nodeBuilder)
        {
            if (nodeBuilder == null) throw new ArgumentNullException(nameof(nodeBuilder));
            this.nodeBuilder = nodeBuilder as DefaultNodeBuilder;
            if (this.nodeBuilder == null) throw new ArgumentException(@"This argument should be a DefaultNodeBuilder", nameof(nodeBuilder));
        }

        public override void Reset()
        {
            contextStack.Clear();
            rootNode = null;
            references.Clear();
            checkedNodes.Clear();
            base.Reset();
        }

        public void Check(GraphNode node, object obj, Type type, bool checkReferences)
        {
            Reset();

            if (node.Content.Value != obj)
                throw new QuantumConsistencyException("The node content value [{0}]", obj.ToStringSafe(), "The node content value [{0}]", node.Content.Value.ToStringSafe(), node);
            if (node.Content.Type != type)
                throw new QuantumConsistencyException("The node content type [{0}]", type.Name, "The node content value [{0}]", node.Content.Type.Name, node);

            rootNode = node;

            while (rootNode != null)
            {
                if (rootNode.Parent != null)
                    throw new QuantumConsistencyException("A root node", "A node with a parent", rootNode);

                if (rootNode.Content.Value != null)
                {
                    var typeDescriptor = TypeDescriptorFactory.Find(rootNode.Content.Type);
                    PushContextNode(rootNode);
                    VisitObject(rootNode.Content.Value, typeDescriptor as ObjectDescriptor, true);
                    PopContextNode();
                }
                checkedNodes.Add(rootNode);
                rootNode = null;

                if (checkReferences)
                {
                    while (references.Count > 0)
                    {
                        var reference = references.Dequeue();
                        if (!checkedNodes.Contains(reference.TargetNode))
                        {
                            rootNode = (GraphNode)reference.TargetNode;
                            break;
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers)
        {
            var node = GetContextNode();

            var referenceInfo = GetReferenceInfo(descriptor.Type, obj);
            if (node.Content.Reference == null)
            {
                if (referenceInfo != null && (node != rootNode || referenceInfo.ReferenceType == typeof(ReferenceEnumerable)))
                    throw new QuantumConsistencyException("Content with a reference", "Content without reference", node);

                var memberCount = descriptor.Members.Count();
                if (node.Children.Count != memberCount)
                    throw new QuantumConsistencyException("A node with [{0}] children", memberCount.ToStringSafe(), "A node with [{0}] children", node.Children.Count.ToStringSafe(), node);

                PushContextNode(node);
                base.VisitObject(obj, descriptor, true);
                PopContextNode();
            }
            else
            {
                if (referenceInfo == null)
                    throw new QuantumConsistencyException("Content without reference", "Content with a reference", node);
                if (node.Content.Reference.GetType() != referenceInfo.ReferenceType)
                    throw new QuantumConsistencyException("Content with a [{0}]", referenceInfo.ReferenceType.Name, "Content with a [{0}]", node.Content.Reference.GetType().Name, node);
                if (!Equals(node.Content.Value, obj))
                    throw new QuantumConsistencyException("The node content value [{0}]", obj.ToStringSafe(), "The node content value [{0}]", node.Content.Value.ToStringSafe(), node);
                if (node.Children.Count > 0)
                    throw new QuantumConsistencyException("A node with a reference and no child", null, "A node with a reference and [{0}] children", node.Children.Count.ToStringSafe(), node);

                AddReference(node, node.Content.Reference);
            }
        }
        /// <inheritdoc/>
        public override void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
        {
            var node = GetContextNode();
            GraphNode child;
            try
            {
                child = (GraphNode)node.Children.Single(x => x.Name == member.Name);
            }
            catch (InvalidOperationException)
            {
                throw new QuantumConsistencyException("A single child node [{0}]", member.Name, "No child or multiple children [{0}]", member.Name, node);
            }

            if (!IsPrimitiveType(child.Content.Type))
            {
                PushContextNode(child);
                Visit(value);
                PopContextNode();
            }
        }

        /// <inheritdoc/>
        public override void VisitCollection(IEnumerable collection, CollectionDescriptor descriptor)
        {
            var containerNode = GetContextNode();

            var count = descriptor.GetCollectionCount(collection);
            var referenceInfo = GetReferenceInfo(descriptor.Type, collection);

            if (referenceInfo != null && referenceInfo.EnumerableCount != count)
                throw new QuantumConsistencyException("A node with an EnumerableReference containing [{0}] items", referenceInfo.EnumerableCount.ToStringSafe(), "A node with an EnumerableReference containing [{0}] items", count.ToStringSafe(), containerNode);

            if (IsPrimitiveType(descriptor.ElementType, false) || IsEnumerable(descriptor.ElementType))
            {
                base.VisitCollection(collection, descriptor);
            }
        }

        /// <inheritdoc/>
        public override void VisitDictionary(object dictionary, DictionaryDescriptor descriptor)
        {
            var containerNode = GetContextNode();

            if (!IsPrimitiveType(descriptor.KeyType))
                throw new QuantumConsistencyException("A dictionary with a primary type for keys", null, "A dictionary [{0}] for keys", descriptor.KeyType.FullName, containerNode);

            // TODO: an access to the count function in DictionaryDescriptor
            var count = ((IEnumerable)dictionary).Cast<object>().Count();
            var referenceInfo = GetReferenceInfo(descriptor.Type, dictionary);
            if (referenceInfo != null && referenceInfo.EnumerableCount != count)
                throw new QuantumConsistencyException("A node with an EnumerableReference containing [{0}] items", referenceInfo.EnumerableCount.ToStringSafe(), "A node with an EnumerableReference containing [{0}] items", count.ToStringSafe(), containerNode);

            if (IsPrimitiveType(descriptor.ValueType, false) || IsEnumerable(descriptor.ValueType))
            {
                base.VisitDictionary(dictionary, descriptor);
            }
        }

        private ReferenceInfo GetReferenceInfo(Type type, object value)
        {
            // Is it a reference?
            if (!type.IsClass && (type.IsStruct() || IsPrimitiveType(type)))
                return null;

            var descriptor = value != null ? TypeDescriptorFactory.Find(value.GetType()) : null;
            var valueType = GetElementValueType(descriptor);

            // This is either an object reference or a enumerable reference of non-primitive type (excluding custom primitive type)
            if (valueType == null || (!type.IsStruct() && !IsPrimitiveType(valueType, false)))
            {
                var refType = Reference.GetReferenceType(value, Index.Empty);
                if (refType == typeof(ReferenceEnumerable))
                {
                    if (value == null) throw new InvalidOperationException("The value is not expected to be null when its node should contains an ReferenceEnumerable");
                    var enumerable = (IEnumerable)value;
                    return new ReferenceInfo(refType, enumerable.Cast<object>().Count());
                }
                return new ReferenceInfo(refType, -1);
            }
            return null;
        }

        private void AddReference(IGraphNode referencer, IReference reference)
        {
            var enumerableReference = reference as ReferenceEnumerable;
            if (enumerableReference != null)
            {
                foreach (var itemReference in enumerableReference)
                {
                    AddObjectReference(referencer, itemReference);
                }
            }
            else
            {
                AddObjectReference(referencer, (ObjectReference)reference);
            }
        }

        private void AddObjectReference(IGraphNode referencer, ObjectReference reference)
        {
            if (reference.TargetNode == null)
                throw new QuantumConsistencyException("A resolved reference", "An unresolved reference", referencer);

            if (referencer.Content.Reference == reference && !Equals(referencer.Content.Value, reference.TargetNode.Content.Value))
                throw new QuantumConsistencyException("Referenced node with same content value that its referencer", "Referenced node with different content value that its referencer", referencer);

            if (reference.TargetGuid != reference.TargetNode.Guid)
                throw new QuantumConsistencyException("Referenced node with same Guid that the reference", "Referenced node with different Guid that the reference", referencer);

            references.Enqueue(reference);
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

        private static bool IsEnumerable(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        private bool IsPrimitiveType(Type type, bool includeAdditionalPrimitiveTypes = true)
        {
            //if (type == null)
                return false;

            //return type.IsPrimitive || type == typeof(string) || type.IsEnum || (includeAdditionalPrimitiveTypes && PrimitiveTypes.Any(x => x.IsAssignableFrom(type)));
        }

        private static Type GetElementValueType(ITypeDescriptor descriptor)
        {
            var dictionaryDescriptor = descriptor as DictionaryDescriptor;
            var collectionDescriptor = descriptor as CollectionDescriptor;
            if (dictionaryDescriptor != null)
            {
                return dictionaryDescriptor.ValueType;
            }
            return collectionDescriptor?.ElementType;
        }

    }
}
