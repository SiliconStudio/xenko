// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// A base abstract implementation of the <see cref="IContent"/> interface.
    /// </summary>
    public abstract class ContentBase : IContent
    {
        protected ContentBase(ITypeDescriptor descriptor, bool isPrimitive, IReference reference)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            Reference = reference;
            Descriptor = descriptor;
            IsPrimitive = isPrimitive;
            ShouldProcessReference = true;
        }

        /// <inheritdoc/>
        public IContentNode OwnerNode { get; private set; }

        /// <inheritdoc/>
        public Type Type => Descriptor.Type;

        /// <inheritdoc/>
        public abstract object Value { get; }

        /// <inheritdoc/>
        public bool IsPrimitive { get; }

        /// <inheritdoc/>
        public ITypeDescriptor Descriptor { get; }

        /// <inheritdoc/>
        public bool IsReference => Reference != null;

        /// <inheritdoc/>
        public IReference Reference { get; }

        /// <inheritdoc/>
        public IEnumerable<object> Indices => GetIndices();

        /// <inheritdoc/>
        public bool ShouldProcessReference { get; internal set; }

        /// <inheritdoc/>
        public event EventHandler<ContentChangeEventArgs> PrepareChange;

        /// <inheritdoc/>
        public event EventHandler<ContentChangeEventArgs> FinalizeChange;

        /// <inheritdoc/>
        public event EventHandler<ContentChangeEventArgs> Changing;

        /// <inheritdoc/>
        public event EventHandler<ContentChangeEventArgs> Changed;

        /// <inheritdoc/>
        public virtual object Retrieve(object index)
        {
            return Retrieve(Value, index);
        }

        /// <inheritdoc/>
        public virtual object Retrieve(object value, object index)
        {
            if (index != null)
            {
                var collectionDescriptor = Descriptor as CollectionDescriptor;
                var dictionaryDescriptor = Descriptor as DictionaryDescriptor;
                if (collectionDescriptor != null)
                {
                    return collectionDescriptor.GetValue(value, (int)index);
                }
                if (dictionaryDescriptor != null)
                {
                    return dictionaryDescriptor.GetValue(value, index);
                }

                throw new NotSupportedException("Unable to get the node value, the collection is unsupported");
            }
            return value;
        }

        /// <inheritdoc/>
        public abstract void Update(object newValue, object index = null);

        /// <inheritdoc/>
        public abstract void Add(object newItem);

        /// <inheritdoc/>
        public abstract void Add(object itemIndex, object newItem);

        /// <inheritdoc/>
        public abstract void Remove(object itemIndex);

        /// <inheritdoc/>
        public override string ToString()
        {
            return "[" + GetType().Name + "]: " + Value;
        }

        internal void RegisterOwner(IContentNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (OwnerNode != null) throw new InvalidOperationException("An owner node has already been registered for this content.");
            OwnerNode = node;
        }

        /// <summary>
        /// Raises the <see cref="Changing"/> event with the given parameters.
        /// </summary>
        /// <param name="index">The index where the change occurred, if applicable. <c>null</c> otherwise.</param>
        /// <param name="changeType">The type of change that occurred.</param>
        /// <param name="oldValue">The old value of this content.</param>
        /// <param name="newValue">The new value of this content.</param>
        protected void NotifyContentChanging(object index, ContentChangeType changeType, object oldValue, object newValue)
        {
            var args = new ContentChangeEventArgs(this, index, changeType, oldValue, newValue);
            PrepareChange?.Invoke(this, args);
            Changing?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Changed"/> event with the given parameters.
        /// </summary>
        /// <param name="index">The index where the change occurred, if applicable. <c>null</c> otherwise.</param>
        /// <param name="changeType">The type of change that occurred.</param>
        /// <param name="oldValue">The old value of this content.</param>
        /// <param name="newValue">The new value of this content.</param>
        protected void NotifyContentChanged(object index, ContentChangeType changeType, object oldValue, object newValue)
        {
            var args = new ContentChangeEventArgs(this, index, changeType, oldValue, newValue);
            Changed?.Invoke(this, args);
            FinalizeChange?.Invoke(this, args);
        }

        private IEnumerable<object> GetIndices()
        {
            var enumRef = Reference as ReferenceEnumerable;
            if (enumRef != null)
                return enumRef.Indices;

            var collectionDescriptor = Descriptor as CollectionDescriptor;
            if (collectionDescriptor != null)
            {
                return Enumerable.Range(0, collectionDescriptor.GetCollectionCount(Value)).Cast<object>();
            }
            var dictionaryDescriptor = Descriptor as DictionaryDescriptor;
            return dictionaryDescriptor?.GetKeys(Value).Cast<object>();
        }
    }
}
