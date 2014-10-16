// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    /// <summary>
    /// An implementation of <see cref="IContent"/> that gives access to an object or a boxed struct.
    /// </summary>
    /// <remarks>This content is not serialized by default.</remarks>
    public class ObjectContent : ContentBase
    {
        private object value;
        private bool updated;

        public override bool IsReadOnly { get { return false; } }

        public ObjectContent(object value, Type type, ITypeDescriptor descriptor)
            : base(type, descriptor, false)
        {
            SerializeFlags = ViewModelContentSerializeFlags.None;
            this.value = value;
            updated = true;
        }

        public override object Value
        {
            get { return value; }
            set
            {
                this.value = value;
                updated = true;
            }
        }

        public sealed override object UpdatedValue
        {
            get
            {
                if (updated)
                {
                    updated = false;
                    return Value;
                }
                return ValueNotUpdated;
            }
        }
    }
}