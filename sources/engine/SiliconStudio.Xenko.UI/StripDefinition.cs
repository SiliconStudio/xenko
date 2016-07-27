// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Represents the definition of a grid strip.
    /// </summary>
    [DataContract(nameof(StripDefinition))]
    [Display(Expand = ExpandRule.Never)]
    public class StripDefinition
    {
        private float maximumSize = float.PositiveInfinity;
        private float minimumSize;
        private StripType type = StripType.Star;
        private float sizeValue = 1.0f;

        /// <summary>
        /// The actual size of the strip in virtual pixels.
        /// </summary>
        [DataMemberIgnore]
        public float ActualSize { get; internal set; }

        /// <summary>
        /// Creates a 1-Star sized strip definition.
        /// </summary>
        public StripDefinition()
        {
        }

        /// <summary>
        /// Creates a <see cref="StripDefinition"/> with the provided size and type.
        /// </summary>
        /// <param name="type">The type of the strip to create</param>
        /// <param name="sizeValue">The value of the strip to create</param>
        public StripDefinition(StripType type, float sizeValue = 1.0f)
        {
            Type = type;
            SizeValue = sizeValue;
        }

        /// <summary>
        /// The maximum size of the strip in virtual pixels.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The provided value is negative.</exception>
        /// <exception cref="InvalidOperationException">The provided value is smaller than <see cref="MinimumSize"/></exception>
        /// <userdoc>The maximum size of the strip in virtual pixels.</userdoc>
        [DataMember]
        [DataMemberRange(0.0f, float.PositiveInfinity)]
        [DefaultValue(float.PositiveInfinity)]
        public float MaximumSize
        {
            get { return maximumSize; }
            set
            {
                if(value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                maximumSize = value;

                DefinitionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The minimum size of the strip in virtual pixels.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The provided value is negative or infinity.</exception>
        /// <exception cref="InvalidOperationException">The provided value is bigger than <see cref="MaximumSize"/></exception>
        /// <userdoc>The minimum size of the strip in virtual pixels.</userdoc>
        [DataMember]
        [DataMemberRange(0.0f, float.MaxValue)]
        [DefaultValue(0)]
        public float MinimumSize
        {
            get { return minimumSize; }
            set
            {
                if (value < 0 || float.IsPositiveInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value));

                minimumSize = value;

                DefinitionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the type of the strip.
        /// </summary>
        /// <userdoc>The type of the strip.</userdoc>
        [DataMember]
        [DefaultValue(StripType.Star)]
        public StripType Type
        {
            get { return type; }
            set
            {
                if(type == value)
                    return;

                type = value;
                
                DefinitionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the size value of the strip. 
        /// Note that the value is interpreted differently depending on the strip <see cref="Type"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The size must be finite positive value.</exception>
        /// <userdoc>The size value of the strip.</userdoc>
        [DataMember]
        [DataMemberRange(0.0f, float.MaxValue)]
        [DefaultValue(1.0f)]
        public float SizeValue
        {
            get { return sizeValue; }
            set
            {
                if (value < 0 || float.IsPositiveInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value));

                sizeValue = value;
                
                DefinitionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        internal event EventHandler<EventArgs> DefinitionChanged;

        /// <summary>
        /// Clamp the provided size by the definition's minimum and maximum values.
        /// </summary>
        /// <param name="desiredSize">The size to clamp</param>
        /// <returns>The size clamped by the minimum and maximum values of the strip definition</returns>
        public float ClampSizeByMinimumMaximum(float desiredSize)
        {
            CheckState();
            return Math.Min(MaximumSize, Math.Max(MinimumSize, desiredSize));
        }

        /// <summary>
        /// Verifies that <see cref="MinimumSize"/> if smaller than or equal to <see cref="MaximumSize"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="MinimumSize"/> is greater than <see cref="MaximumSize"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CheckState()
        {
            if (MinimumSize > MaximumSize)
                throw new InvalidOperationException($"The value of {nameof(MinimumSize)} is greater than the value of {nameof(MaximumSize)}.");
        }

        internal float ValueRelativeMinimum()
        {
            CheckState();
            if (sizeValue < MathUtil.ZeroTolerance)
                return 0;
            return MinimumSize / SizeValue;
        }

        internal float ValueRelativeMaximum()
        {
            CheckState();
            if (sizeValue < MathUtil.ZeroTolerance)
                return 0;
            return MaximumSize / SizeValue;
        }

        internal class SortByIncreasingStarRelativeMinimumValues : IComparer<StripDefinition>
        {
            public int Compare(StripDefinition def1, StripDefinition def2)
            {
                var val1 = def1.ValueRelativeMinimum();
                var val2 = def2.ValueRelativeMinimum();

                return val1.CompareTo(val2);
            }
        }

        internal class SortByIncreasingStarRelativeMaximumValues : IComparer<StripDefinition>
        {
            public int Compare(StripDefinition def1, StripDefinition def2)
            {
                var val1 = def1.ValueRelativeMaximum();
                var val2 = def2.ValueRelativeMaximum();

                return val1.CompareTo(val2);
            }
        }
    }
}
