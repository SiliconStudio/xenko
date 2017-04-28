// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Base class for axis gestures
    /// </summary>
    [DataContract]
    public abstract class AxisGestureBase : InputGestureBase, IAxisGesture
    {
        /// <summary>
        /// Should the axis be inverted
        /// </summary>
        public bool Inverted = false;

        private float lastState;
        
        public event EventHandler<AxisGestureEventArgs> Changed;
        
        protected void UpdateAxis(float newState, IInputDevice sourceDevice)
        {
            newState = GetScaledOutput(newState);
            if (newState != lastState)
            {
                lastState = newState;
                Changed?.Invoke(this, new AxisGestureEventArgs(sourceDevice, lastState));
            }
        }

        /// <summary>
        /// Returns the input value with <see cref="Inverted"/> applied to it
        /// </summary>
        /// <param name="v">the input axis value</param>
        /// <returns>The scaled output value</returns>
        protected float GetScaledOutput(float v)
        {
            return Inverted ? -v : v;
        }
        
        public override string ToString()
        {
            return $"{nameof(Inverted)}: {Inverted}";
        }
    }
}