// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A virtual axis that is calculated from a negative and positive button
    /// </summary>
    [DataContract]
    [Display("Positive & Negative Axes")]
    public class TwoWayGesture : AxisGestureBase
    {
        private IButtonGesture positive;
        private IButtonGesture negative;

        private float stateA;
        private float stateB;

        /// <summary>
        /// The button that generates a positive (1) axis value
        /// </summary>
        public IButtonGesture Positive
        {
            get { return positive; }
            set
            {
                if (positive != null)
                {
                    positive.Changed -= PositiveChanged;
                    RemoveChild(positive);
                }
                positive = value;
                if (positive != null)
                {
                    AddChild(positive);
                    positive.Changed += PositiveChanged;
                }
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
                if (negative != null)
                {
                    negative.Changed -= NegativeChanged;
                    RemoveChild(negative);
                }
                negative = value;
                if (negative != null)
                {
                    AddChild(negative);
                    negative.Changed += NegativeChanged;
                }
            }
        }

        private void PositiveChanged(object sender, ButtonGestureEventArgs args)
        {
            stateA = args.State == ButtonState.Down ? 1.0f : 0.0f;
            UpdateAxis(stateA + stateB, args.Device);
        }
        private void NegativeChanged(object sender, ButtonGestureEventArgs args)
        {
            stateB = args.State == ButtonState.Down ? -1.0f : 0.0f;
            UpdateAxis(stateA + stateB, args.Device);
        }
    }
}