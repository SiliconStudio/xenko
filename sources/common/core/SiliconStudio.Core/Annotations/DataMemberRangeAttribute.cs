// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Core.Annotations
{
    /// <summary>
    /// Defines range values for a property or field.
    /// </summary>
    /// <remarks><see cref="Minimum"/>, <see cref="Maximum"/> and <see cref="SmallStep"/> must have the same type</remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DataMemberRangeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataMemberRangeAttribute" /> class.
        /// </summary>
        /// <param name="minimum">The minimum.</param>
        /// <param name="maximum">The maximum.</param>
        /// <param name="smallStep">The minimum step used to go from minimum to maximum.</param>
        /// <param name="largeStep">The maximum step.</param>
        public DataMemberRangeAttribute(double minimum, double maximum, double smallStep, double largeStep)
        {
            if (double.IsNaN(minimum)) throw new ArgumentOutOfRangeException(nameof(minimum));
            if (double.IsNaN(maximum)) throw new ArgumentOutOfRangeException(nameof(maximum));
            if (minimum > maximum) throw new ArgumentException("The minimum should be lesser or equal to the maximum.", nameof(minimum));
            if (double.IsNaN(smallStep)) throw new ArgumentOutOfRangeException(nameof(smallStep));
            if (double.IsNaN(largeStep)) throw new ArgumentOutOfRangeException(nameof(largeStep));
            if (smallStep > largeStep) throw new ArgumentException("The smallStep should be lesser or equal to the largeStep.", nameof(smallStep));

            Minimum = minimum;
            Maximum = maximum;
            SmallStep = smallStep;
            LargeStep = largeStep;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMemberRangeAttribute" /> class.
        /// </summary>
        /// <param name="minimum">The minimum.</param>
        /// <param name="maximum">The maximum.</param>
        public DataMemberRangeAttribute(double minimum, double maximum)
        {
            if (double.IsNaN(minimum)) throw new ArgumentOutOfRangeException(nameof(minimum));
            if (double.IsNaN(maximum)) throw new ArgumentOutOfRangeException(nameof(maximum));

            Minimum = minimum;
            Maximum = maximum;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMemberRangeAttribute" /> class.
        /// </summary>
        /// <param name="minimum">The minimum.</param>
        /// <param name="maximum">The maximum.</param>
        /// <param name="smallStep">The minimum step used to go from minimum to maximum.</param>
        /// <param name="largeStep">The maximum step.</param>
        /// <param name="decimalPlaces">The decimal places</param>
        public DataMemberRangeAttribute(double minimum, double maximum, double smallStep, double largeStep, int decimalPlaces)
        {
            if (double.IsNaN(minimum)) throw new ArgumentOutOfRangeException(nameof(minimum));
            if (double.IsNaN(maximum)) throw new ArgumentOutOfRangeException(nameof(maximum));
            if (minimum > maximum) throw new ArgumentException("The minimum should be lesser or equal to the maximum.", nameof(minimum));
            if (double.IsNaN(smallStep)) throw new ArgumentOutOfRangeException(nameof(smallStep));
            if (double.IsNaN(largeStep)) throw new ArgumentOutOfRangeException(nameof(largeStep));
            if (smallStep > largeStep) throw new ArgumentException("The smallStep should be lesser or equal to the largeStep.", nameof(smallStep));

            Minimum = minimum;
            Maximum = maximum;
            SmallStep = smallStep;
            LargeStep = largeStep;
            DecimalPlaces = decimalPlaces;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMemberRangeAttribute" /> class.
        /// </summary>
        /// <param name="minimum">The minimum.</param>
        /// <param name="maximum">The maximum.</param>
        /// <param name="decimalPlaces">The decimal places</param>
        public DataMemberRangeAttribute(double minimum, double maximum, int decimalPlaces)
        {
            if (double.IsNaN(minimum)) throw new ArgumentOutOfRangeException(nameof(minimum));
            if (double.IsNaN(maximum)) throw new ArgumentOutOfRangeException(nameof(maximum));
            if (minimum > maximum) throw new ArgumentException("The minimum should be lesser or equal to the maximum.", nameof(minimum));

            Minimum = minimum;
            Maximum = maximum;
            DecimalPlaces = decimalPlaces;
        }

        /// <summary>
        /// Gets or sets a Boolean value indicating whether <see cref="double.NaN"/> is an allowed value in the range.
        /// </summary>
        /// <returns><c>true</c> if <see cref="double.NaN"/> is allowed; otherwise, <c>false</c>. The default is <c>false</c>.</returns>
        public bool AllowNaN { get; set; }

        /// <summary>
        /// Gets the minimum inclusive.
        /// </summary>
        /// <value>The minimum.</value>
        public double? Minimum { get; }

        /// <summary>
        /// Gets the maximum inclusive.
        /// </summary>
        /// <value>The maximum.</value>
        public double? Maximum { get; }

        /// <summary>
        /// Gets the minimum step.
        /// </summary>
        /// <value>The minimum step.</value>
        public double? SmallStep { get; }

        /// <summary>
        /// Gets the maximum step.
        /// </summary>
        /// <value>The maximum step.</value>
        public double? LargeStep { get; }

        /// <summary>
        /// Gets the decimal places.
        /// </summary>
        /// <value>The decimal places.</value>
        public int? DecimalPlaces { get; }
    }
}
