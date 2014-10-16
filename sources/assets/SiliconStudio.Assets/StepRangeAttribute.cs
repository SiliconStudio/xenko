// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// This attribute allows to define boundaries for a numeric property, and advice small and large increment values for the user interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class StepRangeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepRangeAttribute"/> using <see cref="double"/> values.
        /// </summary>
        /// <param name="minimum">The minimum accepted value for the associated property.</param>
        /// <param name="maximum">The maximum accepted value for the associated property.</param>
        public StepRangeAttribute(double minimum, double maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepRangeAttribute"/> using <see cref="double"/> values.
        /// </summary>
        /// <param name="minimum">The minimum accepted value for the associated property.</param>
        /// <param name="maximum">The maximum accepted value for the associated property.</param>
        /// <param name="smallStep">The adviced increment value in case of a small change for the associated property.</param>
        /// <param name="largeStep">The adviced increment value in case of a large change for the associated property.</param>
        public StepRangeAttribute(double minimum, double maximum, double smallStep, double largeStep)
        {
            Minimum = minimum;
            Maximum = maximum;
            SmallStep = smallStep;
            LargeStep = largeStep;
        }

        /// <summary>
        /// Gets or sets the minimum accepted value for the associated property.
        /// </summary>
        public object Minimum { get; set; }

        /// <summary>
        /// Gets or sets the maximum accepted value for the associated property.
        /// </summary>
        public object Maximum { get; set; }

        /// <summary>
        /// Gets or sets the adviced increment value in case of a small change for the associated property.
        /// </summary>
        public object SmallStep { get; set; }

        /// <summary>
        /// Gets or sets the adviced increment value in case of a large change for the associated property.
        /// </summary>
        public object LargeStep { get; set; }
    }
}
