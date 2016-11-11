// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// A virtual axis that is calculated from a negative and positive button
    /// </summary>
    [DataContract]
    public class TwoWayGesture : ScalableInputGesture, IAxisGesture
    {
        private IButtonGesture positive;
        private IButtonGesture negative;

        /// <summary>
        /// The button that generates a positive (1) axis value
        /// </summary>
        public IButtonGesture Positive
        {
            get { return positive; }
            set
            {
                UpdateChild(positive, value);
                positive = value;
            }
        }

        /// <summary>
        /// The button that generates a negative (-1) axis value
        /// </summary>
        public IButtonGesture Negative
        {
            get { return negative; }
            set
            {
                UpdateChild(negative, value);
                negative = value;
            }
        }
        
        /// <inheritdoc />
        /// <remarks>
        /// Calculates positive * 1.0f + negative * -1.0f so that both buttons cancel each other out when pressed at the same time. This value is scaled by delta time since it uses keyboard input.
        /// </remarks>
        [DataMemberIgnore]
        public float Axis => GetScaledOutput((Positive?.Button ?? false ? 1.0f : 0.0f) + (Negative?.Button ?? false ? -1.0f : 0.0f), true);

        public override void Reset(TimeSpan elapsedTime)
        {
            base.Reset(elapsedTime);
            positive?.Reset(elapsedTime);
            negative?.Reset(elapsedTime);
        }

        public override string ToString()
        {
            return $"{nameof(Axis)}: {Axis}, {nameof(Inverted)}: {Inverted}, {nameof(Sensitivity)}: {Sensitivity}";
        }
    }
}