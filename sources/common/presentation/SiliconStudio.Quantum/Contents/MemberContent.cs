// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using System.Reflection;

using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// An implementation of <see cref="IContent"/> that gives access to a member of an object.
    /// </summary>
    public class MemberContent : ContentBase
    {
        private readonly NodeContainer nodeContainer;

        public MemberContent(INodeBuilder nodeBuilder, IContent container, IMemberDescriptor member, bool isPrimitive, IReference reference)
            : base(nodeBuilder.TypeDescriptorFactory.Find(member.Type), isPrimitive, reference)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            Member = member;
            Container = container;
            nodeContainer = nodeBuilder.NodeContainer;
        }

        /// <summary>
        /// The <see cref="IMemberDescriptor"/> used to access the member of the container represented by this content.
        /// </summary>
        public IMemberDescriptor Member { get; protected set; }

        /// <summary>
        /// Gets the name of the node holding this content.
        /// </summary>
        public string Name => OwnerNode?.Name;

        /// <summary>
        /// Gets the container content of this member content.
        /// </summary>
        public IContent Container { get; }

        /// <inheritdoc/>
        public sealed override object Value { get { if (Container.Value == null) throw new InvalidOperationException("Container's value is null"); return Member.Get(Container.Value); } }

        /// <inheritdoc/>
        public override void Update(object newValue, object index = null)
        {
            var oldValue = Retrieve(index);
            NotifyContentChanging(index, ContentChangeType.ValueChange, oldValue, newValue);
            if (index != null)
            {
                var collectionDescriptor = Descriptor as CollectionDescriptor;
                var dictionaryDescriptor = Descriptor as DictionaryDescriptor;
                if (collectionDescriptor != null)
                {
                    collectionDescriptor.SetValue(Value, (int)index, newValue);
                }
                else if (dictionaryDescriptor != null)
                {
                    dictionaryDescriptor.SetValue(Value, index, newValue);
                }
                else
                    throw new NotSupportedException("Unable to set the node value, the collection is unsupported");
            }
            else
            {
                if (Container.Value == null) throw new InvalidOperationException("Container's value is null");
                var containerValue = Container.Value;
                Member.Set(containerValue, newValue);

                if (Container.Value.GetType().GetTypeInfo().IsValueType)
                    Container.Update(containerValue);
            }
            UpdateReferences();
            NotifyContentChanged(index, ContentChangeType.ValueChange, oldValue, newValue);
        }

        public override void Add(object newItem)
        {
            var collectionDescriptor = Descriptor as CollectionDescriptor;
            if (collectionDescriptor != null)
            {
                var index = collectionDescriptor.GetCollectionCount(Value);
                NotifyContentChanging(index, ContentChangeType.CollectionAdd, null, newItem);
                collectionDescriptor.Add(Value, newItem);

                UpdateReferences();
                NotifyContentChanged(index, ContentChangeType.CollectionAdd, null, newItem);
            }
            else
                throw new NotSupportedException("Unable to set the node value, the collection is unsupported");
        }

        public override void Add(object itemIndex, object newItem)
        {
            NotifyContentChanging(itemIndex, ContentChangeType.CollectionAdd, null, newItem);
            var collectionDescriptor = Descriptor as CollectionDescriptor;
            var dictionaryDescriptor = Descriptor as DictionaryDescriptor;
            if (collectionDescriptor != null)
            {
                var index = (int)itemIndex;
                if (collectionDescriptor.GetCollectionCount(Value) == index || !collectionDescriptor.HasInsert)
                {
                    collectionDescriptor.Add(Value, newItem);
                }
                else
                {
                    collectionDescriptor.Insert(Value, index, newItem);
                }
            }
            else if (dictionaryDescriptor != null)
            {
                dictionaryDescriptor.SetValue(Value, itemIndex, newItem);
            }
            else
                throw new NotSupportedException("Unable to set the node value, the collection is unsupported");

            UpdateReferences();
            NotifyContentChanged(itemIndex, ContentChangeType.CollectionAdd, null, newItem);
        }

        public override void Remove(object itemIndex, object item)
        {
            if (itemIndex == null) throw new ArgumentNullException(nameof(itemIndex));
            NotifyContentChanging(itemIndex, ContentChangeType.CollectionRemove, item, null);
            var collectionDescriptor = Descriptor as CollectionDescriptor;
            var dictionaryDescriptor = Descriptor as DictionaryDescriptor;
            if (collectionDescriptor != null)
            {
                if (collectionDescriptor.HasRemoveAt)
                {
                    var index = (int)itemIndex;
                    collectionDescriptor.RemoveAt(Value, index);                  
                }
                else
                {
                    collectionDescriptor.Remove(Value, item);
                }               
            }
            else if (dictionaryDescriptor != null)
            {
                dictionaryDescriptor.Remove(Value, itemIndex);
            }
            else
                throw new NotSupportedException("Unable to set the node value, the collection is unsupported");

            UpdateReferences();
            NotifyContentChanged(itemIndex, ContentChangeType.CollectionRemove, item, null);
        }

        private void UpdateReferences()
        {
            var graphNode = OwnerNode as IGraphNode;
            if (graphNode != null)
            {
                nodeContainer?.UpdateReferences(graphNode);
            }
        }
    }
}
