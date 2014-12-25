// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    /// <summary>
    /// Base class for a constant value for <see cref="Materials.MaterialComputeColor"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MaterialValueComputeColor<T> : MaterialKeyedComputeColor
    {
        private T value;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialValueComputeColor{T}"/> class.
        /// </summary>
        protected MaterialValueComputeColor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialValueComputeColor{T}"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        protected MaterialValueComputeColor(T value) : this()
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
