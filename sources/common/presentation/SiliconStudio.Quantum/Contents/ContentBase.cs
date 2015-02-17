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
        private readonly IReference reference;

        protected ContentBase(ITypeDescriptor descriptor, bool isPrimitive, IReference reference)
        {
            if (descriptor == null) throw new ArgumentNullException("descriptor");
            this.reference = reference;
            Descriptor = descriptor;
            IsPrimitive = isPrimitive;
            SerializeFlags = ViewModelContentSerializeFlags.SerializeValue;
            ShouldProcessReference = true;
        }

        /// <inheritdoc/>
        public Type Type { get { return Descriptor.Type; } }

        /// <inheritdoc/>
        public abstract object Value { get; set; }

        /// <inheritdoc/>
        public bool IsPrimitive { get; private set; }

        /// <inheritdoc/>
        public ITypeDescriptor Descriptor { get; private set; }

        /// <inheritdoc/>
        public bool IsReference { get { return Reference != null; } }

        /// <inheritdoc/>
        public IReference Reference { get { return reference; } }

        /// <inheritdoc/>
        public bool ShouldProcessReference { get; internal set; }

        /// <inheritdoc/>
        public virtual ViewModelContentState LoadState { get; set; }

        /// <inheritdoc/>
        public ViewModelContentFlags Flags { get; set; }

        /// <inheritdoc/>
        public ViewModelContentSerializeFlags SerializeFlags { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "[" + GetType().Name + "]: " + Value;
        }
    }
}
