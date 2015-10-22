// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Input
{
    /// <summary>
    /// Event class for the LongPress gesture.
    /// </summary>
    public sealed class GestureEventLongPress : GestureEvent
    {
        /// <summary>
        /// The position (in pixels) where the LongPress gesture happened.
        /// </summary>
        public Vector2 Position { get; internal set; }

        internal GestureEventLongPress(int numberOfFinger, TimeSpan time, Vector2 position)
        {
            State = GestureState.Occurred;
            Type = GestureType.LongPress;
            NumberOfFinger = numberOfFinger;
            DeltaTime = time;
            TotalTime = time;
            Position = position;
        }
    }
}