// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// A direction gesture generated from 2 <see cref="AxisGesture"/>s (X and Y axis)
    /// </summary>
    [DataContract]
    [Display("Axes to Direction")]
    public class FourWayGesture : DirectionGesture
    {
        private AxisGesture y;
        private AxisGesture x;

        private Vector2 state;

        /// <summary>
        /// The source for the X-axis component of the generated direction
        /// </summary>
        public AxisGesture X
        {
            get { return x; }
            set
            {
                if (x != null)
                {
                    x.Changed -= XChanged;
                    RemoveChild(x);
                }
                x = value;
                if (x != null)
                {
                    AddChild(x);
                    x.Changed += XChanged;
                }
            }
        }

        /// <summary>
        /// The source for the Y-axis component of the generated direction
        /// </summary>
        public AxisGesture Y
        {
            get { return y; }
            set
            {
                if (y != null)
                {
                    y.Changed -= YChanged;
                    RemoveChild(y);
                }
                y = value;
                if (y != null)
                {
                    AddChild(y);
                    y.Changed += YChanged;
                }
            }
        }

        private void XChanged(object sender, AxisGestureEventArgs args)
        {
            state.X = args.State;
            UpdateDirection(state, args.Device);
        }

        private void YChanged(object sender, AxisGestureEventArgs args)
        {
            state.Y = args.State;
            UpdateDirection(state, args.Device);
        }
    }
}