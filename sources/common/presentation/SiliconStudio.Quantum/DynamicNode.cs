// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    public abstract class DynamicNode : DynamicObject, IEnumerable
    {
        protected readonly IGraphNode Node;

        internal DynamicNode(IGraphNode node)
        {
            Node = node;
        }

        /// <summary>
        /// Creates a dynamic node from the given <see cref="IGraphNode"/>.
        /// </summary>
        /// <param name="node">The node to use to create the dynamic node.</param>
        /// <returns>A <see cref="DynamicNode"/> representing the given node.</returns>
        public static dynamic FromNode(IGraphNode node)
        {
            if (node.Content is MemberContent)
                throw new ArgumentException("Cannot create a dynamic node from a member node.");

            return new DynamicDirectNode(node);
        }

        /// <summary>
        /// Returns the <see cref="IGraphNode"/> associated to the given dynamic node.
        /// </summary>
        /// <param name="node">The node from which to retrieve the graph node.</param>
        /// <returns>A <see cref="IGraphNode"/> associated to the given node.</returns>
        public static IGraphNode GetNode(DynamicNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            return node.Node;
        }

        /// <summary>
        /// Returns the <see cref="IGraphNode"/> associated to the given dynamic node.
        /// </summary>
        /// <returns>A <see cref="IGraphNode"/> associated to the given node.</returns>
        public IGraphNode GetNode() => Node;

        /// <summary>
        /// Adds an item to the content of this node, assuming it's a collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(object item)
        {
            var targetNode = GetTargetNode();
            if (targetNode == null)
                throw new InvalidOperationException($"Cannot invoke {nameof(Add)} on this property.");
            targetNode.Content.Add(item);
        }

        /// <summary>
        /// Inserts an item to the content of this node, assuming it's a collection.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="index">The index of the item to insert.</param>
        public void Insert(object item, Index index)
        {
            var targetNode = GetTargetNode();
            if (targetNode == null)
                throw new InvalidOperationException($"Cannot invoke {nameof(Insert)} on this property.");
            targetNode.Content.Add(item, index);
        }

        /// <summary>
        /// Removes an item from the content if this node, assuming it's a collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="index">The index of the item to remove.</param>
        public void Remove(object item, Index index)
        {
            var targetNode = GetTargetNode();
            if (targetNode == null)
                throw new InvalidOperationException($"Cannot invoke {nameof(Remove)} on this property.");
            targetNode.Content.Remove(item, index);
        }

        /// <summary>
        /// Retrieves the actual value of this node.
        /// </summary>
        /// <returns>The actual value of this node.</returns>
        public object Retrieve() => RetrieveValue();

        /// <summary>
        /// Retrieves the actual value of this node.
        /// </summary>
        /// <typeparam name="T">The type of expected value</typeparam>
        /// <returns>The actual value of this node.</returns>
        /// <exception cref="InvalidCastException">The actual type of the node value does not match the given <typeparamref name="T"/> type.</exception>
        public T Retrieve<T>() => (T)Retrieve();

        /// <inheritdoc/>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var memberNode = GetTargetMemberNode(binder.Name);
            result = memberNode != null ? new DynamicDirectNode(memberNode) : null;
            return result != null;
        }

        /// <inheritdoc/>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var memberNode = GetTargetMemberNode(binder.Name);
            try
            {
                // TODO: "changing" notifications will still be sent even if the update fails (but not the "changed") - we should detect preemptively if we can update (implements a bool TryUpdate?)
                memberNode.Content.Update(value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            var value = RetrieveValue();
            if (value == null && !binder.Type.IsValueType)
            {
                result = null;
                return true;
            }
            if (binder.Type.IsInstanceOfType(value))
            {
                result = value;
                return true;
            }
            result = null;
            return false;
        }

        /// <inheritdoc/>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return GetTargetNode()?.Children.Select(x => x.Name) ?? Enumerable.Empty<string>();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            var node = GetTargetNode();
            var indices = GetAllIndices(Node);
            if (indices == null)
                throw new InvalidOperationException("This node is not enumerable.");

            dynamic thisNode = this;
            return indices.Cast<object>().Select(x => thisNode[x]).GetEnumerator();
        }

        protected IGraphNode GetTargetMemberNode(string memberName)
        {
            var targetNode = GetTargetNode();
            var memberNode = targetNode?.Children.FirstOrDefault(x => x.Name == memberName);
            return memberNode;
        }

        protected abstract object RetrieveValue();

        protected abstract IGraphNode GetTargetNode();

        protected static IEnumerable GetAllIndices(IGraphNode node)
        {
            if (node.Content.IsReference)
            {
                var reference = node.Content.Reference as ReferenceEnumerable;
                return reference?.Indices.Select(x => x.Value);
            }
            var value = node.Content.Retrieve();
            var collectionDescriptor = node.Content.Descriptor as CollectionDescriptor;
            if (collectionDescriptor != null)
            {
                var count = collectionDescriptor.GetCollectionCount(value);
                return Enumerable.Range(0, count);
            }
            var dictionaryDescriptor = node.Content.Descriptor as DictionaryDescriptor;
            return dictionaryDescriptor?.GetKeys(value).Cast<object>();
        }

        protected static bool IsIndexExisting(IGraphNode node, Index index)
        {
            if (node.Content.IsReference)
            {
                var reference = node.Content.Reference as ReferenceEnumerable;
                if (reference?.Indices.Contains(index) ?? false)
                {
                    return true;
                }
            }
            else
            {
                var value = node.Content.Retrieve();
                var collectionDescriptor = node.Content.Descriptor as CollectionDescriptor;
                if (collectionDescriptor != null && index.IsInt && index.Int >= 0 && index.Int < collectionDescriptor.GetCollectionCount(value))
                {
                    return true;
                }
                var dictionaryDescriptor = node.Content.Descriptor as DictionaryDescriptor;
                if (dictionaryDescriptor != null && dictionaryDescriptor.KeyType.IsInstanceOfType(index.Value) && dictionaryDescriptor.ContainsKey(value, index.Value))
                {
                    return true;
                }
            }
            return false;
        }

        protected static bool IsIndexValid(IGraphNode node, Index index)
        {
            if (node.Content.IsReference)
            {
                var reference = node.Content.Reference as ReferenceEnumerable;
                return reference != null;
            }
            var collectionDescriptor = node.Content.Descriptor as CollectionDescriptor;
            if (collectionDescriptor != null)
            {
                return index.IsInt && index.Int >= 0;
            }
            var dictionaryDescriptor = node.Content.Descriptor as DictionaryDescriptor;
            if (dictionaryDescriptor != null)
            {
                return dictionaryDescriptor.KeyType.IsInstanceOfType(index.Value);
            }
            return false;
        }

        protected static bool UpdateCollection(IGraphNode node, object value, Index index)
        {
            if (IsIndexExisting(node, index))
            {
                node.Content.Update(value, index);
                return true;
            }
            if (IsIndexValid(node, index))
            {
                node.Content.Add(value, index);
                return true;
            }
            return false;
        }
    }

    internal class DynamicDirectNode : DynamicNode
    {
        internal DynamicDirectNode(IGraphNode node)
            : base(node)
        {
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes.Length == 1)
            {
                var index = new Index(indexes[0]);
                if (IsIndexExisting(Node, index))
                {
                    result = new DynamicIndexedNode(Node, index);
                    return true;
                }
            }
            result = null;
            return false;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes.Length == 1)
            {
                var index = new Index(indexes[0]);
                return UpdateCollection(Node, value, index);
            }
            return false;
        }

        protected override object RetrieveValue()
        {
            return Node.Content.Retrieve();
        }

        protected override IGraphNode GetTargetNode()
        {
            var objectReference = Node.Content.Reference as ObjectReference;
            if (Node.Content.IsReference && objectReference != null)
            {
                return objectReference.TargetNode;
            }
            return Node;
        }
    }

    internal class DynamicIndexedNode : DynamicNode
    {
        private readonly Index index;

        internal DynamicIndexedNode(IGraphNode node, Index index)
            : base(node)
        {
            this.index = index;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var targetNode = GetTargetNode();
            if (indexes.Length == 1 && targetNode != null)
            {
                var nextIndex = new Index(indexes[0]);
                if (IsIndexExisting(targetNode, nextIndex))
                {
                    result = new DynamicIndexedNode(targetNode, nextIndex);
                    return true;
                }
            }
            result = null;
            return false;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            var targetNode = GetTargetNode();
            if (indexes.Length == 1 && targetNode != null)
            {
                var nextIndex = new Index(indexes[0]);
                return UpdateCollection(Node, value, nextIndex);
            }
            return false;
        }

        protected override object RetrieveValue()
        {
            return Node.Content.Retrieve(index);
        }

        protected override IGraphNode GetTargetNode()
        {
            var reference = Node.Content.Reference as ReferenceEnumerable;
            if (Node.Content.IsReference && (reference?.Indices.Contains(index) ?? false))
            {
                return reference[index].TargetNode;
            }
            return null;
        }
    }
}
