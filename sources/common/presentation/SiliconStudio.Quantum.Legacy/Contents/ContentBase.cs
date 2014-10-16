// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    public abstract class ContentBase : IContent
    {
        // TODO: improve this.
        public static readonly object ValueNotUpdated = new object();

        private object associatedData;

        protected ContentBase(Type type, ITypeDescriptor descriptor, bool isReference)
        {
            if (type == null) throw new ArgumentNullException("type");
            Descriptor = descriptor;
            Type = type;
            SerializeFlags = ViewModelContentSerializeFlags.SerializeValue;
        }

        public virtual bool IsReadOnly { get; set; }

        /// <inheritdoc/>
        public virtual IViewModelNode OwnerNode { get; set; }

        /// <inheritdoc/>
        public Type Type { get; set; }

        /// <inheritdoc/>
        public abstract object Value { get; set; }

        /// <inheritdoc/>
        public ITypeDescriptor Descriptor { get; private set; }

        /// <inheritdoc/>
        // TODO: this should be abstract and has no setter here
        public virtual object UpdatedValue { get { throw new NotImplementedException("TODO: this property should be abstract"); } }

        /// <inheritdoc/>
        public object AssociatedData { get { return associatedData; } set { if (associatedData != null) throw new InvalidOperationException("AssociatedData already set."); associatedData = value; } }

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