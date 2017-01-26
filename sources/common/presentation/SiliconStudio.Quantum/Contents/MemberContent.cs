// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using System.Reflection;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// An implementation of <see cref="IContentNode"/> that gives access to a member of an object.
    /// </summary>
    public class MemberContent : ContentNode, IMemberNodeInternal
    {
        private readonly NodeContainer nodeContainer;

        public MemberContent([NotNull] INodeBuilder nodeBuilder, Guid guid, [NotNull] IObjectNode parent, [NotNull] IMemberDescriptor memberDescriptor, bool isPrimitive, IReference reference)
            : base(guid, nodeBuilder.TypeDescriptorFactory.Find(memberDescriptor.Type), isPrimitive, reference)
        {
            if (nodeBuilder == null) throw new ArgumentNullException(nameof(nodeBuilder));
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (memberDescriptor == null) throw new ArgumentNullException(nameof(memberDescriptor));
            Parent = parent;
            MemberDescriptor = memberDescriptor;
            Name = memberDescriptor.Name;
            nodeContainer = nodeBuilder.NodeContainer;
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IObjectNode Parent { get; }

        /// <summary>
        /// The <see cref="IMemberDescriptor"/> used to access the member of the container represented by this content.
        /// </summary>
        public IMemberDescriptor MemberDescriptor { get; protected set; }

        /// <inheritdoc/>
        [Obsolete("Use method Retrieve()")]
        public sealed override object Value { get { if (Parent.Value == null) throw new InvalidOperationException("Container's value is null"); return MemberDescriptor.Get(Parent.Value); } }

        /// <inheritdoc/>
        public IObjectNode Target { get { if (TargetReference == null) throw new InvalidOperationException("This node does not contain an ObjectReference"); return TargetReference.TargetNode; } }

        /// <inheritdoc/>
        public event EventHandler<MemberNodeChangeEventArgs> PrepareChange;

        /// <inheritdoc/>
        public event EventHandler<MemberNodeChangeEventArgs> FinalizeChange;

        /// <inheritdoc/>
        public event EventHandler<MemberNodeChangeEventArgs> Changing;

        /// <inheritdoc/>
        public event EventHandler<MemberNodeChangeEventArgs> Changed;

        /// <inheritdoc/>
        public override void Update(object newValue, Index index)
        {
            Update(newValue, index, true);
        }

        /// <inheritdoc/>
        public override void Add(object newItem)
        {
            var collectionDescriptor = Descriptor as CollectionDescriptor;
            if (collectionDescriptor != null)
            {
                // Some collection (such as sets) won't add item at the end but at an arbitrary location.
                // Better send a null index in this case than sending a wrong value.
                var value = Value;
                var index = collectionDescriptor.IsList ? new Index(collectionDescriptor.GetCollectionCount(value)) : Index.Empty;
                var args = new MemberNodeChangeEventArgs(this, index, ContentChangeType.CollectionAdd, null, newItem);
                NotifyContentChanging(args);
                collectionDescriptor.Add(value, newItem);
                if (value.GetType().GetTypeInfo().IsValueType)
                {
                    var containerValue = Parent.Value;
                    MemberDescriptor.Set(containerValue, value);
                }
                UpdateReferences();
                NotifyContentChanged(args);
            }
            else
                throw new NotSupportedException("Unable to set the node value, the collection is unsupported");
        }

        /// <inheritdoc/>
        public override void Add(object newItem, Index itemIndex)
        {
            var collectionDescriptor = Descriptor as CollectionDescriptor;
            var dictionaryDescriptor = Descriptor as DictionaryDescriptor;
            if (collectionDescriptor != null)
            {
                var index = collectionDescriptor.IsList ? itemIndex : Index.Empty;
                var value = Value;
                var args = new MemberNodeChangeEventArgs(this, index, ContentChangeType.CollectionAdd, null, newItem);
                NotifyContentChanging(args);
                if (collectionDescriptor.GetCollectionCount(value) == itemIndex.Int || !collectionDescriptor.HasInsert)
                {
                    collectionDescriptor.Add(value, newItem);
                }
                else
                {
                    collectionDescriptor.Insert(value, itemIndex.Int, newItem);
                }
                if (value.GetType().GetTypeInfo().IsValueType)
                {
                    var containerValue = Parent.Value;
                    MemberDescriptor.Set(containerValue, value);
                }
                UpdateReferences();
                NotifyContentChanged(args);
            }
            else if (dictionaryDescriptor != null)
            {
                var args = new MemberNodeChangeEventArgs(this, itemIndex, ContentChangeType.CollectionAdd, null, newItem);
                NotifyContentChanging(args);
                var value = Value;
                dictionaryDescriptor.AddToDictionary(value, itemIndex.Value, newItem);
                if (value.GetType().GetTypeInfo().IsValueType)
                {
                    var containerValue = Parent.Value;
                    MemberDescriptor.Set(containerValue, value);
                }
                UpdateReferences();
                NotifyContentChanged(args);
            }
            else
                throw new NotSupportedException("Unable to set the node value, the collection is unsupported");

        }

        /// <inheritdoc/>
        public override void Remove(object item, Index itemIndex)
        {
            if (itemIndex.IsEmpty) throw new ArgumentException(@"The given index should not be empty.", nameof(itemIndex));
            var args = new MemberNodeChangeEventArgs(this, itemIndex, ContentChangeType.CollectionRemove, item, null);
            NotifyContentChanging(args);
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

            if (value.GetType().GetTypeInfo().IsValueType)
            {
                var containerValue = Parent.Value;
                MemberDescriptor.Set(containerValue, value);
            }
            UpdateReferences();
            NotifyContentChanged(args);
        }

        /// <summary>
        /// Raises the <see cref="Changing"/> event with the given parameters.
        /// </summary>
        /// <param name="args">The arguments of the event.</param>
        protected void NotifyContentChanging(MemberNodeChangeEventArgs args)
        {
            PrepareChange?.Invoke(this, args);
            Changing?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Changed"/> event with the given arguments.
        /// </summary>
        /// <param name="args">The arguments of the event.</param>
        protected void NotifyContentChanged(MemberNodeChangeEventArgs args)
        {
            Changed?.Invoke(this, args);
            FinalizeChange?.Invoke(this, args);
        }

        protected internal override void UpdateFromMember(object newValue, Index index)
        {
            Update(newValue, index, false);
        }

        private void Update(object newValue, Index index, bool sendNotification)
        {
            var oldValue = Retrieve(index);
            MemberNodeChangeEventArgs args = null;
            if (sendNotification)
            {
                if (index == Index.Empty)
                    args = new MemberNodeChangeEventArgs(this, index, ContentChangeType.ValueChange, oldValue, newValue);
                else
                    args = new MemberNodeChangeEventArgs(this, index, ContentChangeType.CollectionUpdate, oldValue, newValue);

                NotifyContentChanging(args);
            }

            if (!index.IsEmpty)
            {
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
            }
            else
            {
                if (Parent.Value == null) throw new InvalidOperationException("Container's value is null");
                var containerValue = Parent.Value;
                MemberDescriptor.Set(containerValue, newValue);

                if (Parent.Value.GetType().GetTypeInfo().IsValueType)
                    ((ContentNode)Parent).UpdateFromMember(containerValue, Index.Empty);
            }
            UpdateReferences();
            if (sendNotification)
            {
                NotifyContentChanged(args);
            }
        }

        private void UpdateReferences()
        {
            nodeContainer?.UpdateReferences(this);
        }

        public override string ToString()
        {
            return $"{{Node: Member {Name} = [{Value}]}}";
        }
    }
}
