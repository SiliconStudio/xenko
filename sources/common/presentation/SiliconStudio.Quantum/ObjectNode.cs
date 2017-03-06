// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;
using System.Reflection;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// An implementation of <see cref="IGraphNode"/> that gives access to an object or a boxed struct.
    /// </summary>
    /// <remarks>This content is not serialized by default.</remarks>
    public class ObjectNode : GraphNodeBase, IInitializingObjectNode, IGraphNodeInternal
    {
        private readonly HybridDictionary<string, IMemberNode> childrenMap = new HybridDictionary<string, IMemberNode>();
        private readonly List<IMemberNode> children = new List<IMemberNode>();
        private object value;

        public ObjectNode([NotNull] INodeBuilder nodeBuilder, object value, Guid guid, ITypeDescriptor descriptor, bool isPrimitive, IReference reference)
            : base(nodeBuilder.SafeArgument(nameof(nodeBuilder)).NodeContainer, guid, descriptor, isPrimitive)
        {
            if (reference is ObjectReference)
                throw new ArgumentException($"An {nameof(ObjectNode)} cannot contain an {nameof(ObjectReference)}");
            this.value = value;
            ItemReferences = reference as ReferenceEnumerable;
        }

        /// <inheritdoc/>
        [NotNull]
        public IMemberNode this[string name] => childrenMap[name];

        /// <inheritdoc/>
        public IReadOnlyCollection<IMemberNode> Members => children;

        /// <inheritdoc/>
        public IEnumerable<Index> Indices => GetIndices();

        /// <inheritdoc/>
        public bool IsEnumerable => Descriptor is CollectionDescriptor || Descriptor is DictionaryDescriptor;

        /// <inheritdoc/>
        public override bool IsReference => ItemReferences != null;

        /// <inheritdoc/>
        public ReferenceEnumerable ItemReferences { get; }

        /// <inheritdoc/>
        protected sealed override object Value => value;

        /// <inheritdoc/>
        public event EventHandler<INodeChangeEventArgs> PrepareChange;

        /// <inheritdoc/>
        public event EventHandler<INodeChangeEventArgs> FinalizeChange;

        /// <inheritdoc/>
        public event EventHandler<ItemChangeEventArgs> ItemChanging;

        /// <inheritdoc/>
        public event EventHandler<ItemChangeEventArgs> ItemChanged;

        /// <inheritdoc/>
        [CanBeNull]
        public IMemberNode TryGetChild([NotNull] string name)
        {
            IMemberNode child;
            childrenMap.TryGetValue(name, out child);
            return child;
        }

        /// <inheritdoc/>
        public IObjectNode IndexedTarget(Index index)
        {
            if (index == Index.Empty) throw new ArgumentException(@"index cannot be Index.Empty when invoking this method.", nameof(index));
            if (ItemReferences == null) throw new InvalidOperationException(@"The node does not contain enumerable references.");
            return ItemReferences[index].TargetNode;
        }

        /// <inheritdoc/>
        public void Update(object newValue, Index index)
        {
            Update(newValue, index, true);
        }

        /// <inheritdoc/>
        public void Add(object newItem)
        {
            var collectionDescriptor = Descriptor as CollectionDescriptor;
            if (collectionDescriptor != null)
            {
                // Some collection (such as sets) won't add item at the end but at an arbitrary location.
                // Better send a null index in this case than sending a wrong value.
                var value = Value;
                var index = collectionDescriptor.IsList ? new Index(collectionDescriptor.GetCollectionCount(value)) : Index.Empty;
                var args = new ItemChangeEventArgs(this, index, ContentChangeType.CollectionAdd, null, newItem);
                NotifyItemChanging(args);
                collectionDescriptor.Add(value, newItem);
                // TODO: fixme
                //if (value.GetType().GetTypeInfo().IsValueType)
                //{
                //    var containerValue = Parent.Retrieve();
                //    MemberDescriptor.Set(containerValue, value);
                //}
                UpdateReferences();
                NotifyItemChanged(args);
            }
            else
                throw new NotSupportedException("Unable to set the node value, the collection is unsupported");
        }

        /// <inheritdoc/>
        public void Add(object newItem, Index itemIndex)
        {
            var collectionDescriptor = Descriptor as CollectionDescriptor;
            var dictionaryDescriptor = Descriptor as DictionaryDescriptor;
            if (collectionDescriptor != null)
            {
                var index = collectionDescriptor.IsList ? itemIndex : Index.Empty;
                var value = Value;
                var args = new ItemChangeEventArgs(this, index, ContentChangeType.CollectionAdd, null, newItem);
                NotifyItemChanging(args);
                if (collectionDescriptor.GetCollectionCount(value) == itemIndex.Int || !collectionDescriptor.HasInsert)
                {
                    collectionDescriptor.Add(value, newItem);
                }
                else
                {
                    collectionDescriptor.Insert(value, itemIndex.Int, newItem);
                }
                // TODO: fixme
                //if (value.GetType().GetTypeInfo().IsValueType)
                //{
                //    var containerValue = Parent.Retrieve();
                //    MemberDescriptor.Set(containerValue, value);
                //}
                UpdateReferences();
                NotifyItemChanged(args);
            }
            else if (dictionaryDescriptor != null)
            {
                var args = new ItemChangeEventArgs(this, itemIndex, ContentChangeType.CollectionAdd, null, newItem);
                NotifyItemChanging(args);
                var value = Value;
                dictionaryDescriptor.AddToDictionary(value, itemIndex.Value, newItem);
                // TODO: fixme
                //if (value.GetType().GetTypeInfo().IsValueType)
                //{
                //    var containerValue = Parent.Retrieve();
                //    MemberDescriptor.Set(containerValue, value);
                //}
                UpdateReferences();
                NotifyItemChanged(args);
            }
            else
                throw new NotSupportedException("Unable to set the node value, the collection is unsupported");

        }

        /// <inheritdoc/>
        public void Remove(object item, Index itemIndex)
        {
            if (itemIndex.IsEmpty) throw new ArgumentException(@"The given index should not be empty.", nameof(itemIndex));
            var args = new ItemChangeEventArgs(this, itemIndex, ContentChangeType.CollectionRemove, item, null);
            NotifyItemChanging(args);
            var collectionDescriptor = Descriptor as CollectionDescriptor;
            var dictionaryDescriptor = Descriptor as DictionaryDescriptor;
            var value = Value;
            if (collectionDescriptor != null)
            {
                if (collectionDescriptor.HasRemoveAt)
                {
                    collectionDescriptor.RemoveAt(value, itemIndex.Int);
                }
                else
                {
                    collectionDescriptor.Remove(value, item);
                }
            }
            else if (dictionaryDescriptor != null)
            {
                dictionaryDescriptor.Remove(value, itemIndex.Value);
            }
            else
                throw new NotSupportedException("Unable to set the node value, the collection is unsupported");

            // TODO: fixme
            //if (value.GetType().GetTypeInfo().IsValueType)
            //{
            //    var containerValue = Parent.Retrieve();
            //    MemberDescriptor.Set(containerValue, value);
            //}
            UpdateReferences();
            NotifyItemChanged(args);
        }

        /// <inheritdoc/>
        protected internal override void UpdateFromMember(object newValue, Index index)
        {
            if (index == Index.Empty)
            {
                throw new InvalidOperationException("An ObjectNode value cannot be modified after it has been constructed");
            }
            Update(newValue, index, true);
        }

        protected void SetValue(object newValue)
        {
            value = newValue;
        }

        protected void NotifyItemChanging(ItemChangeEventArgs args)
        {
            PrepareChange?.Invoke(this, args);
            ItemChanging?.Invoke(this, args);
        }

        protected void NotifyItemChanged(ItemChangeEventArgs args)
        {
            ItemChanged?.Invoke(this, args);
            FinalizeChange?.Invoke(this, args);
        }

        private void Update(object newValue, Index index, bool sendNotification)
        {
            if (index == Index.Empty)
                throw new ArgumentException("index cannot be empty.");
            var oldValue = Retrieve(index);
            ItemChangeEventArgs itemArgs = null;
            if (sendNotification)
            {
                itemArgs = new ItemChangeEventArgs(this, index, ContentChangeType.CollectionUpdate, oldValue, newValue);
                NotifyItemChanging(itemArgs);
            }
            var collectionDescriptor = Descriptor as CollectionDescriptor;
            var dictionaryDescriptor = Descriptor as DictionaryDescriptor;
            if (collectionDescriptor != null)
            {
                collectionDescriptor.SetValue(Value, index.Int, newValue);
            }
            else if (dictionaryDescriptor != null)
            {
                dictionaryDescriptor.SetValue(Value, index.Value, newValue);
            }
            else
                throw new NotSupportedException("Unable to set the node value, the collection is unsupported");

            UpdateReferences();
            if (sendNotification)
            {
                NotifyItemChanged(itemArgs);
            }
        }


        private void UpdateReferences()
        {
            NodeContainer?.UpdateReferences(this);
        }

        private IEnumerable<Index> GetIndices()
        {
            var enumRef = ItemReferences;
            if (enumRef != null)
                return enumRef.Indices;

            return GetIndices(this);
        }

        public override string ToString()
        {
            return $"{{Node: Object {Type.Name} = [{Value}]}}";
        }

        /// <inheritdoc/>
        void IInitializingObjectNode.AddMember(IMemberNode member, bool allowIfReference)
        {
            if (isSealed)
                throw new InvalidOperationException("Unable to add a child to a GraphNode that has been sealed");

            // ReSharper disable once HeuristicUnreachableCode - this code is reachable only at the specific moment we call this method!
            if (ItemReferences != null && !allowIfReference)
                throw new InvalidOperationException("A GraphNode cannot have children when its content hold a reference.");

            children.Add(member);
            childrenMap.Add(member.Name, (MemberNode)member);
        }
    }
}
