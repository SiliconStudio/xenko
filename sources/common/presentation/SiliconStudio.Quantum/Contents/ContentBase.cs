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
        public abstract object Value { get; set; }

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
        public event EventHandler<ContentChangedEventArgs> Changed;

        /// <inheritdoc/>
        public override string ToString()
        {
            return "[" + GetType().Name + "]: " + Value;
        }

        /// <summary>
        /// Raises the <see cref="Changed"/> event with the given parameters.
        /// </summary>
        /// <param name="oldValue">The old value of this content.</param>
        /// <param name="newValue">The new value of this content.</param>
        protected void NotifyContentChanged(object oldValue, object newValue)
        {
            Changed?.Invoke(this, new ContentChangedEventArgs(this, oldValue, newValue));
        }
    }
}
