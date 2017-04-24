// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// Base class for a constant value for <see cref="ComputeNode"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract(Inherited = true)]
    public abstract class ComputeValueBase<T> : ComputeKeyedBase
    {
        private T value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeValueBase{T}"/> class.
        /// </summary>
        protected ComputeValueBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeValueBase{T}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        protected ComputeValueBase(T value) : this()
        {
            Value = value;
        }

        /// <summary>
        /// The property to access the internal value
        /// </summary>
        /// <userdoc>
        /// The value.
        /// </userdoc>
        [DataMember(20)]
        [InlineProperty]
        public T Value
        {
            get
            {
                return value;
            }
            set
            {
                if (!value.Equals(this.value))
                {
                    this.value = value;
                }
            }
        }
    }
}
