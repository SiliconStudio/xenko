// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Paradox.Rendering.Materials.ComputeColors
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
        /// The default value.
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
