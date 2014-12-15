// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Core
{
    /// <summary>
    /// Defines range values for a property or field.
    /// </summary>
    /// <remarks><see cref="Minimum"/>, <see cref="Maximum"/> and <see cref="Step"/> must have the same type</remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DataRangeAttribute : Attribute
    {
        private readonly object minimum;
        private readonly object maximum;
        private readonly object step;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRangeAttribute" /> class.
        /// </summary>
        /// <param name="minimum">The minimum.</param>
        /// <param name="maximum">The maximum.</param>
        /// <param name="step">The minimum step used to go from minimum to maximum.</param>
        public DataRangeAttribute(object minimum, object maximum, object step)
        {
            this.minimum = minimum;
            this.maximum = maximum;
            this.step = step;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRangeAttribute"/> class.
        /// </summary>
        /// <param name="minimum">The minimum.</param>
        /// <param name="maximum">The maximum.</param>
        public DataRangeAttribute(object minimum, object maximum)
            : this(minimum, maximum, null)
        {
        }

        /// <summary>
        /// Gets the minimum inclusive.
        /// </summary>
        /// <value>The minimum.</value>
        public object Minimum
        {
            get { return minimum; }
        }

        /// <summary>
        /// Gets the maximum inclusive.
        /// </summary>
        /// <value>The maximum.</value>
        public object Maximum
        {
            get { return maximum; }
        }

        public object Step
        {
            get { return step; }
        }
    }
}