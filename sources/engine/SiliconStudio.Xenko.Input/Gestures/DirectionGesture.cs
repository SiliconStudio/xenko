// Copyright (c) 2016 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Base class for direction gestures
    /// </summary>
    [DataContract]
    public abstract class DirectionGesture : InputGesture
    {
        /// <summary>
        /// Should the direction be inverted
        /// </summary>
        public bool Inverted = false;

        private Vector2 lastState;
        
        public event EventHandler<DirectionGestureEventArgs> Changed;
        
        protected void UpdateDirection(Vector2 newState, IInputDevice sourceDevice)
        {
            newState = GetScaledOutput(newState);
            if (newState != lastState)
            {
                lastState = newState;
                Changed?.Invoke(this, new DirectionGestureEventArgs(sourceDevice, lastState));
            }
        }

        /// <summary>
        /// Returns the input value with <see cref="Inverted"/> applied to it
        /// </summary>
        /// <param name="v">the input axis value</param>
        /// <returns>The scaled output value</returns>
        protected Vector2 GetScaledOutput(Vector2 v)
        {
            return Inverted ? -v : v;
        }

        public override string ToString()
        {
            return $"{nameof(Inverted)}: {Inverted}";
        }
    }
}