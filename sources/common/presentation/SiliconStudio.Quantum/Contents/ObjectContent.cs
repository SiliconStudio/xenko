// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// An implementation of <see cref="IContent"/> that gives access to an object or a boxed struct.
    /// </summary>
    /// <remarks>This content is not serialized by default.</remarks>
    public class ObjectContent : ContentBase
    {
        private object value;

        public ObjectContent(object value, ITypeDescriptor descriptor, bool isPrimitive, IReference reference)
            : base(descriptor, isPrimitive, reference)
        {
            this.value = value;
        }

        public override object Value { get { return value; } set { throw new InvalidOperationException("An ObjectContent value cannot be modified after it has been constructed"); } }

        protected void SetValue(object newValue)
        {
            value = newValue;
        }
    }
}