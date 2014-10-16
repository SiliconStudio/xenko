// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.Legacy.Contents
{
    /// <summary>
    /// An implementation of <see cref="IViewModelContent"/> that holds a value type element.
    /// </summary>
    public sealed class ValueViewModelContent : ContentBase
    {
        public override object Value
        {
            get
            {
                return value;
            }
            set
            {
                if (!Type.IsInstanceOfType(value))
                    throw new InvalidOperationException("The value must be of the same type that the initial value.");

                this.value = value;
            }
        }

        private object value;

        public override bool IsReadOnly { get { return false; } }

        public ValueViewModelContent(object value)
            : base(value.GetType(), null, false)
        {
            if (!value.GetType().IsValueType && value.GetType() != typeof(string))
                throw new InvalidOperationException("The value to set must be a value-type.");

            this.value = value;
        }
    }

}
