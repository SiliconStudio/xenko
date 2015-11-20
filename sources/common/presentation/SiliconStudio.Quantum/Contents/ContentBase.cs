// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

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
        public bool ShouldProcessReference { get; internal set; }

        /// <inheritdoc/>
        public event EventHandler<ContentChangeEventArgs> Changing;

        /// <inheritdoc/>
        public event EventHandler<ContentChangeEventArgs> Changed;

        /// <inheritdoc/>
        public virtual object Retrieve(object index)
        {
            if (index != null)
            {
                var collectionDescriptor = Descriptor as CollectionDescriptor;
                var dictionaryDescriptor = Descriptor as DictionaryDescriptor;
                if (collectionDescriptor != null)
                {
                    return collectionDescriptor.GetValue(Value, (int)index);
                }
                if (dictionaryDescriptor != null)
                {
                    return dictionaryDescriptor.GetValue(Value, index);
                }

                throw new NotSupportedException("Unable to get the node value, the collection is unsupported");
            }
            return Value;
        }

        /// <inheritdoc/>
        public abstract void Update(object newValue, object index);

        /// <inheritdoc/>
        public override string ToString()
        {
            return "[" + GetType().Name + "]: " + Value;
        }

        /// <summary>
        /// Raises the <see cref="Changing"/> event with the given parameters.
        /// </summary>
        /// <param name="index">The index where the change occurred, if applicable. <c>null</c> otherwise.</param>
        /// <param name="oldValue">The old value of this content.</param>
        /// <param name="newValue">The new value of this content.</param>
        protected void NotifyContentChanging(object index, object oldValue, object newValue)
        {
            Changing?.Invoke(this, new ContentChangeEventArgs(this, index, oldValue, newValue));
        }

        /// <summary>
        /// Raises the <see cref="Changed"/> event with the given parameters.
        /// </summary>
        /// <param name="index">The index where the change occurred, if applicable. <c>null</c> otherwise.</param>
        /// <param name="oldValue">The old value of this content.</param>
        /// <param name="newValue">The new value of this content.</param>
        protected void NotifyContentChanged(object index, object oldValue, object newValue)
        {
            Changed?.Invoke(this, new ContentChangeEventArgs(this, index, oldValue, newValue));
        }
    }
}
